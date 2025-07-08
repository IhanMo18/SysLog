using SysLog.Domine.Interfaces.Repositories;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Mappers;
using SysLog.Shared.ModelDto;

namespace SysLog.Service.Services;

public class BackupLoaderService(IBackupLoaderRepository repository) : IBackupLoaderService
{
    private readonly IBackupLoaderRepository _repository = repository;

    public async Task<List<LogDto>> LoadBackupAsync(string day)
    {
        var logs = await _repository.LoadBackupAsync(day);
        return logs.Select(MapperLog.MapToLogDto).ToList();
    }

    public async Task<List<LogDto>> LoadCurrentLogsAsync()
    {
        var logs = await _repository.LoadCurrentLogsAsync();
        return logs.Select(MapperLog.MapToLogDto).ToList();
    }
}
