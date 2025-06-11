using Microsoft.EntityFrameworkCore;
using SysLog.Domine.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Model;

namespace SysLog.Repository.Repositories;

public class LogRepository(ApplicationDbContext dbContext)  : Repository<Log>(dbContext),ILogRepository
{
    public async Task<Log> getLastLogAsync()
    {
        return await _dbContext.Set<Log>()
            .OrderByDescending(log => log.DateTime)
            .FirstOrDefaultAsync();
    }

    public async Task RemoveAllLogsAsync()
    {
        var allLogs = await _dbContext.Set<Log>().ToListAsync();
        _dbContext.Set<Log>().RemoveRange(allLogs);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Log>> GetPagedLogsAsync(int page, int pageSize)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 1;

        return await _dbContext.Set<Log>()
            .OrderByDescending(log => log.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}