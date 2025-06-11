using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysLog.Repository.Model;

[Table(name:"signatures")]
public class Signature
{
 [Key]
 public int Id { get; set; }
 public string Message { get; set; }
 
 public LogType LogType { get; set; }
 public int LogTypeId { get; set; }
}