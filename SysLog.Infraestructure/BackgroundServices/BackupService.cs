using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.EntityFrameworkCore;
using SysLog.Repository.Data;
using SysLog.Service.Interfaces;
using SysLog.Service.Interfaces.Services;
using SysLog.Shared.ModelDto;

namespace SysLog.Repository.BackgroundServices;

public class BackupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private ILogService _logService;
    private IBackupFileService _backupFileService;
    private readonly ILogger<BackupService> _logger;

    public BackupService(IServiceProvider serviceProvider, ILogger<BackupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = _serviceProvider.CreateScope();
        var backup = scope.ServiceProvider.GetRequiredService<IBackup>();
        _logService = scope.ServiceProvider.GetRequiredService<ILogService>();
        _backupFileService = scope.ServiceProvider.GetRequiredService<IBackupFileService>();

        // Ensure the backup database and table are ready before running the loop
        var backupCtx = scope.ServiceProvider.GetRequiredService<BackupDbContext>();
        await backupCtx.Database.EnsureCreatedAsync(stoppingToken);
        backupCtx.Database.ExecuteSqlRaw(
            @"CREATE TABLE IF NOT EXISTS backup_file (
        ""Id"" SERIAL PRIMARY KEY,
        ""PathFile"" TEXT NOT NULL,
        ""FileName"" TEXT NOT NULL
    );"
        );

        
        while (!stoppingToken.IsCancellationRequested)
        {
           await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

           var lastLog = await _logService.GetLastLogAsync();
           var lastBackupFile = _backupFileService.GetLastBackupFileDayTime();

           if (lastLog == null || lastLog.DateTime.Day == lastBackupFile)
               continue;

           try
           {
               var path =  await backup.BackupAsync();
               var backupFileDto = new BackupFileDto()
               {
                   PathFile = Path.GetDirectoryName(path)!,
                   FileName = Path.GetFileName(path)
               };
               await _backupFileService.AddAsync(backupFileDto);
               await _backupFileService.SaveAsync();
               _logger.LogInformation("Backup saved to {Path}", path);
               
               await _logService.RemoveAllLogsAsync();
               _logger.LogInformation("All Logs has been Deleted successfully");
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error storing backup record  or  Deleting all Logs");
           }
        }
        
    }
}