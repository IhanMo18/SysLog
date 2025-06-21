using SysLog.Repository.Model;
using System;

namespace SysLog.Domine.Interfaces.Repositories;

public interface IBackupFileRepository : IRepository<BackupFile>
{
    public int GetLastBackupFileDayTime();
    Task<BackupFile?> FindByDateAsync(string day);
}