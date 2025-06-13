
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

    public async Task<TaskResult<List<LogDto>>> GetPagedLogs(int page, int pageSize)
    {
        var result = await Client.CallApiAsync<List<LogDto>>($"api/log?page={page}&pageSize={pageSize}", "GET");

        if (result.Value.IsSuccessful(out var logDtos))
        {
            return TaskResult<List<LogDto>>.FromData(logDtos);
        }
        return TaskResult<List<LogDto>>.FromFailure(result.Value.Message, result.Value.Code,result.Value.Details);
    }
}
