using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Mappers;
using SysLog.Repository.Data;
using SysLog.Repository.Repositories;
using System.IO;
using SysLog.Shared.ModelDto;


namespace LogUdp.Apis;


[ApiController]
[Route("api/[controller]")] 
public class LogController : ControllerBase
{
    private readonly ILogService _logService;
    private readonly IBackupFileService _backupFileService;
    private readonly IConfiguration _configuration;
    
    
    

    public LogController(ILogService logService, IBackupFileService backupFileService, IConfiguration configuration)
    {
        _logService = logService;
        _backupFileService = backupFileService;
        _configuration = configuration;
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
        await CleanupBackupAsync();
        var logs = await _logService.GetPagedLogsAsync(page, pageSize);
        return Ok(logs);
    }

    [HttpGet("backup")]
    public async Task<ActionResult<IEnumerable<LogDto>>> GetBackupLogs([FromQuery] DateTime day, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var backup = await _backupFileService.FindByDateAsync(day);
        if (backup == null)
            return NotFound();

        var cs = _configuration.GetConnectionString("BackupDb")!;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(cs)
            .Options;

        using var ctx = new ApplicationDbContext(options);
        await ctx.Database.EnsureCreatedAsync();

        await CleanupBackupAsync();

        var scriptPath = Path.Combine(backup.PathFile, backup.FileName);
        var sql = await System.IO.File.ReadAllTextAsync(scriptPath);
        await ctx.Database.ExecuteSqlRawAsync(sql);

        var repo = new LogRepository(ctx);
        var entities = await repo.GetPagedLogsAsync(page, pageSize);
        var result = entities.Select(MapperLog.MapToLogDto);
        return Ok(result);
    }

    private async Task CleanupBackupAsync()
    {
        var cs = _configuration.GetConnectionString("BackupDb")!;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(cs)
            .Options;
        using var ctx = new ApplicationDbContext(options);
        const string sql = @"
DROP TABLE IF EXISTS \"signatures\" CASCADE;
DROP TABLE IF EXISTS \"logs_type\" CASCADE;
DROP TABLE IF EXISTS \"actions\" CASCADE;
DROP TABLE IF EXISTS \"interfaces\" CASCADE;
DROP TABLE IF EXISTS \"protocols\" CASCADE;
DROP TABLE IF EXISTS \"logs\" CASCADE;";
        await ctx.Database.ExecuteSqlRawAsync(sql);
    }
}