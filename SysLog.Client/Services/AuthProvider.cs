using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SysLog.Client.Client;
using SysLog.Shared;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class AuthProvider : AuthenticationStateProvider
{
    private readonly ClientSideApi _client;
    private CurrentUserDto? _currentUser;

    public AuthProvider(ClientSideApi clientSideApi)
    {
        _client = clientSideApi;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        var result = await GetCurrentUser();
        if (result.IsSuccessful(out var user) && user.IsAuthenticated)
        {
            _currentUser = user;
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };
            identity = new ClaimsIdentity(claims, "serverAuth");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public  async Task<TaskResult<CurrentUserDto>> GetCurrentUser()
    {
        var result = await _client.CallApiAsync<CurrentUserDto>("api/user/me", "GET");
        if (result.Value.IsSuccessful(out var user))
        {
            return TaskResult<CurrentUserDto>.FromData(user);
        }

        return TaskResult<CurrentUserDto>.FromFailure(result.Value.Message);
    }

    public async Task<TaskResult> SignIn(string email, string password)
    {
        var dto = new UserLoginDto(email, password);
        var result = await _client.CallApiAsync<string>("api/user/login", "POST", dto);
        
        if (!result.Value.IsSuccessful(out _))
            return TaskResult.FromFailure(result.Value.Message);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return TaskResult.FromSuccess(result.Value.Message);
    }

    public async Task Logout()
    {
        await _client.CallApiAsync<string>("api/user/logout", "POST");
        _currentUser = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<TaskResult> SignUp(string username,string email, string password)
    {
        var registerDto = new UserDto(username,email,password);
        var registerRes = await _client.CallApiAsync<string>("api/user/register", "POST", registerDto);

        if (!registerRes.Value.IsSuccessful(out _))
            return TaskResult.FromFailure(registerRes.Value.Message);
        
        var meRes = await _client.CallApiAsync<CurrentUserDto>("api/user/me", "GET");
        if (meRes.Value.IsSuccessful(out var me))
           await SignIn(email,password);    

        return TaskResult.FromSuccess(registerRes.Value.Message);
    }

    public CurrentUserDto? CurrentUser => _currentUser;
}
