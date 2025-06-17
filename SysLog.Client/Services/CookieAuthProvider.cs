
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using SysLog.Client.Client;
using SysLog.Shared;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class CookieAuthProvider : AuthenticationStateProvider
{
    private readonly ClientSideApi _client;
    private readonly ClaimsPrincipal _anon = new(new ClaimsIdentity());

    public CookieAuthProvider(ClientSideApi client) => _client = client;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var resp = await _client.CallApiAsync<CurrentUserDto>("api/user/me", "GET");

        if (resp.Value.IsSuccessful(out var user) && user.IsAuthenticated)
        {
            var identity = new ClaimsIdentity(
                user.Claims.Select(c => new Claim(c.Type, c.Value)),
                CookieAuthenticationDefaults.AuthenticationScheme);

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        return new AuthenticationState(_anon);
    }

    public void NotifyLogin(CurrentUserDto user)
    {
        var identity = new ClaimsIdentity(
            user.Claims.Select(c => new Claim(c.Type, c.Value)),
            CookieAuthenticationDefaults.AuthenticationScheme);

        var authState = Task.FromResult(new AuthenticationState(
            new ClaimsPrincipal(identity)));

        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyLogout()
    {
        var authState = Task.FromResult(new AuthenticationState(_anon));
        NotifyAuthenticationStateChanged(authState);
    }
}


