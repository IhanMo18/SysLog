using Microsoft.EntityFrameworkCore;
using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Model;
using SysLog.Shared;
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
        // Removing each entity individually did not reliably delete all data
        // in some cases.  Instead, issue a TRUNCATE with CASCADE to ensure
        // that all log related tables are cleared.
        
        const string sql = @"
TRUNCATE TABLE ""signatures"", ""logs_type"", ""actions"", ""interfaces"", ""protocols"", ""logs"" RESTART IDENTITY CASCADE;
";


        await _dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    public async Task<PagedResult<Log>> GetPagedLogsAsync(int page, int pageSize)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 1;

        var totalItems = await _dbContext.Set<Log>().CountAsync();

        var items = await _dbContext.Set<Log>()
            .OrderByDescending(log => log.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(log => log.Protocol)
            .Include(log => log.Action)
            .Include(log => log.Interface)
            .Include(log => log.LogType)
                .ThenInclude(lt => lt.Signature)
            .ToListAsync();

        return new PagedResult<Log>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }}