namespace EndToEndTests;

using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebApi;
using Xunit;

/// <summary>
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Startup>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:17")
        .Build();

    public async Task InitializeAsync()
    {
        await this._postgresContainer.StartAsync();

        // Trigger host creation so ConfigureWebHost runs with the container ready
        using var scope = this.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MangaContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await this._postgresContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["FeatureManagement:PostgreSql"] = "true",
                        ["PersistenceModule:DefaultConnection"] = this._postgresContainer.GetConnectionString(),
                        ["FeatureManagement:CurrencyExchangeModule"] = "true"
                    });
            });
    }
}
