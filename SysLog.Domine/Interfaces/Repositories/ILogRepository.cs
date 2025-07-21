using SysLog.Repository.Model;
using SysLog.Shared;

namespace SysLog.Domine.Interfaces.Repositories;

public interface ILogRepository : IRepository<Log>
{
    public Task<Log> getLastLogAsync();
    Task RemoveAllLogsAsync();
    /// <summary>
    /// Remove all logs along with their related entities.
    /// </summary>
    Task RemoveAllLogsWithPropertiesAsync();
    Task<PagedResult<Log>> GetPagedLogsAsync(int page, int pageSize);
    Task<PagedResult<Log>> SearchLogsAsync(string? property, string? term, int page, int pageSize);
}