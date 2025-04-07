using SysLog.Data.Data;
using SysLog.Domine.Interface;
using SysLog.Domine.Model;

namespace SysLog.Repository.Repositories;

public class LogRepository(ApplicationDbContext dbContext)  : Repository<Log>(dbContext),ILogRepository
{
    
}