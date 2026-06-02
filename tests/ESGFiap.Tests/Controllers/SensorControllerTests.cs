using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ESGFiap.Tests.Helpers;
using Xunit;

namespace ESGFiap.Tests.Controllers;

public class SensorControllerTests(CustomWebApplicationFactory factory)
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
        var request = "/api/sensor";

        var response = await client.GetAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreateSensor_ReturnsHttpStatusCode201()
    {
        var client = await AuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/sensor", new
        {
            Name = "Sensor Entrada",
            Location = "Portaria principal",
            Type = 1,
            AlertThresholdLitersPerHour = 50.0
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreateSensor_InvalidData_ReturnsBadRequest()
    {
        var client = await AuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/sensor", new
        {
            Name = "",
            Location = "",
            Type = 1,
            AlertThresholdLitersPerHour = 0
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Unauthenticated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/sensor");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Paginated_ReturnsPagedResult()
    {
        var client = await AuthenticatedClient();

        var response = await client.GetAsync("/api/sensor?page=1&pageSize=5");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedResultShape>();
        Assert.NotNull(body);
        Assert.Equal(1, body.Page);
        Assert.Equal(5, body.PageSize);
    }

    private record PagedResultShape(int Page, int PageSize, int TotalCount, int TotalPages);
}
