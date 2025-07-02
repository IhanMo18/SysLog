using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Data;
using SysLog.Repository.Model;
using Microsoft.EntityFrameworkCore;
using System.IO;
using SysLog.Repository.Data.BackupDbContext;

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

    
    public async Task<BackupFile?> FindByDateAsync(string day)
    {
        return await backupDbContext.BackupFile
            .FirstOrDefaultAsync(f => f.FileName.StartsWith(day));
    }

    public async Task<List<BackupFile>> FindAllByDateAsync(string dayPrefix)
    {
        return await backupDbContext.BackupFile
            .Where(f => f.FileName.StartsWith(dayPrefix))
            .ToListAsync();
    }

    public async Task<List<string>> GetAvailableDaysAsync()
    {
        return await backupDbContext.BackupFile
            .Select(f => f.FileName.Substring(0, 8))
            .Distinct()
            .ToListAsync();
    }
}
