namespace EndToEndTests;

using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using WebApi;

/// <summary>
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.ConfigureAppConfiguration(
        (context, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    ["FeatureManagement:PostgreSql"] = "true",
                    ["PersistenceModule:DefaultConnection"] =
                        "Host=localhost;Port=5432;Database=Accounts;Username=postgres;Password=YourStrong!Passw0rd",
                    ["FeatureManagement:CurrencyExchangeModule"] = "true"
                });
        });
}
