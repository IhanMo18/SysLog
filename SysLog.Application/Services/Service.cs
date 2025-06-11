using SysLog.Domine.Repositories;
using SysLog.Service.Interfaces.Mappers;
using SysLog.Service.Interfaces.Services;

namespace SysLog.Service.Services;

public abstract class Service<TDto,TEntity>(IRepository<TEntity> repository) :IServiceDto<TDto> where TEntity : class, new() where TDto : new()
{
    
    public async Task<IEnumerable<TDto>> GetAllAsync()
    {
        var entities = await repository.GetAllAsync();
        return entities.Select(MapperTo.Map<TEntity,TDto>);
    }

    public async Task<TDto?> GetByIdAsync(int id)
    {
       var entities = await repository.GetByIdAsync(id);
       return MapperTo.Map<TEntity,TDto>(entities);
    }

    public async Task AddAsync(TDto dto)
    {
       var entity = MapperTo.Map<TDto,TEntity>(dto);
       await repository.AddAsync(entity);
    }

    public async Task Update(TDto dto)
    {
        var entity = MapperTo.Map<TDto,TEntity>(dto);
        await repository.Update(entity);
    }

    public async Task SaveAsync()
    {
       await repository.SaveAsync();
    }

    public async Task Remove(TDto dto)
    {
        var entity = MapperTo.Map<TDto,TEntity>(dto); 
        await repository.Remove(entity);
    }
}
