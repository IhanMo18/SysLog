using Microsoft.AspNetCore.Mvc;
using SysLog.Domine.ModelDto;
using SysLog.Service.Interfaces.Services;


namespace LogUdp.Apis;


[ApiController]
[Route("api/[controller]")] 
public class LogApi : ControllerBase    
{
    private ILogService _logService;
    
    
    

    public LogApi(ILogService logService)
    {
        _logService = logService;
    }


    [HttpGet("last")]
    public async Task<ActionResult<LogDto>> GetLastLog()
    {
        var result = await _logService.GetLastLogAsync();
        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LogDto>>> GetPagedLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var logs = await _logService.GetPagedLogsAsync(page, pageSize);
        return Ok(logs);
    }
}