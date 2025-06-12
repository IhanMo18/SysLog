using SysLog.Repository.Model;

namespace SysLog.Domine.Interfaces.Repositories;

public interface ILogRepository : IRepository<Log>
{
    public Task<Log> getLastLogAsync();
    Task RemoveAllLogsAsync();
    Task<IEnumerable<Log>> GetPagedLogsAsync(int page, int pageSize);
}