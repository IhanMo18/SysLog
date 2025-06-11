using SysLog.Repository.Model;

namespace SysLog.Domine.Repositories;

public interface ILogRepository : IRepository<Log>
{
    public Task<Log> getLastLogAsync();
    public void RemoveAllLogs();
}