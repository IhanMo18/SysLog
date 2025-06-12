using SysLog.Domine.Interfaces.Repositories;
using SysLog.Domine.ModelDto;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Mappers;

namespace SysLog.Service.Services;

public class LogService(ILogRepository repository) : Service<LogDto,Log>(repository),ILogService
{
    
    public async Task<LogDto> GetLastLogAsync()
    {
        var entity = await repository.getLastLogAsync();
        return MapperTo.Map<Log, LogDto>(entity);
    }

    public Task RemoveAllLogsAsync()
    {
        return repository.RemoveAllLogsAsync();
    }

    public async Task<IEnumerable<LogDto>> GetPagedLogsAsync(int page, int pageSize)
    {
        var entities = await repository.GetPagedLogsAsync(page, pageSize);
        return entities.Select(MapperTo.Map<Log, LogDto>);
    }
}