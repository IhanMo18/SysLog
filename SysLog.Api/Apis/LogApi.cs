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
}