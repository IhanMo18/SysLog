using SysLog.Repository.Model;
using SysLog.Shared;

namespace SysLog.Domine.Interfaces.Repositories;

public interface IBackupLoaderRepository
{
    Task<List<Log>> LoadBackupAsync(string day);
    Task<List<Log>> LoadCurrentLogsAsync();

    /// <summary>
    /// Load logs for a specific backup day with pagination.
    /// </summary>
    Task<PagedResult<Log>> LoadBackupPagedAsync(string day, int page, int pageSize);

    /// <summary>
    /// Load current logs using the backup loader pipeline with pagination.
    /// </summary>
    Task<PagedResult<Log>> LoadCurrentLogsPagedAsync(int page, int pageSize);
}
