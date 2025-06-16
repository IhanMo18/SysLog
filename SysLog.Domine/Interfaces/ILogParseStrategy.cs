namespace SysLog.Domine.Interfaces;

using SysLog.Repository.Model;

/// <summary>
/// Strategy for parsing a log message into a <see cref="Log"/> entity.
/// Returns true when the parser can handle the message.
/// </summary>
public interface ILogParseStrategy
{
    bool TryParse(string message, out Log log);
}
