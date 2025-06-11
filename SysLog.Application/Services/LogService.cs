using SysLog.Domine.ModelDto;
using SysLog.Domine.Repositories;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces.Mappers;
using SysLog.Service.Interfaces.Services;
namespace SysLog.Service.Services;

public class LogService(ILogRepository repository) : Service<LogDto,Log>(repository),ILogService
{
    
    public async Task<LogDto> GetLastLogAsync()
    {
        var entity = await repository.getLastLogAsync();
        return  MapperTo.Map<Log,LogDto>(entity);     
    }

    public void RemoveAllLogs()
    {
        repository.RemoveAllLogs();
    }
}