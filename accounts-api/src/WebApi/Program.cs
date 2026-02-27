namespace WebApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

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

        // Add Aspire service defaults (OpenTelemetry, health checks, service discovery, resilience)
        builder.AddServiceDefaults();

        // Configure all application services via the existing Startup class
        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();

        // Configure the HTTP request pipeline via the existing Startup class
        startup.Configure(app, app.Environment);

        // Map Aspire default health check endpoints
        app.MapDefaultEndpoints();

        app.Run();
    }
}
