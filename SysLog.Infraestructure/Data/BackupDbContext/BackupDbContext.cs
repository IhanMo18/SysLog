using Microsoft.EntityFrameworkCore;
using SysLog.Repository.Model;

namespace SysLog.Repository.Data;

public class BackupDbContext : DbContext
{
    public BackupDbContext(DbContextOptions<BackupDbContext> options) : base(options)
    {
    }

    public DbSet<BackupFile> BackupFile{ get; set; }
}