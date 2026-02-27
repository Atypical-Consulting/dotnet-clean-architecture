namespace WebApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Common;
using OpenTelemetry.Metrics;
using Serilog;

/// <summary>
///     Program entry point.
/// </summary>
public static class Program
{
    /// <summary>
    ///     Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog from appsettings and enrich with trace context
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.With<ActivityEnricher>());

        // Add Aspire service defaults (OpenTelemetry, health checks, service discovery, resilience)
        builder.AddServiceDefaults();

        // Register custom business metrics and add the meter to OpenTelemetry
        builder.Services.AddSingleton<BusinessMetrics>();
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddMeter(BusinessMetrics.MeterName));

        // Configure all application services via the existing Startup class
        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();

        // Add Serilog request logging middleware for structured HTTP request logs
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.Identity.Name ?? string.Empty);
                }
            };
        });

        // Configure the HTTP request pipeline via the existing Startup class
        startup.Configure(app, app.Environment);

        // Map Aspire default health check endpoints
        app.MapDefaultEndpoints();

        app.Run();
    }
}
