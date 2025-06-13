using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SysLog.Domain.Model;
using SysLog.Shared.ModelDto;

namespace SysLog.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public UserController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<IResult> Authenticate([FromBody] UserLoginDto userLoginDto)
    {
        var user = _userManager.Users.FirstOrDefault(u=>u.Email == userLoginDto.Email);
        if (user is not null)
        {
            var result = await _signInManager.PasswordSignInAsync(user, userLoginDto.Password, true, false);
            if(result.Succeeded)
                return Results.Ok("Signed In");
        }
        return Results.BadRequest(400);
    }

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] UserDto userDto)
    {
        var user = new AppUser(userDto.Username,userDto.Email);
        var result =await _userManager.CreateAsync(user, userDto.Password);
        if (result.Succeeded)
        {
            await _signInManager.PasswordSignInAsync(user, userDto.Password, true, false);
            return Results.Ok(new{user.Id, user.UserName, user.Email});
        }
        return Results.BadRequest(result.Errors);
    }
    
    [HttpPost("logout")]
    public async Task<IResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Results.Ok();
    }
    
    [HttpGet("current-user")]
    public IResult GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return Results.Ok(new 
            {
                name = User.Identity.Name,
                authenticated=User.Identity.IsAuthenticated,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
        return Results.Unauthorized();
    }
}