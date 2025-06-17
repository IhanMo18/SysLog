using Microsoft.AspNetCore.Components.Authorization;
using SysLog.Client.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SysLog.Client;
using SysLog.Client.Client;
using SysLog.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddScoped(_ =>
{
    var http = new HttpClient()
    {
        BaseAddress = new Uri("https://localhost:7167/"),
        DefaultRequestHeaders =
        {
            { "X-Requested-With", "XMLHttpRequest" }
        }
    };
    return http;
});

// Configurar autenticación
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthenticationStateProvider,CookieAuthProvider>();     
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ClientSideApi>();
builder.Services.AddScoped<LogService>();

await builder.Build().RunAsync();
