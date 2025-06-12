using System.Globalization;
using System.Text.RegularExpressions;
using SysLog.Domine.Builder;
using SysLog.Domine.Interfaces;
using SysLog.Repository.Model;
using Action = SysLog.Repository.Model.Action;

namespace SysLog.Repository.Utilities.Parsing;

/// <summary>
/// Parses basic syslogd messages like "syslogd: restart".
/// </summary>
public class SyslogdLogParseStrategy : ILogParseStrategy
{
    public bool TryParse(string logMessage, out Log log)
    {
        log = null!;
        const string pattern = @"<\d+>([A-Za-z]+)\s+(\d+)\s+([\d:]+)\s+syslogd:\s+(.*)";
        var match = Regex.Match(logMessage, pattern);
        if (!match.Success)
            return false;

        string month = match.Groups[1].Value;
        int day = int.Parse(match.Groups[2].Value);
        string time = match.Groups[3].Value;
        string message = match.Groups[4].Value;

        var fullDate = $"{DateTime.UtcNow.Year} {month} {day} {time}";
        DateTime dateTime = DateTime.ParseExact(fullDate, "yyyy MMM d HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        log = new Log
        {
            LogType = new LogTypeBuilder().WithType("syslogd").Build(),
            Interface = new Interface { Name = "N/A" },
            Protocol = new Protocol { Name = "N/A" },
            IpOut = "N/A",
            IpDestiny = message,
            Action = new Action { AcctionName = "N/A" },
            DateTime = dateTime
        };

        return true;
    }
}
