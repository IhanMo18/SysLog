namespace SysLog.Domine.Interfaces;

public interface IParser<out T,Y>
{
    T Parse(Y input);
}