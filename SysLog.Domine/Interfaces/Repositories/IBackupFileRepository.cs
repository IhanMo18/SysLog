using SysLog.Repository.Model;

namespace SysLog.Domine.Interfaces.Repositories;

public interface IBackupFileRepository : IRepository<BackupFile>
{
    public int GetLastBackupFileDayTime();
}