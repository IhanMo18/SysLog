using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using SysLog.Domine.Builder;
using SysLog.Domine.Interfaces;
using SysLog.Repository.Model;
using Action = SysLog.Repository.Model.Action;

namespace SysLog.Repository.Utilities.Parsing;

/// <summary>
/// Parses Suricata style JSON logs.
/// </summary>
public class JsonLogParseStrategy : ILogParseStrategy
{
    public bool TryParse(string logMessage, out Log log)
    {
        log = null!;
        int jsonStartIndex = logMessage.IndexOf('{');
        if (jsonStartIndex == -1)
            return false;

        string jsonPart = logMessage.Substring(jsonStartIndex);

        JsonElement root;
        try
        {
            using JsonDocument doc = JsonDocument.Parse(jsonPart);
            root = doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return TryParsePartial(jsonPart, out log);
        }

        string timestamp = root.GetProperty("timestamp").GetString()!;
        string inIface = root.GetProperty("in_iface").GetString()!;
        string srcIp = root.GetProperty("src_ip").GetString()!;
        string destIp = root.GetProperty("dest_ip").GetString()!;
        string protocol = root.GetProperty("proto").GetString()!;
        string eventType = root.GetProperty("event_type").GetString()!;

        string action = IsPrivate(srcIp) ? "out" : "in";

        string? signature = null;
        if (eventType == "alert" && root.TryGetProperty("alert", out JsonElement alertElement))
        {
            signature = alertElement.GetProperty("signature").GetString();
        }

        DateTime dateTime;
        try
        {
            dateTime = DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }
        catch (FormatException)
        {
            return false;
        }

        log = new Log
        {
            LogType = new LogTypeBuilder().WithType(eventType)
                .WhitSignature(signature).Build(),
            Interface = new Interface { Name = inIface },
            Protocol = new Protocol { Name = protocol },
            IpOut = srcIp,
            IpDestiny = destIp,
            Action = new Action { AcctionName = action },
            DateTime = dateTime
        };

        return true;
    }

    private static bool IsPrivate(string ip)
    {
        if (System.Net.IPAddress.TryParse(ip, out var ipAddress))
        {
            byte[] bytes = ipAddress.GetAddressBytes();
            return (bytes[0] == 10) ||
                   (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                   (bytes[0] == 192 && bytes[1] == 168);
        }
        
        return false;
    }

    private static bool TryParsePartial(string jsonPart, out Log log)
    {
        log = null!;

        string Extract(string name)
        {
            var match = Regex.Match(jsonPart, $"\"{name}\":\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        string timestamp = Extract("timestamp");
        string inIface = Extract("in_iface");
        string srcIp = Extract("src_ip");
        string destIp = Extract("dest_ip");
        string protocol = Extract("proto");
        string eventType = Extract("event_type");

        if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(inIface) ||
            string.IsNullOrEmpty(srcIp) || string.IsNullOrEmpty(destIp) ||
            string.IsNullOrEmpty(protocol) || string.IsNullOrEmpty(eventType))
        {
            return false;
        }

        DateTime dateTime;
        if (!DateTime.TryParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal,
                out dateTime))
        {
            return false;
        }
        dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        string action = IsPrivate(srcIp) ? "out" : "in";

        string? signature = null;
        var sigMatch = Regex.Match(jsonPart, "\"signature\":\"([^\"]+)\"");
        if (sigMatch.Success)
            signature = sigMatch.Groups[1].Value;

        log = new Log
        {
            LogType = new LogTypeBuilder().WithType(eventType)
                .WhitSignature(signature).Build(),
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
