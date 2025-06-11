using SysLog.Domine.ModelDto;

namespace SysLog.Service.Interfaces.Services;

public interface ILogService : IServiceDto<LogDto>
{
    public Task<LogDto> GetLastLogAsync();
    Task RemoveAllLogsAsync();
}