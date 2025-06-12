using SysLog.Shared.ModelDto;

namespace SysLog.Service.Interfaces.Services;

public interface ILogService : IServiceDto<LogDto>
{
    public Task<LogDto> GetLastLogAsync();
    Task RemoveAllLogsAsync();
    /// <summary>
    /// Remove logs and their related properties after backup.
    /// </summary>
    Task RemoveAllLogsWithPropertiesAsync();
    Task<IEnumerable<LogDto>> GetPagedLogsAsync(int page, int pageSize);
}