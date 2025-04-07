using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SysLog.Data.Data;
using SysLog.Domine.Model;

namespace SysLog.Service;

public class UdpLogListener
{
    private readonly UdpClient _udpListener;
    private readonly ApplicationDbContext _context;

    public UdpLogListener(ApplicationDbContext context)
    {
        _udpListener = new UdpClient(514);
        _context = context;
    }

    public async Task StartListeningAsync()
    {
        IPEndPoint anyIp = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            try
            {
                var received = await _udpListener.ReceiveAsync();
                var logMessage = Encoding.ASCII.GetString(received.Buffer);
                var logsToSave = ParseLogMessage(logMessage);

                if (logsToSave.Count > 0)
                {
                    await SaveLogsAsync(logsToSave);
                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al procesar JSON: {jsonEx.Message}");
            }
            catch (FormatException fmtEx)
            {
                Console.WriteLine($"Error en formato de fecha u otro valor: {fmtEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
            }
        }
    }

    private List<Log> ParseLogMessage(string logMessage)
    {
        var logsToSave = new List<Log>();

        if (TryParseJsonLog(logMessage, out var jsonLog))
        {
            logsToSave.Add(jsonLog);
        }
        else if (TryParseFilterLog(logMessage, out var filterLog))
        {
            logsToSave.Add(filterLog);
        }else if (TryParseCustomFilterLog(logMessage, out var customFilterLog))
        {
            logsToSave.Add(customFilterLog);
        }
        else if (TryParseCronLog(logMessage, out var cronLog))
        {
            logsToSave.Add(cronLog);
        }
        else
        {
            logsToSave.Add(CreateUnknownLog(logMessage));
        }

        return logsToSave;
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

        string action = EsIpPrivada(srcIp) ? "out" : "in";

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
            Type = eventType,
            Interface = inIface,
            Protocol = protocol,
            IpOut = srcIp,
            IpDestiny = destIp,
            Acction = action,
            DateTime = dateTime,
            Signature = signature
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
            Type = "filterlog",
            Interface = inIface,
            Protocol = protocol,
            IpOut = srcIp,
            IpDestiny = destIp,
            Acction = action,
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
            Type = "cron",
            Interface = process,
            Protocol = "N/A",
            IpOut = user,
            IpDestiny = command,
            Acction = "N/A",
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
            Type = "filterlog",
            Interface = inIface,
            Protocol = protocol,
            IpOut = srcIp,
            IpDestiny = destIp,
            Acction = action,
            DateTime = dateTime
        };

        return true;
    }

    private Log CreateUnknownLog(string logMessage)
    {
        return new Log()
        {
            Type = "unknown",
            Interface = "N/A",
            Protocol = "N/A",
            IpOut = "N/A",
            IpDestiny = logMessage,
            Acction = "N/A",
            DateTime = DateTime.UtcNow
        };
    }

    private async Task SaveLogsAsync(List<Log> logsToSave)
    {
        _context.AddRange(logsToSave);
        await _context.SaveChangesAsync();
    }
    
    
    
    

    private bool EsIpPrivada(string ip)
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
