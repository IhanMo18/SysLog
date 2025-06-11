using SysLog.Domine.Interface;
using SysLog.Repository.Model;

namespace SysLog.Domine.Builder;

public class LogTypeBuilder :ILogTypeBuilder
{
    private Signature _signature;
    private string _typeName { get; set; }
    
    public ILogTypeBuilder WhitSignature(string signature)
    {
        _signature = new Signature() { Message = signature };
        return this;
    }

    public ILogTypeBuilder WithType(string typeName)
    {
        _typeName = typeName;
        return this;
    }
    
    public LogType Build()
    { 
        return string.IsNullOrEmpty(_typeName) ? new LogType(){TypeName ="N/A", Signature = _signature} : new LogType(){TypeName = _typeName, Signature = _signature};
    }
}