namespace CleanArchitecture.WebApi.Modules.AppModules;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using TheAppManager.Modules;

/// <summary>
///     Module for Aspire service defaults (OpenTelemetry, health checks, service discovery, resilience).
/// </summary>
public sealed class AspireDefaultsModule : IAppModule
{
    /// <inheritdoc />
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
    }

    /// <inheritdoc />
    public void ConfigureMiddleware(WebApplication app)
    {
        app.MapDefaultEndpoints();
    }
}
