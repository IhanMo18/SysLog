using SysLog.Domine.Interfaces.Repositories;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Mappers;
using SysLog.Shared.ModelDto;
using SysLog.Shared;

namespace SysLog.Service.Services;

public class BackupLoaderService(IBackupLoaderRepository repository) : IBackupLoaderService
{
    
    public async Task<List<LogDto>> LoadBackupAsync(string day)
    {
        var logs = await repository.LoadBackupAsync(day);
        return logs.Select(MapperLog.MapToLogDto).ToList();
    }

    public async Task<List<LogDto>> LoadCurrentLogsAsync()
    {
        var logs = await repository.LoadCurrentLogsAsync();
        return logs.Select(MapperLog.MapToLogDto).ToList();
    }

    public async Task<PagedResult<LogDto>> LoadBackupPagedAsync(string day, int page, int pageSize)
    {
        var result = await repository.LoadBackupPagedAsync(day, page, pageSize);
        return new PagedResult<LogDto>
        {
            Items = result.Items.Select(MapperLog.MapToLogDto).ToList(),
            TotalItems = result.TotalItems,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<PagedResult<LogDto>> LoadCurrentLogsPagedAsync(int page, int pageSize)
    {
        var result = await repository.LoadCurrentLogsPagedAsync(page, pageSize);
        return new PagedResult<LogDto>
        {
            Items = result.Items.Select(MapperLog.MapToLogDto).ToList(),
            TotalItems = result.TotalItems,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }
}
