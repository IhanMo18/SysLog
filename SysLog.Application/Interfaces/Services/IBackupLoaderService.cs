using SysLog.Shared.ModelDto;
using SysLog.Shared;

namespace SysLog.Service.Interfaces.Services;

public interface IBackupLoaderService
{
    Task<List<LogDto>> LoadBackupAsync(string day);
    Task<List<LogDto>> LoadCurrentLogsAsync();

    /// <summary>
    /// Load logs for a specific backup day using pagination.
    /// </summary>
    Task<PagedResult<LogDto>> LoadBackupPagedAsync(string day, int page, int pageSize);

    /// <summary>
    /// Load current logs through the backup pipeline using pagination.
    /// </summary>
    Task<PagedResult<LogDto>> LoadCurrentLogsPagedAsync(int page, int pageSize);
}
