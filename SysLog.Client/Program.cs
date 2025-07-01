using Microsoft.AspNetCore.Components.Authorization;
using SysLog.Client.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SysLog.Client;
using SysLog.Client.Services;
using FluentValidation;
using Blazored.FluentValidation;
using SysLog.Client.Validators;
using SysLog.Repository.BackgroundServices;
using BackupService = SysLog.Client.Services.BackupService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(_ =>
{
    var http = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7167/"),
        DefaultRequestHeaders =
        {
            { "X-Requested-With", "XMLHttpRequest" }
        }
    };
    return http;
});

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

builder.Services.AddValidatorsFromAssemblyContaining<UserLoginValidator>();
builder.Services.AddScoped<AuthProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClientSideApi>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<BackupService>();

await builder.Build().RunAsync();