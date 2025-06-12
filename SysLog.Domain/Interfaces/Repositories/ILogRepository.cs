using SysLog.Repository.Model;

namespace SysLog.Domine.Interfaces.Repositories;

public interface ILogRepository : IRepository<Log>
{
    public Task<Log> getLastLogAsync();
    Task RemoveAllLogsAsync();
    /// <summary>
    /// Remove all logs along with their related entities.
    /// </summary>
    Task RemoveAllLogsWithPropertiesAsync();
    Task<IEnumerable<Log>> GetPagedLogsAsync(int page, int pageSize);
}