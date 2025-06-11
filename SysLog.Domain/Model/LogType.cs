using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysLog.Repository.Model;

[Table(name:"log_types")]
public class LogType
{
    [Key] 
    public int Id { get; set; }
    public string TypeName{ get; set; }
    public Signature? Signature { get; set; }
    
    public Log Log { get; set; }
    public int LogId { get; set; }
}