using SysLog.Domine.Interfaces.Repositories;
using SysLog.Domine.ModelDto;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces.Services;

namespace SysLog.Service.Services;

public class BackupFileService(IBackupFileRepository repository) : Service<BackupFileDto,BackupFile>(repository),IBackupFileService
{
    public int GetLastBackupFileDayTime()
    {
        return repository.GetLastBackupFileDayTime();
    }
}