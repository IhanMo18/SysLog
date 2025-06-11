using System.Globalization;
using System.Text.RegularExpressions;
using SysLog.Domine.Builder;
using SysLog.Domine.Interfaces;
using SysLog.Repository.Model;
using Action = SysLog.Repository.Model.Action;

namespace SysLog.Repository.Utilities.Parsing;

/// <summary>
/// Parses cron style log messages.
/// </summary>
public class CronLogParseStrategy : ILogParseStrategy
{
    public bool TryParse(string logMessage, out Log log)
    {
        log = null!;
        const string pattern = @"<\d+>([A-Za-z]+)\s+(\d+)\s+([\d:]+)\s+([^\[]+)\[(\d+)\]:\s+\((.*?)\)\s+CMD\s+\((.*)\)";
        Match match = Regex.Match(logMessage, pattern);
        if (!match.Success)
            return false;

        string month = match.Groups[1].Value;
        int day = int.Parse(match.Groups[2].Value);
        string time = match.Groups[3].Value;
        string process = match.Groups[4].Value;
        string user = match.Groups[6].Value;
        string command = match.Groups[7].Value;

        var fullDate = $"{DateTime.UtcNow.Year} {month} {day} {time}";
        DateTime dateTime = DateTime.ParseExact(fullDate, "yyyy MMM d HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        log = new Log
        {
            LogType = new LogTypeBuilder().WithType("cron").Build(),
            Interface = new Interface { Name = process },
            Protocol = new Protocol { Name = "N/A" },
            IpOut = user,
            IpDestiny = command,
            Action = new Action { AcctionName = "N/A" },
            DateTime = dateTime
        };
        return true;
    }
}
