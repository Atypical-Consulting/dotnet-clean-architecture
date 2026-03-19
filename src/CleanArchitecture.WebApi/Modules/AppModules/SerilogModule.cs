namespace CleanArchitecture.WebApi.Modules.AppModules;

using Microsoft.AspNetCore.Builder;
using CleanArchitecture.WebApi.Modules.Common;
using Serilog;
using TheAppManager.Modules;

/// <summary>
///     Module for Serilog structured logging configuration.
/// </summary>
public sealed class SerilogModule : IAppModule
{
    /// <inheritdoc />
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.With<ActivityEnricher>());
    }

    /// <inheritdoc />
    public void ConfigureMiddleware(WebApplication app)
    {
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
    }
}
