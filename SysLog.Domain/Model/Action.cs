namespace SysLog.Repository.Model;

public class Action
{
    public int Id { get; set; }
    public string AcctionName { get; set; }
    public Log Log { get; set; }
    public int LogId { get; set; }
}