using SysLog.Repository.Model;

namespace SysLog.Domine.Interface;

public interface ILogTypeBuilder: IBuilder<LogType>
{
    public ILogTypeBuilder WhitSignature(string signature);
    public ILogTypeBuilder WithType(string typeName);
}