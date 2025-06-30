using Microsoft.AspNetCore.Components.Authorization;
using SysLog.Client.Client;
using SysLog.Shared;
using SysLog.Shared.ModelDto;

namespace SysLog.Client.Services;

public class AuthService
{
    private readonly AuthProvider _authProvider;   
    private readonly ClientSideApi _client;

    public AuthService(AuthProvider asp, ClientSideApi client)
    {
        _authProvider =asp;
        _client       = client;
    }
    
    public async Task<TaskResult> SignIn(string email, string password)
    {
       return await _authProvider.SignIn(email,password);    
    }

    public async Task Logout()
    {
        await _authProvider.Logout();
    }
    
    public async Task<TaskResult> SignUp(string username,string email, string password)
    {
      return await _authProvider.SignUp(username, email, password);
    }

    public async Task<TaskResult> ForgotPassword(string email, string password)
    {
        return await _authProvider.ChangePassword(email, password);
    }
}
