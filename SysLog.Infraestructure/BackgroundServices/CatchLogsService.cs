
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SysLog.Domine.Interfaces;
using SysLog.Domine.ModelDto;
using SysLog.Repository.Model;
using SysLog.Service.Interfaces.Mappers;
using SysLog.Service.Interfaces.Services;

namespace SysLog.Repository.BackgroundServices;

public class CatchLogsService : BackgroundService
{
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CatchLogsService> _logger;

    public CatchLogsService(IServiceProvider serviceProvider, ILogger<CatchLogsService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        var scope = _serviceProvider.CreateScope();
        var protocol = _serviceProvider.GetRequiredService<IUdpProtocol>();
        protocol.Start();
        
        while (true)
        {
            try
            {
                var logService = scope.ServiceProvider.GetRequiredService<ILogService>();
                var parser = scope.ServiceProvider.GetRequiredService<IJsonParser>();
                var logMessage = await protocol.CatchLog();
                
                // Intenta Parsear JSON a Log
                var log = parser.Parse(logMessage);

                var logDto = MapperTo.Map<Log, LogDto>(log);

                // Guardar en la base de datos si hay logs válidos
                await logService.AddAsync(logDto);
                await logService.SaveAsync();
                
                Console.WriteLine(logMessage);
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Error al procesar JSON: {jsonEx.Message}");
                _logger.LogError(jsonEx,$"Error al procesar JSON: {jsonEx.Message}");
            }
            catch (FormatException fmtEx)
            {
                Console.WriteLine($"Error en formato de fecha u otro valor: {fmtEx.Message}");
                _logger.LogError(fmtEx,$"Error en formato de fecha u otro valoR: {fmtEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error general: {ex.Message}");
                _logger.LogError($"Error general: {ex.Message}");
            }
        }
    }

    
}