
using SysLog.Client.Client;
using SysLog.Client.Client;
using SysLog.Shared;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class LogService
{
    public ClientSideApi Client { get; set; }

    public LogService(ClientSideApi client)
    {
        Client = client;
    }

    public async Task<TaskResult<PagedResult<LogDto>>> GetPagedLogs(int page, int pageSize)
    {
        var result = await Client.CallApiAsync<PagedResult<LogDto>>($"api/log?page={page}&pageSize={pageSize}", "GET");

        if (result.Value.IsSuccessful(out var data))
        {
            return TaskResult<PagedResult<LogDto>>.FromData(data);
        }
        return TaskResult<PagedResult<LogDto>>.FromFailure(result.Value.Message, result.Value.Code, result.Value.Details);
    }
    
    
    
}
