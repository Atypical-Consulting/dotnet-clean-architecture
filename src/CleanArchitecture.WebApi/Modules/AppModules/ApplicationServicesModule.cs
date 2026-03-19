namespace CleanArchitecture.WebApi.Modules.AppModules;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TheAppManager.Modules;

/// <summary>
///     Module that delegates to the existing Startup class for application service
///     registration and HTTP pipeline configuration.
/// </summary>
public sealed class ApplicationServicesModule : IAppModule
{
    /// <inheritdoc />
    public void ConfigureServices(WebApplicationBuilder builder)
    {
        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);

        // Store the Startup instance so ConfigureMiddleware can use it
        builder.Services.AddSingleton(startup);
    }

    /// <inheritdoc />
    public void ConfigureMiddleware(WebApplication app)
    {
        var startup = app.Services.GetRequiredService<Startup>();
        startup.Configure(app, app.Environment);
    }
}
