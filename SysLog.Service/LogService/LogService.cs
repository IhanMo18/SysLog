using SysLog.Domine.Interface;
using SysLog.Domine.Interface.Service;
using SysLog.Domine.Model;

namespace SysLog.Service.LogService;

public class LogService(ILogRepository repository) : Service<Log>(repository),ILogService
{
}