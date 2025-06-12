using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces.Services;
using SysLog.Shared.ModelDto;

namespace SysLog.Service.Services;

public class BackupFileService(IBackupFileRepository repository) : Service<BackupFileDto,BackupFile>(repository),IBackupFileService
{
    public int GetLastBackupFileDayTime()
    {
        return repository.GetLastBackupFileDayTime();
    }
}