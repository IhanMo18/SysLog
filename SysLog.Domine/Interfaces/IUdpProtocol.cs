

namespace SysLog.Domine.Interfaces;

public interface IUdpProtocol : IProtocol
{
    Task<string> CatchLog();
}