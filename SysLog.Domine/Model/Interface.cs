using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysLog.Repository.Model;

[Table(name:"interfaces")]
public class Interface
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    
    public Log Log { get; set; }
    public int LogId { get; set; }
}