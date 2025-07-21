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
    }

    public async Task<PagedResult<Log>> SearchLogsAsync(string? property, string? term, int page, int pageSize)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 1;

        var query = _dbContext.Set<Log>()
            .Include(l => l.Protocol)
            .Include(l => l.Action)
            .Include(l => l.Interface)
            .Include(l => l.LogType)
                .ThenInclude(lt => lt.Signature)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            term = term.ToLower();
            if (!string.IsNullOrWhiteSpace(property))
            {
                query = property switch
                {
                    "IpOut" => query.Where(l => l.IpOut.ToLower().Contains(term)),
                    "IpDestiny" => query.Where(l => l.IpDestiny.ToLower().Contains(term)),
                    "Protocol" => query.Where(l => l.Protocol.Name.ToLower().Contains(term)),
                    "Action" => query.Where(l => l.Action.AcctionName.ToLower().Contains(term)),
                    "Interface" => query.Where(l => l.Interface.Name.ToLower().Contains(term)),
                    "LogType" => query.Where(l => l.LogType.TypeName.ToLower().Contains(term)),
                    "DateTime" => query.Where(l => l.DateTime.ToString().ToLower().Contains(term)),
                    _ => query.Where(l => l.LogType.Signature.Message.ToLower().Contains(term))
                };
            }
            else
            {
                query = query.Where(l =>
                    l.IpOut.ToLower().Contains(term) ||
                    l.IpDestiny.ToLower().Contains(term) ||
                    l.Protocol.Name.ToLower().Contains(term) ||
                    l.Action.AcctionName.ToLower().Contains(term) ||
                    l.Interface.Name.ToLower().Contains(term) ||
                    l.LogType.TypeName.ToLower().Contains(term) ||
                    l.LogType.Signature.Message.ToLower().Contains(term) ||
                    l.DateTime.ToString().ToLower().Contains(term)
                );
            }
        }

        var totalItems = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.DateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Log>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize
        };
    }
}