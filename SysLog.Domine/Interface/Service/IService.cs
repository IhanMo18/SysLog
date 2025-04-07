namespace SysLog.Domine.Interface.Service;

public interface IService<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    void Update(T obj);
    void Save();
    public void Remove(T obj);
}