using SysLog.Repository.Model;

namespace SysLog.Domine.Interfaces.Repositories;

public interface IBackupLoaderRepository
{
    Task<List<Log>> LoadBackupAsync(string day);
    Task<List<Log>> LoadCurrentLogsAsync();
}
