namespace SysLog.Service.Interfaces.Services;

public interface IServiceDto<TDto>
{ 
        Task<IEnumerable<TDto>> GetAllAsync();
        Task<TDto?> GetByIdAsync(int id);
        Task AddAsync(TDto dto);
        Task Update(TDto dto);
        Task SaveAsync();
        Task Remove(TDto dto);
    
}