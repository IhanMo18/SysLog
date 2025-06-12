using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Model;

namespace SysLog.Repository.Repositories;

public class BackupFileRepository(BackupDbContext backupDbContext): Repository<BackupFile>(backupDbContext),IBackupFileRepository
{
    public int GetLastBackupFileDayTime()
    {
       var file = backupDbContext.BackupFile.Last();
       var dayString = file.FileName[6..];
       return Convert.ToInt32(dayString);
    }
}