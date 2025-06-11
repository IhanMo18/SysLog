using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SysLog.Repository.Model;

[Table(name:"backup_file")]
public class BackupFile
{
    [Key]
    public int Id { get; set; }
    public string PathFile { get; set; }
    public string FileName { get; set; }
}