namespace SysLog.Domine.Interface;

public interface IBuilder<out T>
{
    public T Build();
}