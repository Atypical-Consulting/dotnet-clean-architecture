namespace CleanArchitecture.WebApi.Modules;

using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using Common.FeatureFlags;
using CleanArchitecture.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;

/// <summary>
///     HealthChecks Extensions.
/// </summary>
public static class HealthChecksExtensions
{
    /// <summary>
    ///     Add Health Checks dependencies varying on configuration.
    /// </summary>
    public static IServiceCollection AddHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IHealthChecksBuilder healthChecks = services.AddHealthChecks();

        IFeatureManager featureManager = services
            .BuildServiceProvider()
            .GetRequiredService<IFeatureManager>();

        bool isEnabled = featureManager
            .IsEnabledAsync(nameof(CustomFeature.PostgreSql))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        if (isEnabled)
        {
            healthChecks.AddDbContextCheck<MangaContext>("MangaDbContext");
        }

        return services;
    }

    /// <summary>
    ///     Use Health Checks dependencies.
    /// </summary>
    public static IApplicationBuilder UseHealthChecks(
        this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health",
            new HealthCheckOptions { ResponseWriter = WriteResponse });

        return app;
    }

    private static Task WriteResponse(HttpContext context, HealthReport result)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var response = new Dictionary<string, object>
        {
            ["status"] = result.Status.ToString(),
            ["results"] = result.Entries.ToDictionary(
                pair => pair.Key,
                pair => (object)new Dictionary<string, object>
                {
                    ["status"] = pair.Value.Status.ToString(),
                    ["description"] = pair.Value.Description ?? string.Empty,
                    ["data"] = pair.Value.Data.ToDictionary(p => p.Key, p => p.Value)
                })
        };

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
    }
}
