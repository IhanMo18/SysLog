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

    public async Task<TaskResult<List<BackupFileDto>>> GetAllBackups()
    {
        var result = await _client.CallApiAsync<List<BackupFileDto>>("api/log/days", "GET");
        if (result != null && result.Value.IsSuccessful(out var backups))
        {
            return TaskResult<List<BackupFileDto>>.FromData(backups);
        }
        return TaskResult<List<BackupFileDto>>.FromFailure(result?.Value.Message ?? "Error");
    }

    public async Task<TaskResult> GetSelectedBackup(string backupName)
    {
        // Placeholder: in the future this will load the script for the selected backup
        await _client.CallApiAsync<string>($"api/log/backup?day={backupName}", "GET");
        return TaskResult.SuccessResult;
    }
}
