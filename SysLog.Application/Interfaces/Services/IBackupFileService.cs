using SysLog.Shared.ModelDto;

namespace SysLog.Service.Interfaces.Services;

public interface IBackupFileService : IServiceDto<BackupFileDto>
{
    public int GetLastBackupFileDayTime();
}