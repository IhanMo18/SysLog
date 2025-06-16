using Microsoft.AspNetCore.Identity;

namespace SysLog.Domain.Model;

public class AppUser: IdentityUser
{
    public AppUser(){}
    public AppUser(string username,string email)
    {
        UserName = username;
        Email = email;
    }
}