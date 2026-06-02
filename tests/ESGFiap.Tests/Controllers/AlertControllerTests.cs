using System.Net;
using System.Net.Http.Headers;
using ESGFiap.Tests.Helpers;

namespace ESGFiap.Tests.Controllers;

public class AlertControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public async Task InitializeAsync() => await factory.SeedAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<HttpClient> AuthenticatedClient()
    {
        var client = factory.CreateClient();
        var token = await AuthHelper.GetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        var client = await AuthenticatedClient();
        var request = "/api/alert";

        var response = await client.GetAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithStatusFilter_ReturnsHttpStatusCode200()
    {
        var client = await AuthenticatedClient();

        var response = await client.GetAsync("/api/alert?status=1");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_WithSeverityFilter_ReturnsHttpStatusCode200()
    {
        var client = await AuthenticatedClient();

        var response = await client.GetAsync("/api/alert?severity=3");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_NotFound_Returns404()
    {
        var client = await AuthenticatedClient();

        var response = await client.GetAsync("/api/alert/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
