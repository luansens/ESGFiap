using ESGFiap.Data;
using ESGFiap.Models;
using ESGFiap.Services;
using ESGFiap.ViewModels.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace ESGFiap.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(MongoContext mongo, TokenService tokenService) : ControllerBase
{
    private static readonly PasswordHasher<UserDocument> Hasher = new();

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        var normalizedEmail = model.Email.Trim().ToUpperInvariant();
        var alreadyExists = await mongo.Users.Find(u => u.NormalizedEmail == normalizedEmail).AnyAsync();
        if (alreadyExists)
            return BadRequest(new[] { "Já existe um usuário cadastrado com este e-mail." });

        var user = new UserDocument
        {
            Email = model.Email,
            NormalizedEmail = normalizedEmail,
            FullName = model.FullName,
            CompanyName = model.CompanyName,
            UserType = model.UserType,
            Roles = ["User"],
            SustainabilityProfile = model.SustainabilityProfile.ToBsonDocument()
        };
        user.PasswordHash = Hasher.HashPassword(user, model.Password);

        await mongo.Users.InsertOneAsync(user);

        var (token, expiresAt) = tokenService.GenerateToken(user);
        return Ok(new AuthResponseViewModel
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var normalizedEmail = model.Email.Trim().ToUpperInvariant();
        var user = await mongo.Users.Find(u => u.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync();
        if (user is null ||
            Hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
            return Unauthorized("Credenciais inválidas.");

        var (token, expiresAt) = tokenService.GenerateToken(user);
        return Ok(new AuthResponseViewModel
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName,
            ExpiresAt = expiresAt
        });
    }
}
