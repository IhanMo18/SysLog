using SysLog.Shared.ModelDto;

namespace SysLog.Service.Interfaces.Services;

public interface IBackupLoaderService
{
    Task<List<LogDto>> LoadBackupAsync(string day);
    Task<List<LogDto>> LoadCurrentLogsAsync();
}
