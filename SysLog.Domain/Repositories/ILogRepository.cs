using SysLog.Repository.Model;

namespace SysLog.Domine.Repositories;

public interface ILogRepository : IRepository<Log>
{
    public Task<Log> getLastLogAsync();
    Task RemoveAllLogsAsync();
}