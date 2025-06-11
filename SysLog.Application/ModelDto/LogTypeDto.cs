namespace SysLog.Domine.ModelDto;

public class LogTypeDto
{
    public string TypeName{ get; set; }
    public SignatureDto? Signature { get; set; }
    
    public LogDto Log { get; set; }
    public int LogId { get; set; }
}
