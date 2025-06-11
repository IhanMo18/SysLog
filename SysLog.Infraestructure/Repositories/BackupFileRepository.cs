using SysLog.Domine.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Model;

namespace SysLog.Repository.Repositories;

public class BackupFileRepository(BackupDbContext backupDbContext): Repository<BackupFile>(backupDbContext),IBackupFileRepository
{
    
}