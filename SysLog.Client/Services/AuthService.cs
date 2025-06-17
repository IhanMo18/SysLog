using Microsoft.AspNetCore.Components.Authorization;
using SysLog.Client.Client;
using SysLog.Shared;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class AuthService
{
    private readonly CookieAuthProvider _authProvider;   
    private readonly ClientSideApi _client;

    public AuthService(AuthenticationStateProvider asp, ClientSideApi client)
    {
        _authProvider = (CookieAuthProvider)asp;
        _client       = client;
    }

    public async Task<TaskResult> SignIn(string email, string password)
    {
        var loginDto = new UserLoginDto(email, password);
        var loginRes = await _client.CallApiAsync<string>("api/user/login", "POST", loginDto);

        if (!loginRes.Value.IsSuccessful(out _))
            return TaskResult.FromFailure(loginRes.Value.Message);

        var meRes = await _client.CallApiAsync<CurrentUserDto>("api/user/me", "GET");
        if (meRes.Value.IsSuccessful(out var me))
            _authProvider.NotifyLogin(me);    

        return TaskResult.FromSuccess(loginRes.Value.Message);
    }

    public async Task Logout()
    {
        await _client.CallApiAsync<string>("api/user/logout", "POST");
        _authProvider.NotifyLogout();          // actualiza estado
    }
}
