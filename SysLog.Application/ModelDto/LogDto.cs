namespace SysLog.Domine.ModelDto;

public class LogDto
{
    public LogTypeDto LogType { get; set; }
    public ActionDto Action { get; set; }
    public InterfaceDto Interface { get; set; }
    public ProtocolDto Protocol { get; set; }
    public string IpOut { get; set; }
    public string IpDestiny { get; set; }
    public DateTime DateTime { get; set; }
}