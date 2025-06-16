
namespace SysLog.Repository.Model;



public class Log
{
    public int LogId { get; set; }
    public LogType LogType { get; set; }
    public Action Action { get; set; }
    public Interface Interface { get; set; }
    public Protocol Protocol { get; set; }
    public string IpOut { get; set; }
    public string IpDestiny { get; set; }
    public DateTime DateTime { get; set; }
    
}