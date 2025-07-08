using Microsoft.EntityFrameworkCore;
using Npgsql;
using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Data.BackupDbContext;
using SysLog.Repository.Model;
using SysLog.Shared;

namespace SysLog.Repository.Repositories;

public class BackupLoaderRepository : IBackupLoaderRepository
{
    private readonly BackupDbContext _backupContext;
    private readonly ApplicationDbContext _logContext;

    public BackupLoaderRepository(BackupDbContext backupContext, ApplicationDbContext logContext)
    {
        _backupContext = backupContext;
        _logContext = logContext;
    }

    public async Task<List<Log>> LoadBackupAsync(string day)
    {
        await using var conn = (NpgsqlConnection)_backupContext.Database.GetDbConnection();
        await conn.OpenAsync();
        await CleanupAsync(conn);

        var backups = await _backupContext.BackupFile
            .Where(b => b.FileName.StartsWith(day))
            .OrderBy(b => b.FileName)
            .ToListAsync();

        foreach (var backup in backups)
        {
            var scriptPath = Path.Combine(backup.PathFile, backup.FileName);
            var sql = await File.ReadAllTextAsync(scriptPath);

            await ExecuteSqlScriptAsync(conn, sql);
        }

        await conn.CloseAsync();
        return await GetLogsFromBackupAsync();
    }

    public async Task<List<Log>> LoadCurrentLogsAsync()
    {
        await using var conn = (NpgsqlConnection)_backupContext.Database.GetDbConnection();
        await conn.OpenAsync();
        await CleanupAsync(conn);
        await conn.CloseAsync();

        return await GetLogsFromMainAsync();
    }

    public async Task<PagedResult<Log>> LoadBackupPagedAsync(string day, int page, int pageSize)
    {
        await using var conn = (NpgsqlConnection)_backupContext.Database.GetDbConnection();
        await conn.OpenAsync();
        await CleanupAsync(conn);

        var backups = await _backupContext.BackupFile
            .Where(b => b.FileName.StartsWith(day))
            .OrderBy(b => b.FileName)
            .ToListAsync();

        foreach (var backup in backups)
        {
            var scriptPath = Path.Combine(backup.PathFile, backup.FileName);
            var sql = await File.ReadAllTextAsync(scriptPath);

            await ExecuteSqlScriptAsync(conn, sql);
        }

        await conn.CloseAsync();

        var totalItems = await _backupContext.Logs.CountAsync();
        var items = await _backupContext.Logs
            .Include(l => l.Protocol)
            .Include(l => l.Action)
            .Include(l => l.Interface)
            .Include(l => l.LogType)
                .ThenInclude(lt => lt.Signature)
            .OrderByDescending(l => l.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Log>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<Log>> LoadCurrentLogsPagedAsync(int page, int pageSize)
    {
        await using var conn = (NpgsqlConnection)_backupContext.Database.GetDbConnection();
        await conn.OpenAsync();
        await CleanupAsync(conn);
        await conn.CloseAsync();

        var totalItems = await _logContext.Logs.CountAsync();

        var items = await _logContext.Logs
            .Include(l => l.Protocol)
            .Include(l => l.Action)
            .Include(l => l.Interface)
            .Include(l => l.LogType)
                .ThenInclude(lt => lt.Signature)
            .OrderByDescending(l => l.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Log>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }

    private static async Task ExecuteSqlScriptAsync(NpgsqlConnection connection, string script)
    {
        await using var cmd = new NpgsqlCommand(script, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task CleanupAsync(NpgsqlConnection conn)
    {
        var tables = new List<string>();
        await using (var cmd = new NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE' AND table_name NOT IN ('backup_file','__EFMigrationsHistory');", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                tables.Add(reader.GetString(0));
        }

        foreach (var table in tables)
            await _backupContext.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS \"{table}\" CASCADE;");
    }

    private async Task<List<Log>> GetLogsFromBackupAsync()
    {
        var logs = await _backupContext.Logs
            .Include(l => l.Protocol)
            .Include(l => l.Action)
            .Include(l => l.Interface)
            .Include(l => l.LogType)
                .ThenInclude(lt => lt.Signature)
            .OrderByDescending(l => l.DateTime)
            .ToListAsync();

        return logs;
    }

    private async Task<List<Log>> GetLogsFromMainAsync()
    {
        var logs = await _logContext.Logs
            .Include(l => l.Protocol)
            .Include(l => l.Action)
            .Include(l => l.Interface)
            .Include(l => l.LogType)
                .ThenInclude(lt => lt.Signature)
            .OrderByDescending(l => l.DateTime)
            .ToListAsync();

        return logs;
    }
}
