namespace ESGFiap.ViewModels.Auth;

public class AuthResponseViewModel
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
