using Microsoft.AspNetCore.Mvc;
using SysLog.Service.Interfaces.Services;
using SysLog.Repository.Data;
using SysLog.Repository.Repositories;
using System.Collections.Generic;
using System.Linq;
using SysLog.Shared.ModelDto;
using SysLog.Shared;



namespace LogUdp.Apis;


[ApiController]
[Route("api/[controller]")] 
public class LogController : ControllerBase
{
    private readonly ILogService _logService;
    private readonly IBackupFileService _backupFileService;
    private readonly IBackupLoaderService _backupLoaderService;
    
    
    

    public LogController(ILogService logService, IBackupFileService backupFileService, IBackupLoaderService backupLoaderService)
    {
        _logService = logService;
        _backupFileService = backupFileService;
        _backupLoaderService = backupLoaderService;
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
    public async Task<ActionResult<PagedResult<LogDto>>> GetPagedLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var logs = await _logService.GetPagedLogsAsync(page, pageSize);
        return Ok(logs);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<LogDto>>> Search([FromQuery] string? property, [FromQuery] string? term, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var logs = await _logService.SearchLogsAsync(property, term, page, pageSize);
        return Ok(logs);
    }
    

    [HttpGet("days")]
    public async Task<ActionResult<IEnumerable<string>>> GetAllsBackups()
    {
        var list = await _backupFileService.GetAvailableDaysAsync();
        return Ok(list);
    }

    [HttpGet("backup")]
    public async Task<ActionResult<IEnumerable<LogDto>>> LoadBackupDay([FromQuery] string day)
    {
        var logs = await _backupLoaderService.LoadBackupAsync(day);
        return Ok(logs);
    }

    [HttpGet("backup-paged")]
    public async Task<ActionResult<PagedResult<LogDto>>> LoadBackupDayPaged([FromQuery] string day, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var logs = await _backupLoaderService.LoadBackupPagedAsync(day, page, pageSize);
        return Ok(logs);
    }

    [HttpGet("current")]
    public async Task<ActionResult<IEnumerable<LogDto>>> LoadCurrentLogs()
    {
        var logs = await _backupLoaderService.LoadCurrentLogsAsync();
        return Ok(logs);
    }

    [HttpGet("current-paged")]
    public async Task<ActionResult<PagedResult<LogDto>>> LoadCurrentLogsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var logs = await _backupLoaderService.LoadCurrentLogsPagedAsync(page, pageSize);
        return Ok(logs);
    }
    
    
    
    }