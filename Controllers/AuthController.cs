using ESGFiap.Models;
using ESGFiap.Services;
using ESGFiap.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            CompanyName = model.CompanyName,
            UserType = model.UserType
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await userManager.AddToRoleAsync(user, "User");

        var (token, expiresAt) = tokenService.GenerateToken(user, ["User"]);
        return Ok(new AuthResponseViewModel
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized("Credenciais inválidas.");

        var roles = await userManager.GetRolesAsync(user);
        var (token, expiresAt) = tokenService.GenerateToken(user, roles);

        return Ok(new AuthResponseViewModel
        {
            Token = token,
            Email = user.Email!,
            FullName = user.FullName,
            ExpiresAt = expiresAt
        });
    }
}
