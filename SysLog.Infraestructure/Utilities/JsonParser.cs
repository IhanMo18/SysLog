using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using SysLog.Domine.Builder;
using SysLog.Domine.Interfaces;
using SysLog.Repository.Data;
using SysLog.Repository.Model;
using Action = SysLog.Repository.Model.Action;

namespace SysLog.Repository.Utilities;

public class JsonParser : IJsonParser
{
    private readonly ApplicationDbContext _context;

    public JsonParser(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public Log Parse(string logMessage)
    {
        
        if (TryParseJsonLog(logMessage, out var jsonLog))
        {
            return jsonLog;
        }
        else if (TryParseFilterLog(logMessage, out var filterLog))
        {
            return filterLog;
        }else if (TryParseCustomFilterLog(logMessage, out var customFilterLog))
        {
            return customFilterLog;
        }
        else if (TryParseCronLog(logMessage, out var cronLog))
        {
            return cronLog;
        }
        else
        {
            return CreateUnknownLog(logMessage);
        }
    }

    private bool TryParseJsonLog(string logMessage, out Log log)
    {
        log = null;
        int jsonStartIndex = logMessage.IndexOf("{");
        if (jsonStartIndex == -1) return false;

        string jsonPart = logMessage.Substring(jsonStartIndex);
        using JsonDocument doc = JsonDocument.Parse(jsonPart);
        JsonElement root = doc.RootElement;

        string timestamp = root.GetProperty("timestamp").GetString();
        string inIface = root.GetProperty("in_iface").GetString();
        string srcIp = root.GetProperty("src_ip").GetString();
        int srcPort = root.GetProperty("src_port").GetInt32();
        string destIp = root.GetProperty("dest_ip").GetString();
        int destPort = root.GetProperty("dest_port").GetInt32();
        string protocol = root.GetProperty("proto").GetString();
        string eventType = root.GetProperty("event_type").GetString();

        string action = IsPrivate(srcIp) ? "out" : "in";

        // Obtener la firma en caso de alert
        string? signature = null;
        if (eventType == "alert" && root.TryGetProperty("alert", out JsonElement alertElement))
        {
            signature = alertElement.GetProperty("signature").GetString();
        }

        DateTime dateTime = DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
            CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        log = new Log()
        {
           LogType = new LogTypeBuilder().WithType(typeName:eventType)
               .WhitSignature(signature).Build(),
           Interface = new Interface(){Name = inIface}, 
           Protocol = new Protocol(){Name = protocol}, 
           IpOut = srcIp, 
           IpDestiny = destIp, 
           Action = new Action(){AcctionName = action}, 
           DateTime = dateTime,
        };

        return true;
    }

    private bool TryParseFilterLog(string logMessage, out Log log)
    {
        log = null;
        string pattern = @"<\d+>([A-Za-z]+)\s+(\d+)\s+([\d:]+)\s+filterlog\[\d+\]:.*?,(em\d+\.\d+),(match|pass|block),.*?,(tcp|udp|icmp),.*?,([\d\.]+),([\d\.]+),(\d+),(\d+),";
        Match match = Regex.Match(logMessage, pattern);

        if (!match.Success) return false;

        string month = match.Groups[1].Value;
        int day = int.Parse(match.Groups[2].Value);
        string time = match.Groups[3].Value;
        string inIface = match.Groups[4].Value;
        string action = match.Groups[5].Value;
        string protocol = match.Groups[6].Value;
        string srcIp = match.Groups[7].Value;
        string destIp = match.Groups[8].Value;
        int srcPort = int.Parse(match.Groups[9].Value);
        int destPort = int.Parse(match.Groups[10].Value);

        var fullDate = $"{DateTime.UtcNow.Year} {month} {day} {time}";
        DateTime dateTime = DateTime.ParseExact(fullDate, "yyyy MMM d HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        log = new Log()
        {
            LogType = new LogTypeBuilder().WithType("filterlog").Build(),
            Interface = new Interface(){Name = inIface},
            Protocol = new Protocol(){Name = protocol},
            IpOut = srcIp,
            IpDestiny = destIp,
            Action = new Action(){AcctionName = action},
            DateTime = dateTime
        };

        return true;
    }

    private bool TryParseCronLog(string logMessage, out Log log)
    {
        log = null;
        string cronPattern = @"<\d+>([A-Za-z]+)\s+(\d+)\s+([\d:]+)\s+([^\[]+)\[(\d+)\]:\s+\((.*?)\)\s+CMD\s+\((.*)\)";
        Match cronMatch = Regex.Match(logMessage, cronPattern);

        if (!cronMatch.Success) return false;

        string month = cronMatch.Groups[1].Value;
        int day = int.Parse(cronMatch.Groups[2].Value);
        string time = cronMatch.Groups[3].Value;
        string process = cronMatch.Groups[4].Value;
        string processId = cronMatch.Groups[5].Value;
        string user = cronMatch.Groups[6].Value;
        string command = cronMatch.Groups[7].Value;

        var fullDate = $"{DateTime.UtcNow.Year} {month} {day} {time}";
        DateTime dateTime = DateTime.ParseExact(fullDate, "yyyy MMM d HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        log = new Log()
        {
            LogType = new LogTypeBuilder().WithType("cron").Build(),
            Interface = new Interface(){Name = process},
            Protocol = new Protocol(){Name = "N/A"},
            IpOut = user,
            IpDestiny = command,
            Action = new Action(){AcctionName = "N/A"},
            DateTime = dateTime
        };

        return true;
    }
    private bool TryParseCustomFilterLog(string logMessage, out Log log)
    {
        log = null;
        string pattern = @"<\d+>([A-Za-z]+)\s+(\d+)\s+filterlog\[(\d+)\]:\s+(\d+),,,(\d+),([a-zA-Z0-9.]+),([a-zA-Z]+),([a-zA-Z]+),(\d+),0x(\w*),,(\d+),(\d+),(\d+),none,(\d+),([a-zA-Z]+),(\d+),([\d\.]+),([\d\.]+),([a-zA-Z]+),(\d+),(\d+)";
        Match match = Regex.Match(logMessage, pattern);

        if (!match.Success) return false;

        string month = match.Groups[1].Value; // Mar
        int day = int.Parse(match.Groups[2].Value); // 20
        string time = match.Groups[3].Value; // 09:14:22
        string inIface = match.Groups[6].Value; // em0.10
        string action = match.Groups[7].Value; // match
        string protocol = match.Groups[8].Value; // block
        string srcIp = match.Groups[18].Value; // 10.34.24.97
        string destIp = match.Groups[19].Value; // 10.36.169.121
        int srcPort = int.Parse(match.Groups[20].Value); // 1
        int destPort = int.Parse(match.Groups[21].Value); // 4740

        var fullDate = $"{DateTime.UtcNow.Year} {month} {day} {time}";
        DateTime dateTime = DateTime.ParseExact(fullDate, "yyyy MMM d HH:mm:ss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        log = new Log()
        {
            LogType = new LogTypeBuilder().WithType("filterlog").Build(),
            Interface = new Interface(){Name = inIface},
            Protocol = new Protocol(){Name = protocol},
            IpOut = srcIp,
            IpDestiny = destIp,
            Action = new Action(){AcctionName = action},
            DateTime = dateTime
        };

        return true;
    }

    private Log CreateUnknownLog(string logMessage)
    {
        return new Log()
        {
            LogType = new LogTypeBuilder().WithType("unknown").Build(),
            Interface = new Interface(){Name = "N/A"},
            Protocol = new Protocol(){Name = "N/A"},
            IpOut = "N/A",
            IpDestiny = logMessage,
            Action = new Action(){AcctionName = "N/A"},
            DateTime = DateTime.UtcNow
        };
    }

    private async Task SaveLogsAsync(List<Log> logsToSave)
    {
        _context.AddRange(logsToSave);
        await _context.SaveChangesAsync();
    }
    
    private bool IsPrivate(string ip)
    {
        if (IPAddress.TryParse(ip, out IPAddress ipAddress))
        {
            byte[] bytes = ipAddress.GetAddressBytes();
            return (bytes[0] == 10) ||
                   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168);
        }

        return false;
    }
    
}
