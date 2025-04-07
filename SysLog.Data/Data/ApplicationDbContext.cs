using Microsoft.EntityFrameworkCore;
using SysLog.Domine.Model;

namespace SysLog.Data.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    private DbSet<Log> Log { get; set; }
}