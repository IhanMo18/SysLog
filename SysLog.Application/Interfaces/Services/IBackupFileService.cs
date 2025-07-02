using SysLog.Shared.ModelDto;
using System;

namespace SysLog.Service.Interfaces.Services;

public interface IBackupFileService : IServiceDto<BackupFileDto>
{
    public int GetLastBackupFileDayTime();
    Task<BackupFileDto?> FindByDateAsync(string day);
    Task<List<BackupFileDto>> FindAllByDateAsync(string dayPrefix);
    Task<List<string>> GetAvailableDaysAsync();
}