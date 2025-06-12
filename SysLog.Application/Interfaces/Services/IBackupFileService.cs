using SysLog.Domine.ModelDto;

namespace SysLog.Service.Interfaces.Services;

public interface IBackupFileService : IServiceDto<BackupFileDto>
{
    public int GetLastBackupFileDayTime();
}