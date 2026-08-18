using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ESGFiap.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly string DbName = "testdb_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Mongo:ConnectionString"] =
                    Environment.GetEnvironmentVariable("MONGO_TEST_CONNECTION")
                    ?? "mongodb://esgfiap:ESGFiap%402024!@localhost:27017",
                ["Mongo:DatabaseName"] = DbName
            });
        });

        builder.UseEnvironment("Testing");
    }

    public Task SeedAsync() => Task.CompletedTask;
}
