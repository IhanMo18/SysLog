using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SysLog.Service.Interfaces;

namespace SysLog.Domine.Services
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

            // Use the configured connection string for the SysLog database. The
            // previous code attempted to read a non-existent
            // "Database:ConnectionStrings:PostgresConnection" key which resulted
            // in an empty connection string and an InvalidOperationException.
            string connectionString = _configuration.GetConnectionString("SysLogDb")!;

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

                // 1) Leer todas las tablas public
                var tableNames = new List<string>();
                await using (var cmd = new NpgsqlCommand(
                    @"SELECT table_name 
                      FROM information_schema.tables 
                      WHERE table_schema='public' 
                        AND table_type='BASE TABLE';",
                    conn))
                await using (var rdr = await cmd.ExecuteReaderAsync())
                    while (await rdr.ReadAsync())
                        tableNames.Add(rdr.GetString(0));

                // 2) Para cada tabla, leer columnas y construir CREATE TABLE sin relaciones
                var tableColumns = new Dictionary<string, List<string>>();
                var identityCols  = new Dictionary<string, HashSet<string>>();

                foreach (var table in tableNames)
                {
                    var cols = new List<string>();
                    var idents = new HashSet<string>();

                    await using (var cmd = new NpgsqlCommand(@"
                        SELECT column_name, data_type,
                               is_nullable, character_maximum_length,
                               numeric_precision, numeric_scale,
                               is_identity
                        FROM information_schema.columns
                        WHERE table_schema='public' 
                          AND table_name = @tbl;",
                        conn))
                    {
                        cmd.Parameters.AddWithValue("tbl", table);
                        await using var rdr = await cmd.ExecuteReaderAsync();

                        while (await rdr.ReadAsync())
                        {
                            string name     = rdr.GetString(0);
                            string dtype    = rdr.GetString(1);
                            bool nullable   = rdr.GetString(2) == "YES";
                            var maxLen      = rdr["character_maximum_length"];
                            var prec        = rdr["numeric_precision"];
                            var scale       = rdr["numeric_scale"];
                            bool isIdentity = rdr.GetString(6) == "YES";

                            string sqlType = dtype switch
                            {
                                "character varying" => $"VARCHAR({(maxLen is DBNull ? "255" : maxLen)})",
                                "character" => $"CHAR({(maxLen is DBNull ? "1" : maxLen)})",
                                "numeric"   => $"NUMERIC({prec}, {scale})",
                                _ => dtype.ToUpper()
                            };

                            if (isIdentity)
                            {
                                cols.Add($@"""{name}"" SERIAL PRIMARY KEY");
                                idents.Add(name);
                            }
                            else
                            {
                                cols.Add($@"""{name}"" {sqlType} {(nullable ? "NULL" : "NOT NULL")}");
                            }
                        }
                    }

                    tableColumns[table] = cols;
                    identityCols[table]  = idents;

                    // Escribimos el CREATE TABLE básico
                    await writer.WriteLineAsync($@"CREATE TABLE IF NOT EXISTS ""{table}"" (");
                    await writer.WriteLineAsync("    " + string.Join(",\n    ", cols));
                    await writer.WriteLineAsync(");");
                    await writer.WriteLineAsync();
                }

                // 3) Leer relaciones FK
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
                      AND tc.table_schema = 'public';
                ", conn))
                await using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        string name    = rdr.GetString(0);
                        string fkTbl   = rdr.GetString(1);
                        string fkCol   = rdr.GetString(2);
                        string pkTbl   = rdr.GetString(3);
                        string pkCol   = rdr.GetString(4);

                        fkConstraints.Add($@"
ALTER TABLE ""{fkTbl}""
  ADD CONSTRAINT ""{name}""
  FOREIGN KEY (""{fkCol}"") 
  REFERENCES ""{pkTbl}""(""{pkCol}"")
  ON UPDATE CASCADE
  ON DELETE SET NULL;");
                    }
                }

                // 4) Deshabilitar chequeo de FK antes de INSERTs
                await writer.WriteLineAsync("SET session_replication_role = replica;\n");

                // 5) INSERTs por tabla
                foreach (var table in tableNames)
                {
                    await using var insCmd = new NpgsqlCommand($@"SELECT * FROM ""{table}"";", conn);
                    await using var rdr = await insCmd.ExecuteReaderAsync();

                    while (await rdr.ReadAsync())
                    {
                        var colList = new List<string>();
                        var valList = new List<string>();

                        for (int i = 0; i < rdr.FieldCount; i++)
                        {
                            var colName = rdr.GetName(i);
                            if (identityCols[table].Contains(colName))
                                continue; // omitimos identities

                            colList.Add($@"""{colName}""");
                            var v = rdr.IsDBNull(i)
                                ? "NULL"
                                : $"'{rdr.GetValue(i).ToString()!.Replace("'", "''")}'";
                            valList.Add(v);
                        }

                        if (colList.Any())
                        {
                            string insertSql = $@"INSERT INTO ""{table}"" 
  ({string.Join(", ", colList)})
  VALUES ({string.Join(", ", valList)});";
                            await writer.WriteLineAsync(insertSql);
                        }
                    }

                    await writer.WriteLineAsync();
                }

                // 6) Reactivar chequeo de FK
                await writer.WriteLineAsync("SET session_replication_role = DEFAULT;\n");

                // 7) Escribir las relaciones
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
