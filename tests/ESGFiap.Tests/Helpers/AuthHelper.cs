using System.Net.Http.Json;

namespace ESGFiap.Tests.Helpers;

public static class AuthHelper
{
    public static async Task<string> GetTokenAsync(HttpClient client)
    {
        var email = $"test_{Guid.NewGuid():N}@test.com";

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            FullName = "Test User",
            Email = email,
            Password = "Test123!",
            CompanyName = "Test Co",
            UserType = 2
        });

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = "Test123!"
        });

        var body = await login.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(body,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return result!.Token;
    }

    private record TokenResponse(string Token);
}
