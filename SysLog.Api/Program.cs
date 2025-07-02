
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using SysLog.Domain.Model;
using SysLog.Domine.Interfaces;
using SysLog.Domine.Interfaces.Repositories;
using SysLog.Repository.BackgroundServices;
using SysLog.Repository.Data;
using SysLog.Repository.Data.BackupDbContext;
using SysLog.Repository.Model;
using SysLog.Repository.Protocols;
using SysLog.Repository.Repositories;
using SysLog.Repository.Service;
using SysLog.Repository.Utilities;
using SysLog.Repository.Utilities.Parsing;
using SysLog.Service.Interfaces;
using SysLog.Service.Interfaces.Services;
using SysLog.Service.Services;
using BackupService = SysLog.Client.Services.BackupService;
using Log = Serilog.Log;
using Microsoft.AspNetCore.Builder;


string projectRoot = Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.Parent!.FullName;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(Path.Combine(projectRoot, "SysLog.Infraestructure/Logs/log-.txt"), 
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
var sysLogCs = builder.Configuration.GetConnectionString("SysLogDb");
var backupCs = builder.Configuration.GetConnectionString("BackupDb");

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", p => p
        .WithOrigins(
            "https://localhost:5002",
            "http://localhost:5001",
            "https://localhost:7182")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

//Identity
builder.Services.AddIdentity<AppUser,IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
    options.Cookie.Name      = "__syslog.auth";
    options.Cookie.HttpOnly  = true;
    options.Cookie.SameSite  = SameSiteMode.Strict;   
    options.LoginPath        = "/login";              // usado si navegas a Razor Pages
    options.ExpireTimeSpan   = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});    

// Registrar ApplicationDbContext para logs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(sysLogCs));

// Registrar BackupDbContext para archivos de backup
builder.Services.AddDbContext<BackupDbContext>(options =>
    options.UseNpgsql(backupCs));


builder.Services.AddSingleton<IUdpProtocol, UdpProtocol>();
builder.Services.AddScoped<IRepository<BackupFile>,BackupFileRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IBackupFileRepository,BackupFileRepository>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IBackupFileService,BackupFileService>();    
// Register log parsing strategies and the composite parser
builder.Services.AddScoped<ILogParseStrategy, JsonLogParseStrategy>();
builder.Services.AddScoped<ILogParseStrategy, FilterLogParseStrategy>();
builder.Services.AddScoped<ILogParseStrategy, CustomFilterLogParseStrategy>();
builder.Services.AddScoped<ILogParseStrategy, CronLogParseStrategy>();
builder.Services.AddScoped<ILogParseStrategy, SyslogdLogParseStrategy>();
builder.Services.AddScoped<ILogParseStrategy, UnknownLogParseStrategy>();
builder.Services.AddScoped<IJsonParser, LogParser>();
builder.Services.AddScoped<IBackup,PostgreSqlServerBackup>();
    //builder.Services.AddHostedService<BackupService>();
builder.Services.AddHostedService<CatchLogsService>();
builder.Services.AddHostedService<BackupBackgroundService>();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    loggingBuilder.AddSerilog(Log.Logger, dispose : true);
});

var app = builder.Build();

// Ensure the backup database exists and has the backup_file table
using (var scope = app.Services.CreateScope())
{
    var backupCtx = scope.ServiceProvider.GetRequiredService<BackupDbContext>();
    backupCtx.Database.EnsureCreated();
    backupCtx.Database.ExecuteSqlRaw(
        @"CREATE TABLE IF NOT EXISTS backup_file(
        ""Id"" SERIAL PRIMARY KEY,
        ""PathFile"" TEXT NOT NULL,
        ""FileName"" TEXT NOT NULL)"
    );
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseCors("BlazorClient");
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapStaticAssets();
app.MapFallbackToFile("index.html");
app.MapControllers();

app.Run();

