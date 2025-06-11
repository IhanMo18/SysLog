namespace SysLog.Domine.ModelDto;

public class SignatureDto
{
    public string Message { get; set; }
 
    public LogTypeDto LogType { get; set; }
    public int LogTypeId { get; set; }
}