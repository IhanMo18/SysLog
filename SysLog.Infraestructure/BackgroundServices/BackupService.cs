using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SysLog.Domine.ModelDto;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces;
using SysLog.Service.Interfaces.Services;

namespace SysLog.Repository.BackgroundServices;

public class BackupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private ILogService _logService;
    private IBackupFileService _backupFileService;

    public BackupService(IServiceProvider serviceProvider, ILogService logService, IBackupFileService backupFileService)
    {
        _serviceProvider = serviceProvider;
        _logService = logService;
        _backupFileService = backupFileService;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scope = _serviceProvider.CreateScope();
        var backup = scope.ServiceProvider.GetRequiredService<IBackup>();
        _logService = scope.ServiceProvider.GetRequiredService<ILogService>();
        _backupFileService = scope.ServiceProvider.GetRequiredService<IBackupFileService>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
           await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            var lastLog = await _logService.GetLastLogAsync();

            if (lastLog.DateTime.Minute == DateTime.Now.Minute) continue;
            
           
            var path =  await backup.BackupAsync();
            _logService.RemoveAllLogs();
            
            var backupFileDto = new BackupFileDto()
            {
                PathFile = path,
                FileName = path
            };
           await _backupFileService.AddAsync(backupFileDto);
        }
        
    }
}