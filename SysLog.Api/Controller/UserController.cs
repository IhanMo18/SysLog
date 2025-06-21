using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IResult> Authenticate([FromBody] UserLoginDto userLoginDto)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.Email == userLoginDto.Email);
        if (user is not null)
        {
            var result = await _signInManager.PasswordSignInAsync(user, userLoginDto.Password, true, false);
            if (result.Succeeded)
                return Results.Ok("Signed In");
        }

        return Results.BadRequest("Invalid credentials");
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] UserDto userDto)
    {
        var user = new AppUser(userDto.Username,userDto.Email);
        var result =await _userManager.CreateAsync(user, userDto.Password);
        if (result.Succeeded)
        {
            await _signInManager.PasswordSignInAsync(user, userDto.Password, true, false);
            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("User"));
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            return Results.Ok(new{user.Id, user.UserName, user.Email});
        }
        return Results.BadRequest(result.Errors);
    }
    
    [Authorize]
    [HttpPost("logout")]
    public async Task<IResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Results.Ok("Logout successfully");
    }
    
    [Authorize]
    [HttpGet("me")]
    public IResult GetCurrentUser()
    {

        return Results.Ok(new
        {
            userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            username = User.Identity?.Name,
            email = User.FindFirstValue(ClaimTypes.Email),
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });

    }
}