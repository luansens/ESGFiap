using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ESGFiap.Tests.Helpers;
using Xunit;

namespace ESGFiap.Tests.Controllers;

public class WaterReportControllerTests(CustomWebApplicationFactory factory)
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
        var request = "/api/waterreport";

        var response = await client.GetAsync(request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_CreateReport_ReturnsHttpStatusCode201()
    {
        var client = await AuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/waterreport", new
        {
            Title = "Relatório Q1 2024",
            Region = "São Paulo - SP",
            PeriodStart = new DateTime(2024, 1, 1),
            PeriodEnd = new DateTime(2024, 3, 31),
            RegionalAverageLiters = 50000.0,
            ImpactAnalysis = "A empresa consumiu abaixo da média regional.",
            MitigationPlan = "Manter práticas de reuso de água e instalar mais sensores."
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Post_InvalidPeriod_ReturnsBadRequest()
    {
        var client = await AuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/waterreport", new
        {
            Title = "Relatório Inválido",
            Region = "SP",
            PeriodStart = new DateTime(2024, 3, 31),
            PeriodEnd = new DateTime(2024, 1, 1),
            RegionalAverageLiters = 50000.0,
            ImpactAnalysis = "Teste.",
            MitigationPlan = "Teste."
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_NotFound_Returns404()
    {
        var client = await AuthenticatedClient();

        var response = await client.GetAsync("/api/waterreport/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
