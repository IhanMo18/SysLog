using SysLog.Client.Client;
using SysLog.Shared;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class BackupService
{
    private readonly ClientSideApi _client;

    public BackupService(ClientSideApi client)
    {
        _client = client;
    }

    public async Task<TaskResult<List<string>>> GetAllBackups()
    {
        var result = await _client.CallApiAsync<List<string>>("api/log/days", "GET");
        if (result != null && result.Value.IsSuccessful(out var backups))
        {
            return TaskResult<List<string>>.FromData(backups);
        }
        return TaskResult<List<string>>.FromFailure(result?.Message ?? "Error");
    }

    public async Task<TaskResult<List<LogDto>>> LoadBackup(string day)
    {
        var result = await _client.CallApiAsync<List<LogDto>>($"api/log/backup?day={day}", "GET");
        if (result != null && result.Value.IsSuccessful(out var logs))
            return TaskResult<List<LogDto>>.FromData(logs);
        return TaskResult<List<LogDto>>.FromFailure(result?.Message ?? "Error");
    }

    public async Task<TaskResult<PagedResult<LogDto>>> LoadBackupPaged(string day, int page, int pageSize)
    {
        var result = await _client.CallApiAsync<PagedResult<LogDto>>($"api/log/backup-paged?day={day}&page={page}&pageSize={pageSize}", "GET");
        if (result != null && result.Value.IsSuccessful(out var logs))
            return TaskResult<PagedResult<LogDto>>.FromData(logs);
        return TaskResult<PagedResult<LogDto>>.FromFailure(result?.Message ?? "Error");
    }

    public async Task<TaskResult<List<LogDto>>> LoadCurrentLogs()
    {
        var result = await _client.CallApiAsync<List<LogDto>>("api/log/current", "GET");
        if (result != null && result.Value.IsSuccessful(out var logs))
            return TaskResult<List<LogDto>>.FromData(logs);
        return TaskResult<List<LogDto>>.FromFailure(result?.Message ?? "Error");
    }

    public async Task<TaskResult<PagedResult<LogDto>>> LoadCurrentPaged(int page, int pageSize)
    {
        var result = await _client.CallApiAsync<PagedResult<LogDto>>($"api/log/current-paged?page={page}&pageSize={pageSize}", "GET");
        if (result != null && result.Value.IsSuccessful(out var logs))
            return TaskResult<PagedResult<LogDto>>.FromData(logs);
        return TaskResult<PagedResult<LogDto>>.FromFailure(result?.Message ?? "Error");
    }
}
