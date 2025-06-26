using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SysLog.Repository.Data.BackupDbContext;

namespace SysLog.Repository.Data;

public class BackupDbContextFactory : IDesignTimeDbContextFactory<BackupDbContext.BackupDbContext>
{
    public BackupDbContext.BackupDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BackupDbContext.BackupDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("BackupDb"));

        return new BackupDbContext.BackupDbContext(optionsBuilder.Options);
    }
}