using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SysLog.Service.Interfaces;

namespace SysLog.Repository.Service
{
    public class PostgreSqlServerBackup : IBackup
    {
        private readonly ILogger<PostgreSqlServerBackup> _logger;
        private readonly IConfiguration _configuration;

        public PostgreSqlServerBackup(IConfiguration configuration, ILogger<PostgreSqlServerBackup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> BackupAsync()
        {
            string backupPath = _configuration["Database:BackupPath"]!;
            string databaseName = _configuration["Database:Name"]!;
            string schemaName = _configuration["Database:Schema"] ?? "public";

            string connectionString = _configuration.GetConnectionString("SysLogDb")!;
            var csBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrWhiteSpace(databaseName))
                csBuilder.Database = databaseName;
            connectionString = csBuilder.ConnectionString;

            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            string backupFile = Path.Combine(
                backupPath,
                $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{databaseName}.sql"
            );

            await using var writer = new StreamWriter(backupFile, false, Encoding.UTF8);

            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();

                // 1) Leer todas las tablas del esquema
                var tableNames = new List<string>();
                await using (var cmd = new NpgsqlCommand(
                    @"SELECT table_name
                      FROM information_schema.tables
                      WHERE table_schema = @schema
                        AND table_type = 'BASE TABLE';",
                    conn))
                {
                    cmd.Parameters.AddWithValue("schema", schemaName);
                    await using var rdr = await cmd.ExecuteReaderAsync();
                    while (await rdr.ReadAsync())
                        tableNames.Add(rdr.GetString(0));
                }

                // Excluir EF migrations e Identity tables
                var excludedTables = new[]
                {
                    "__EFMigrationsHistory",
                    "AspNetUsers",
                    "AspNetRoles",
                    "AspNetUserRoles",
                    "AspNetUserClaims",
                    "AspNetUserLogins",
                    "AspNetUserTokens",
                    "AspNetRoleClaims"
                };
                tableNames.RemoveAll(t => excludedTables.Contains(t));


                var identityCols = new Dictionary<string, HashSet<string>>();
                foreach (var table in tableNames)
                {
                    var columns = new List<string>();
                    var identities = new HashSet<string>();

                    await using (var cmd = new NpgsqlCommand(@"
                        SELECT column_name, data_type,
                               is_nullable, character_maximum_length,
                               numeric_precision, numeric_scale,
                               is_identity, column_default
                        FROM information_schema.columns
                        WHERE table_schema = @schema
                          AND table_name = @tbl
                        ORDER BY ordinal_position;", conn))
                    {
                        cmd.Parameters.AddWithValue("schema", schemaName);
                        cmd.Parameters.AddWithValue("tbl", table);
                        await using var rdr = await cmd.ExecuteReaderAsync();

                        while (await rdr.ReadAsync())
                        {
                            string colName = rdr.GetString(0);
                            string dtype = rdr.GetString(1).ToUpperInvariant();
                            bool nullable = rdr.GetString(2) == "YES";
                            var maxLen = rdr["character_maximum_length"];
                            var prec = rdr["numeric_precision"];
                            var scale = rdr["numeric_scale"];
                            bool isIdentity = rdr.GetString(6) == "YES";
                            var defaultValue = rdr["column_default"] as string;

                            string sqlType = dtype switch
                            {
                                "CHARACTER VARYING" => $"VARCHAR({(maxLen is DBNull ? "255" : maxLen)})",
                                "CHARACTER" => $"CHAR({(maxLen is DBNull ? "1" : maxLen)})",
                                "UUID" => "UUID",
                                "BOOLEAN" => "BOOLEAN",
                                "INTEGER" => "INTEGER",
                                "TEXT" => "TEXT",
                                "TIMESTAMP WITHOUT TIME ZONE" => "TIMESTAMP",
                                "TIMESTAMP WITH TIME ZONE" => "TIMESTAMPTZ",
                                "NUMERIC" => $"NUMERIC({prec},{scale})",
                                _ => dtype
                            };

                            if (isIdentity || (defaultValue != null && defaultValue.Contains("nextval")))
                            {
                                columns.Add($@"""{colName}"" SERIAL PRIMARY KEY");
                                identities.Add(colName);
                            }
                            else
                            {
                                string nullPart = nullable ? "NULL" : "NOT NULL";
                                string defPart = (defaultValue != null && !defaultValue.Contains("nextval")) ? $" DEFAULT {defaultValue}" : "";
                                columns.Add($@"""{colName}"" {sqlType} {nullPart}{defPart}".TrimEnd());
                            }
                        }
                    }
                    identityCols[table] = identities;

                    await writer.WriteLineAsync($@"CREATE TABLE ""{schemaName}"".""{table}"" (");
                    await writer.WriteLineAsync("    " + string.Join(",\n    ", columns));
                    await writer.WriteLineAsync(");");
                    await writer.WriteLineAsync();
                }


                var fkConstraints = new List<string>();
                await using (var cmd = new NpgsqlCommand(@"
                    SELECT
                        tc.constraint_name,
                        tc.table_name AS fk_table,
                        kcu.column_name AS fk_column,
                        ccu.table_name AS pk_table,
                        ccu.column_name AS pk_column
                    FROM
                        information_schema.table_constraints AS tc
                        JOIN information_schema.key_column_usage AS kcu
                            ON tc.constraint_name = kcu.constraint_name
                        JOIN information_schema.constraint_column_usage AS ccu
                            ON ccu.constraint_name = tc.constraint_name
                    WHERE tc.constraint_type = 'FOREIGN KEY'
                      AND tc.table_schema = @schema;", conn))
                {
                    cmd.Parameters.AddWithValue("schema", schemaName);
                    await using var rdr = await cmd.ExecuteReaderAsync();
                    while (await rdr.ReadAsync())
                    {
                        string name = rdr.GetString(0);
                        string fkTbl = rdr.GetString(1);
                        string fkCol = rdr.GetString(2);
                        string pkTbl = rdr.GetString(3);
                        string pkCol = rdr.GetString(4);

                        fkConstraints.Add($@"
ALTER TABLE ""{schemaName}"".""{fkTbl}""
    ADD CONSTRAINT ""{name}""
    FOREIGN KEY (""{fkCol}"")
    REFERENCES ""{schemaName}"".""{pkTbl}""(""{pkCol}"")
    ON UPDATE CASCADE
    ON DELETE SET NULL;");
                    }
                }


                await writer.WriteLineAsync("SET session_replication_role = replica;");
                await writer.WriteLineAsync();


                foreach (var table in tableNames)
                {
                    await using var insCmd = new NpgsqlCommand($@"SELECT * FROM ""{schemaName}"".""{table}"";", conn);
                    await using var rdr = await insCmd.ExecuteReaderAsync();

                    while (await rdr.ReadAsync())
                    {
                        var colList = new List<string>();
                        var valList = new List<string>();

                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            var colName = rdr.GetName(i);
                            if (identityCols[table].Contains(colName))
                                continue; // omitimos SERIALs

                            colList.Add($@"""{colName}""");
                            string v;
                            if (rdr.IsDBNull(i))
                                v = "NULL";
                            else
                            {
                                var raw = rdr.GetValue(i);
                                if (raw is DateTime dt)
                                    v = $"'{dt:yyyy-MM-dd HH:mm:ss}'";
                                else if (raw is DateTimeOffset dto)
                                    v = $"'{dto:yyyy-MM-dd HH:mm:sszzz}'";
                                else if (raw is bool b)
                                    v = b ? "TRUE" : "FALSE";
                                else
                                    v = $"'{raw.ToString()!.Replace("'", "''")}'";
                            }
                            valList.Add(v);
                        }

                        if (colList.Any())
                        {
                            string insertSql = $@"INSERT INTO ""{schemaName}"".""{table}""
({string.Join(", ", colList)})
VALUES ({string.Join(", ", valList)});
";
                            await writer.WriteLineAsync(insertSql);
                        }
                    }

                    await writer.WriteLineAsync();
                }


                await writer.WriteLineAsync("SET session_replication_role = DEFAULT;");
                await writer.WriteLineAsync();
                
                foreach (var fk in fkConstraints)
                    await writer.WriteLineAsync(fk + "\n");

                _logger.LogInformation("Backup completado: {File}", backupFile);
                return backupFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando backup");
                throw;
            }
        }
    }
}
