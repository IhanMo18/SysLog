
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using SysLog.Domine.Interfaces;
using SysLog.Domine.Repositories;
using SysLog.Domine.Services;
using SysLog.Repository.BackgroundServices;
using SysLog.Repository.Data;
using SysLog.Repository.Protocols;
using SysLog.Repository.Repositories;
using SysLog.Repository.Utilities;
using SysLog.Service;
using SysLog.Service.Interfaces;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Services;


string projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(Path.Combine(projectRoot, "SysLog.Infraestructure/Logs/log-.txt"), 
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var sysLogCs = builder.Configuration.GetConnectionString("SysLogDb");
var backupCs = builder.Configuration.GetConnectionString("BackupDb");


// Registrar ApplicationDbContext para logs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(sysLogCs));

// Registrar BackupDbContext para archivos de backup
builder.Services.AddDbContext<BackupDbContext>(options =>
    options.UseNpgsql(backupCs));

builder.Services.AddSingleton<IUdpProtocol, UdpProtocol>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IBackupFileRepository,BackupFileRepository>();

builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IBackupFileService,BackupFileService>();    
builder.Services.AddScoped<IJsonParser, JsonParser>();
builder.Services.AddScoped<IBackup,PostgreSqlServerBackup>();
builder.Services.AddHostedService<BackupService>();
builder.Services.AddHostedService<CatchLogsService>();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    loggingBuilder.AddSerilog(Log.Logger, dispose : true);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Logs}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();