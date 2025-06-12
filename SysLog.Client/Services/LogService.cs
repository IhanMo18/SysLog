using System;
using LogUdp.Api;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class LogService
{
    public SysApiDynamic Client { get; set; }

    public LogService(SysApiDynamic client)
    {
        this.Client = client;
    }

    public async Task<List<LogDto>> GetPagedLogs(int page, int pageSize)
    {
        var logs = await Client.CallApiAsync<List<LogDto>>($"api/log?page={page}&pageSize={pageSize}", "GET");
        return logs;
    }
}
