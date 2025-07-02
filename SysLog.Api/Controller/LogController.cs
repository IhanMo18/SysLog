using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Mappers;
using SysLog.Repository.Data;
using SysLog.Repository.Repositories;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
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
    

    [HttpGet("days")]
    public async Task<ActionResult<IEnumerable<string>>> GetAllsBackups()
    {
        var list = await _backupFileService.GetAvailableDaysAsync();
        return Ok(list);
    }

    [HttpGet("backup")]
    public async Task<IActionResult> LoadBackupDay([FromQuery] string day)
    {
        var backups = await _backupFileService.FindAllByDateAsync(day);
        if (backups == null || backups.Count == 0)
            return NotFound();

        var cs = _configuration.GetConnectionString("BackupDb")!;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(cs)
            .Options;

        using var ctx = new ApplicationDbContext(options);
        await ctx.Database.EnsureCreatedAsync();

        await CleanupBackupAsync(ctx);

        foreach (var backup in backups.OrderBy(b => b.FileName))
        {
            var scriptPath = Path.Combine(backup.PathFile, backup.FileName);
            var sql = await System.IO.File.ReadAllTextAsync(scriptPath);
            await ctx.Database.ExecuteSqlRawAsync(sql);
        }

        return Ok();
    }
    private async Task CleanupBackupAsync(ApplicationDbContext ctx)
    {
        await using var conn = (Npgsql.NpgsqlConnection)ctx.Database.GetDbConnection();
        await conn.OpenAsync();
        var tables = new List<string>();
        await using (var cmd = new Npgsql.NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE' AND table_name NOT IN ('backup_file','__EFMigrationsHistory');", conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                tables.Add(reader.GetString(0));
        }

        foreach (var table in tables)
            await ctx.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS \"{table}\" CASCADE;");
    }
    
    
    
    
}