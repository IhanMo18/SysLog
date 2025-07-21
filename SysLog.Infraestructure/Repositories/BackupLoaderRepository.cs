using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Data.BackupDbContext;
using SysLog.Repository.Model;
using SysLog.Shared;

namespace SysLog.Repository.Repositories
{
    public class BackupLoaderRepository : IBackupLoaderRepository
    {
        private readonly BackupDbContext      _backupContext;
        private readonly ApplicationDbContext _logContext;

        // Tablas que NUNCA queremos tocar (migraciones, Identity, backup_file)
        private static readonly HashSet<string> ExcludedTables = new(StringComparer.OrdinalIgnoreCase)
        {
            "backup_file",
            "__EFMigrationsHistory",
            "AspNetUsers",
            "AspNetRoles",
            "AspNetUserRoles",
            "AspNetUserClaims",
            "AspNetUserLogins",
            "AspNetUserTokens",
            "AspNetRoleClaims"
        };

        // Regex para extraer el nombre de la tabla de sentencias comunes
        private static readonly Regex TableRegex = new(
            @"\b(?:FROM|INTO|TABLE|ALTER\s+TABLE|INSERT\s+INTO|DROP\s+TABLE)\s+" +
            @"(?:(?:public\.)?""?(?<table>\w+)""?)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public BackupLoaderRepository(
            BackupDbContext backupContext,
            ApplicationDbContext logContext)
        {
            _backupContext = backupContext;
            _logContext    = logContext;
        }

        public async Task<List<Log>> LoadBackupAsync(string day)
        {
            await using var conn = (NpgsqlConnection)_backupContext.Database.GetDbConnection();
            await conn.OpenAsync();

            // 1) Limpiar solo datos existentes (TRUNCATE)
            await CleanupAsync(conn);

            // 2) Leer y ejecutar cada backup file (.sql)
            var backups = await _backupContext.BackupFile
                .Where(b => b.FileName.StartsWith(day))
                .OrderBy(b => b.FileName)
                .ToListAsync();

            foreach (var b in backups)
            {
                var path   = Path.Combine(b.PathFile, b.FileName);
                var script = await File.ReadAllTextAsync(path);
                await ExecuteSqlScriptAsync(conn, script);
            }

            await conn.CloseAsync();

            // 3) Traer los logs cargados
            return await GetLogsFromBackupAsync();
        }

        public async Task<PagedResult<Log>> LoadBackupPagedAsync(
            string day, int page, int pageSize)
        {
            await using var conn = (NpgsqlConnection)_backupContext.Database.GetDbConnection();
            await conn.OpenAsync();

            await CleanupAsync(conn);

            var backups = await _backupContext.BackupFile
                .Where(b => b.FileName.StartsWith(day))
                .OrderBy(b => b.FileName)
                .ToListAsync();

            foreach (var b in backups)
            {
                var path   = Path.Combine(b.PathFile, b.FileName);
                var script = await File.ReadAllTextAsync(path);
                await ExecuteSqlScriptAsync(conn, script);
            }

            await conn.CloseAsync();

            var total = await _backupContext.Logs.CountAsync();
            var items = await _backupContext.Logs
                .Include(l => l.Protocol)
                .Include(l => l.Action)
                .Include(l => l.Interface)
                .Include(l => l.LogType).ThenInclude(lt => lt.Signature)
                .OrderByDescending(l => l.DateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Log>
            {
                Items      = items,
                TotalItems = total,
                Page       = page,
                PageSize   = pageSize
            };
        }

        public async Task<List<Log>> LoadCurrentLogsAsync()
        {
            // Limpia espacio temporal, no recarga backups
            await using var conn = (NpgsqlConnection)_backupContext.Database.GetDbConnection();
            await conn.OpenAsync();
            await CleanupAsync(conn);
            await conn.CloseAsync();

            return await GetLogsFromMainAsync();
        }

        public async Task<PagedResult<Log>> LoadCurrentLogsPagedAsync(
            int page, int pageSize)
        {
            await using var conn = (NpgsqlConnection)_backupContext.Database.GetDbConnection();
            await conn.OpenAsync();
            await CleanupAsync(conn);
            await conn.CloseAsync();

            var total = await _logContext.Logs.CountAsync();
            var items = await _logContext.Logs
                .Include(l => l.Protocol)
                .Include(l => l.Action)
                .Include(l => l.Interface)
                .Include(l => l.LogType).ThenInclude(lt => lt.Signature)
                .OrderByDescending(l => l.DateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Log>
            {
                Items      = items,
                TotalItems = total,
                Page       = page,
                PageSize   = pageSize
            };
        }

        private async Task CleanupAsync(NpgsqlConnection conn)
        {
            var tables = new List<string>();
            await using (var cmd = new NpgsqlCommand(
                @"SELECT table_name
                    FROM information_schema.tables
                   WHERE table_schema = 'public'
                     AND table_type   = 'BASE TABLE';",
                conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var t = reader.GetString(0);
                    if (!ExcludedTables.Contains(t))
                        tables.Add(t);
                }
            }

            foreach (var tbl in tables)
            {
                await conn.ExecuteAsync(
                    $@"TRUNCATE TABLE ""{tbl}"" RESTART IDENTITY CASCADE;");
            }
        }

        private async Task ExecuteSqlScriptAsync(NpgsqlConnection conn, string script)
        {
            foreach (var stmtRaw in SafeSplitSql(script))
            {
                var m = TableRegex.Match(stmtRaw);
                if (m.Success && ExcludedTables.Contains(m.Groups["table"].Value))
                    continue;

                var stmt = SanitizeStatement(stmtRaw);

                try
                {
                    await using var cmd = new NpgsqlCommand(stmt, conn);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (PostgresException ex) when (
                    ex.SqlState == "42P01" || // undefined_table
                    ex.SqlState == "42P07" || // duplicate_table
                    ex.SqlState == "42710")   // duplicate_object
                {
                    // ignorar y continuar
                }
            }
        }

        private static IEnumerable<string> SafeSplitSql(string script)
        {
            var sb = new StringBuilder();
            bool inSingle = false;

            foreach (char c in script)
            {
                if (c == '\'')
                {
                    sb.Append(c);
                    inSingle = !inSingle;
                }
                else if (c == ';' && !inSingle)
                {
                    var part = sb.ToString().Trim();
                    if (!string.IsNullOrEmpty(part))
                        yield return part;
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            var last = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(last))
                yield return last;
        }

        private string SanitizeStatement(string sql)
        {
            if (!sql.TrimStart().StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                return sql;

            var mHeader = Regex.Match(sql,
                @"^(INSERT\s+INTO\s+\S+\s*\([^\)]*\)\s*VALUES\s*)\(",
                RegexOptions.IgnoreCase);
            if (!mHeader.Success) 
                return sql;

            int openParen  = sql.IndexOf('(', mHeader.Length);
            int closeParen = sql.LastIndexOf(')');
            if (openParen < 0 || closeParen <= openParen) 
                return sql;

            string prefix  = sql.Substring(0, openParen + 1);
            string content = sql.Substring(openParen + 1, closeParen - openParen - 1);
            string suffix  = sql.Substring(closeParen);

            var sb = new StringBuilder();
            bool inSingle = false;
            foreach (char c in content)
            {
                if (c == '\'')
                {
                    sb.Append(c);
                    inSingle = !inSingle;
                }
                else if (inSingle)
                {
                    if (c == '\'') sb.Append("''");
                    else if (c == '"') sb.Append("\"\"");
                    else sb.Append(c);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return prefix + sb + suffix;
        }

        private Task<List<Log>> GetLogsFromBackupAsync() =>
            _backupContext.Logs
                .Include(l => l.Protocol)
                .Include(l => l.Action)
                .Include(l => l.Interface)
                .Include(l => l.LogType).ThenInclude(lt => lt.Signature)
                .OrderByDescending(l => l.DateTime)
                .ToListAsync();

        private Task<List<Log>> GetLogsFromMainAsync() =>
            _logContext.Logs
                .Include(l => l.Protocol)
                .Include(l => l.Action)
                .Include(l => l.Interface)
                .Include(l => l.LogType).ThenInclude(lt => lt.Signature)
                .OrderByDescending(l => l.DateTime)
                .ToListAsync();
    }

    public static class NpgsqlConnectionExtensions
    {
        public static async Task ExecuteAsync(
            this NpgsqlConnection conn, string sql)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
