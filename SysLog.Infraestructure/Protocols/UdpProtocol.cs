
using System.Net.Sockets;
using System.Text;
using SysLog.Domine.Interfaces;

namespace SysLog.Repository.Protocols;

public class UdpProtocol : IUdpProtocol
{
    private UdpClient _listener;
    public void Start()
    {
        _listener = new UdpClient(514);
    }

    public async Task<string> CatchLog()
    {
        var received = await _listener.ReceiveAsync();
        var logMessage = Encoding.ASCII.GetString(received.Buffer);
        return logMessage;
    }
}
