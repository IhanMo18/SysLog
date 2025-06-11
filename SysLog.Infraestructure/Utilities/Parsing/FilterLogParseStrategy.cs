using System.Globalization;
using System.Text.RegularExpressions;
using SysLog.Domine.Builder;
using SysLog.Domine.Interfaces;
using SysLog.Repository.Model;
using Action = SysLog.Repository.Model.Action;

namespace SysLog.Repository.Utilities.Parsing;

/// <summary>
/// Parses pf firewall filterlog messages.
/// </summary>
public class FilterLogParseStrategy : ILogParseStrategy
{
    public bool TryParse(string logMessage, out Log log)
    {
        log = null!;
        const string pattern = @"<\d+>([A-Za-z]+)\s+(\d+)\s+([\d:]+)\s+filterlog\[\d+\]:.*?,(em\d+\.\d+),(match|pass|block),.*?,(tcp|udp|icmp),.*?,([\d\.]+),([\d\.]+),(\d+),(\d+),";
        Match match = Regex.Match(logMessage, pattern);
        if (!match.Success)
            return false;

        string month = match.Groups[1].Value;
        int day = int.Parse(match.Groups[2].Value);
        string time = match.Groups[3].Value;
        string inIface = match.Groups[4].Value;
        string action = match.Groups[5].Value;
        string protocol = match.Groups[6].Value;
        string srcIp = match.Groups[7].Value;
        string destIp = match.Groups[8].Value;

        var fullDate = $"{DateTime.UtcNow.Year} {month} {day} {time}";
        DateTime dateTime = DateTime.ParseExact(fullDate, "yyyy MMM d HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        log = new Log
        {
            LogType = new LogTypeBuilder().WithType("filterlog").Build(),
            Interface = new Interface { Name = inIface },
            Protocol = new Protocol { Name = protocol },
            IpOut = srcIp,
            IpDestiny = destIp,
            Action = new Action { AcctionName = action },
            DateTime = dateTime
        };

        return true;
    }
}
