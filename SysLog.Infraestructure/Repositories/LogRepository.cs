using Microsoft.EntityFrameworkCore;
using SysLog.Domine.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Model;

namespace SysLog.Repository.Repositories;

public class LogRepository(ApplicationDbContext dbContext)  : Repository<Log>(dbContext),ILogRepository
{
    public async Task<Log> getLastLogAsync()
    {
      return await _dbContext.Set<Log>().OrderByDescending(log=>log.DateTime)
            .FirstOrDefaultAsync();
    }

    public void RemoveAllLogs()
    {
        var allLogs = _dbContext.Set<Log>().ToList();
        _dbContext.Set<Log>().RemoveRange(allLogs);
        _dbContext.SaveChanges();
    }
}