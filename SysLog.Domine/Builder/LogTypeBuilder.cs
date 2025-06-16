using SysLog.Domine.Interface;
using SysLog.Repository.Model;

namespace SysLog.Domine.Builder;

public class LogTypeBuilder :ILogTypeBuilder
{
    private Signature? _signature;
    private string _typeName { get; set; }
    
    public ILogTypeBuilder WhitSignature(string signature)
    {
        if (!string.IsNullOrWhiteSpace(signature))
        {
            _signature = new Signature() { Message = signature };
        }
        return this;
    }

    public ILogTypeBuilder WithType(string typeName)
    {
        _typeName = typeName;
        return this;
    }
    
    public LogType Build()
    {
        var typeName = string.IsNullOrEmpty(_typeName) ? "N/A" : _typeName;
        return _signature is null
            ? new LogType() { TypeName = typeName }
            : new LogType() { TypeName = typeName, Signature = _signature };
    }
}