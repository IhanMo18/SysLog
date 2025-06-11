using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SysLog.Repository.Data;

public class BackupDbContextFactory : IDesignTimeDbContextFactory<BackupDbContext>
{
    public BackupDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<BackupDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("BackupDb"));

        return new BackupDbContext(optionsBuilder.Options);
    }
}