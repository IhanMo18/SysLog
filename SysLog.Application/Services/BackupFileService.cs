using SysLog.Domine.ModelDto;
using SysLog.Domine.Repositories;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces.Services;

namespace SysLog.Service.Services;

public class BackupFileService(IRepository<BackupFile> repository) : Service<BackupFileDto,BackupFile>(repository),IBackupFileService
{
    
}