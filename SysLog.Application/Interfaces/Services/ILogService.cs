using SysLog.Shared.ModelDto;
using SysLog.Shared;

namespace SysLog.Service.Interfaces.Services;

public interface ILogService : IServiceDto<LogDto>
{
    public Task<LogDto> GetLastLogAsync();
    Task RemoveAllLogsAsync();
    /// <summary>
    /// Remove logs and their related properties after backup.
    /// </summary>
    Task RemoveAllLogsWithPropertiesAsync();
    Task<PagedResult<LogDto>> GetPagedLogsAsync(int page, int pageSize);}