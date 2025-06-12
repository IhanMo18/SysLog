using SysLog.Shared.ModelDto;

namespace SysLog.Service.Interfaces.Services;

public interface ILogService : IServiceDto<LogDto>
{
    public Task<LogDto> GetLastLogAsync();
    Task RemoveAllLogsAsync();
    Task<IEnumerable<LogDto>> GetPagedLogsAsync(int page, int pageSize);
}