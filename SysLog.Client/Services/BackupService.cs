using Microsoft.Extensions.Hosting;
using SysLog.Client.Client;
using SysLog.Shared;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class BackupService
{
    
    public ClientSideApi clientSideApi { get; set; }


    public BackupService(ClientSideApi clientSideApi)
    {
        this.clientSideApi = clientSideApi;
    }

    public async Task<TaskResult<List<BackupFileDto>>> GetAllBackups()
    {
        var result = await clientSideApi.CallApiAsync<List<BackupFileDto>>($"api/days", "GET");
        return result.Value.IsSuccessful(out var backupFileDtos) ? 
            TaskResult<List<BackupFileDto>>.FromData(backupFileDtos) : 
            TaskResult<List<BackupFileDto>>.FromFailure(result.Value.Message, result.Value.Code, result.Value.Details);
    }


    public async Task<TaskResult<List<BackupFileDto>>> GetSelectedBackup(string date)
    {
       var result =  await clientSideApi.CallApiAsync<List<BackupFileDto>>($"api/log/backup?day={date}", "GET");
       return result.Value.IsSuccessful(out var backupFileDto) ?
       TaskResult<List<BackupFileDto>>.FromData(backupFileDto) : 
       TaskResult<List<BackupFileDto>>.FromFailure(result.Value.Message, result.Value.Code, result.Value.Details);
       
    }
}