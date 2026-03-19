namespace CleanArchitecture.WebApi.Modules.AppModules;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using CleanArchitecture.WebApi.Modules.Common;
using OpenTelemetry.Metrics;
using TheAppManager.Modules;

/// <summary>
///     Module for custom business metrics and OpenTelemetry meter registration.
/// </summary>
public sealed class ObservabilityModule : IAppModule
{
    /// <inheritdoc />
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<BusinessMetrics>();
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddMeter(BusinessMetrics.MeterName));
    }
}
