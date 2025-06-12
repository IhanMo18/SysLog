using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Model;
using System.IO;

namespace SysLog.Repository.Repositories;

public class BackupFileRepository(BackupDbContext backupDbContext): Repository<BackupFile>(backupDbContext),IBackupFileRepository
{
    public int GetLastBackupFileDayTime()
    {
        var file = backupDbContext.BackupFile
            .OrderByDescending(f => f.Id)
            .FirstOrDefault();

        if (file is null)
            return 0;

        var name = Path.GetFileNameWithoutExtension(file.FileName);
        if (name.Length >= 8 && int.TryParse(name.Substring(6, 2), out var day))
            return day;

        return 0;
    }
}
