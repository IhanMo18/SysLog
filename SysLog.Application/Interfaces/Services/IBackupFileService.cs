using SysLog.Shared.ModelDto;
using System;

namespace SysLog.Service.Interfaces.Services;

public interface IBackupFileService : IServiceDto<BackupFileDto>
{
    public int GetLastBackupFileDayTime();
    Task<BackupFileDto?> FindByDateAsync(DateTime date);
}