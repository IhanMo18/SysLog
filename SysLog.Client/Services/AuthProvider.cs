// CustomAuthStateProvider.cs

using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using SysLog.Client.Client;
using SysLog.Shared;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class AuthProvider 
{
    private readonly ClientSideApi _client;

    public AuthProvider(ClientSideApi clientSideApi)
    {
        _client = clientSideApi;
    }

    public async Task<TaskResult<CurrentUserDto>> GetAuthenticationStateAsync()
    {
        var result = await _client.CallApiAsync<CurrentUserDto>("api/user/current-user","GET");

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

        if (!result.Value.IsSuccessful(out var _)) return TaskResult.FromFailure(result.Value.Message);
        
        return TaskResult.FromSuccess(result.Value.Message);
    }


    public async Task Logout()
    {
        await _client.CallApiAsync<string>("api/user/logout", "POST");
    }

}

