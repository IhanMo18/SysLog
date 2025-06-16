using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Mappers;
using SysLog.Shared.ModelDto;

namespace SysLog.Service.Services;

public class LogService(ILogRepository repository) : Service<LogDto,Log>(repository),ILogService
{
    public Task<IEnumerable<LogDto>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<LogDto> GetLastLogAsync()
    {
        var entity = await repository.getLastLogAsync();
        if (entity is null) return null;
        return MapperLog.MapToLogDto(entity);
    }

    public Task RemoveAllLogsAsync()
    {
        return repository.RemoveAllLogsAsync();
    }

    public Task RemoveAllLogsWithPropertiesAsync()
    {
        return repository.RemoveAllLogsWithPropertiesAsync();
    }

    public async Task<IEnumerable<LogDto>> GetPagedLogsAsync(int page, int pageSize)
    {
        var entities = await repository.GetPagedLogsAsync(page, pageSize);
        return entities.Select(MapperLog.MapToLogDto);
    }
}