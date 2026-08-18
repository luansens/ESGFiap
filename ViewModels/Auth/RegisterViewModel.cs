using System.Text.Json;
using ESGFiap.Models;

namespace ESGFiap.ViewModels.Auth;

public class RegisterViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public UserType UserType { get; set; } = UserType.Residential;
    public JsonElement? SustainabilityProfile { get; set; }
}
