namespace SysLog.Client.Models;

public class UserRegisterModel
{
    public UserRegisterModel(){}
    
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}