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
        return TaskResult<List<string>>.FromFailure(result?.Value.Message ?? "Error");
    }

    public async Task<TaskResult> GetSelectedBackup(string day)
    {
        await _client.CallApiAsync<string>($"api/log/backup?day={day}", "GET");
        return TaskResult.SuccessResult;
    }
}
