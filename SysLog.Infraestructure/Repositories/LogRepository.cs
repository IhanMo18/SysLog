using Microsoft.EntityFrameworkCore;
using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Model;
using Action = SysLog.Repository.Model.Action;

namespace SysLog.Repository.Repositories;

public class LogRepository(ApplicationDbContext dbContext)  : Repository<Log>(dbContext),ILogRepository
{
    public async Task<Log> getLastLogAsync()
    {
        return await _dbContext.Set<Log>()
            .Include(log => log.Protocol)
            .Include(log => log.Action)
            .Include(log => log.Interface)
            .Include(log => log.LogType)
                .ThenInclude(lt => lt.Signature)
            .OrderByDescending(log => log.DateTime)
            .FirstOrDefaultAsync();
    }

    public async Task RemoveAllLogsAsync()
    {
        var allLogs = await _dbContext.Set<Log>().ToListAsync();
        _dbContext.Set<Log>().RemoveRange(allLogs);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveAllLogsWithPropertiesAsync()
    {
        var logs = await _dbContext.Set<Log>()
            .Include(l => l.Protocol)
            .Include(l => l.Action)
            .Include(l => l.Interface)
            .Include(l => l.LogType)
                .ThenInclude(lt => lt.Signature)
            .ToListAsync();

        _dbContext.Set<Signature>().RemoveRange(logs.Select(l => l.LogType.Signature));
        _dbContext.Set<LogType>().RemoveRange(logs.Select(l => l.LogType));
        _dbContext.Set<Action>().RemoveRange(logs.Select(l => l.Action));
        _dbContext.Set<Interface>().RemoveRange(logs.Select(l => l.Interface));
        _dbContext.Set<Protocol>().RemoveRange(logs.Select(l => l.Protocol));
        _dbContext.Set<Log>().RemoveRange(logs);

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
            .Include(log => log.Protocol)
            .Include(log => log.Action)
            .Include(log => log.Interface)
            .Include(log => log.LogType)
                .ThenInclude(lt => lt.Signature)
            .ToListAsync();
    }
}