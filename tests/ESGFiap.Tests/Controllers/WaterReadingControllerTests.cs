using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ESGFiap.Tests.Helpers;
using Xunit;

namespace ESGFiap.Tests.Controllers;

public class WaterReadingControllerTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    public async Task InitializeAsync() => await factory.SeedAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<(HttpClient client, int sensorId)> ClientWithSensor()
    {
        var client = factory.CreateClient();
        var token = await AuthHelper.GetTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.PostAsJsonAsync("/api/sensor", new
        {
            Name = "Sensor Leitura",
            Location = "Medidor principal",
            Type = 1,
            AlertThresholdLitersPerHour = 100.0
        });

        var body = await res.Content.ReadAsStringAsync();
        var sensor = System.Text.Json.JsonSerializer.Deserialize<SensorId>(body,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return (client, sensor!.Id);
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        var (client, sensorId) = await ClientWithSensor();
        var request = $"/api/sensor/{sensorId}/readings";

        var response = await client.GetAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreateReading_ReturnsHttpStatusCode201()
    {
        var (client, sensorId) = await ClientWithSensor();

        var response = await client.PostAsJsonAsync($"/api/sensor/{sensorId}/readings", new
        {
            LitersPerHour = 45.5,
            PressureBar = 3.2
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_ReadingAboveThreshold_GeneratesAlert()
    {
        var (client, sensorId) = await ClientWithSensor();

        var response = await client.PostAsJsonAsync($"/api/sensor/{sensorId}/readings", new
        {
            LitersPerHour = 250.0,
            PressureBar = 3.2
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var result = System.Text.Json.JsonSerializer.Deserialize<ReadingResult>(body,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.True(result!.AlertGenerated);
    }

    [Fact]
    public async Task Get_Summary_ReturnsHttpStatusCode200()
    {
        var (client, sensorId) = await ClientWithSensor();

        var response = await client.GetAsync($"/api/sensor/{sensorId}/readings/summary");

        response.EnsureSuccessStatusCode();
    }

    private record SensorId(int Id);
    private record ReadingResult(bool AlertGenerated);
}
