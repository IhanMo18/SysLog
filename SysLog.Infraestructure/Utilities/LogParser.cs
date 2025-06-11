using SysLog.Domine.Interfaces;
using SysLog.Repository.Model;

namespace SysLog.Repository.Utilities;

/// <summary>
/// Combines multiple <see cref="ILogParseStrategy"/> implementations to parse log messages.
/// </summary>
public class LogParser : IJsonParser
{
    private readonly IEnumerable<ILogParseStrategy> _strategies;

    public LogParser(IEnumerable<ILogParseStrategy> strategies)
    {
        _strategies = strategies;
    }

    public Log Parse(string logMessage)
    {
        foreach (var strategy in _strategies)
        {
            if (strategy.TryParse(logMessage, out var log))
                return log;
        }

        throw new InvalidOperationException("No parser configured");
    }
}
