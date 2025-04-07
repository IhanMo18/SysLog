using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LogUdp.Models;
using SysLog.Domine.Interface.Service;
using SysLog.Domine.Model;

namespace LogUdp.Controllers;

[Route("api/v1/logs")]
public class LogsController(ILogger<LogsController> _logger,ILogService logService) : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Log>>> GetAllLogs()
    {
        return Ok(await logService.GetAllAsync());
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<Log>>> GetLogById(int id)
    {
        return Ok(await logService.GetByIdAsync(id));
    }
}