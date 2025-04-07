using SysLog.Domine.Interface;
using SysLog.Domine.Interface.Repository;
using SysLog.Domine.Interface.Service;

namespace SysLog.Service;

public class Service<T>(IRepository<T> repository) :IService<T>   where T : class
{
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
       return await repository.GetByIdAsync(id);
    }

    public void Update(T obj)
    {
        throw new NotImplementedException();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }

    public void Remove(T obj)
    {
        throw new NotImplementedException();
    }
}
