using SysLog.Domine.Builder;
using SysLog.Domine.Interfaces;
using SysLog.Repository.Model;
using Action = SysLog.Repository.Model.Action;

namespace SysLog.Repository.Utilities.Parsing;

/// <summary>
/// Fallback strategy when no other parser matches the log message.
/// </summary>
public class UnknownLogParseStrategy : ILogParseStrategy
{
    public bool TryParse(string logMessage, out Log log)
    {
        log = new Log
        {
            LogType = new LogTypeBuilder().WithType("unknown").Build(),
            Interface = new Interface { Name = "N/A" },
            Protocol = new Protocol { Name = "N/A" },
            IpOut = "N/A",
            IpDestiny = logMessage,
            Action = new Action { AcctionName = "N/A" },
            DateTime = DateTime.UtcNow
        };
        return true;
    }
}
