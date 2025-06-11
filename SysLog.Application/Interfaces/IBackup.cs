namespace SysLog.Service.Interfaces;

public interface IBackup
{
    Task<string> BackupAsync();
}