namespace WebApi;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules;
using Modules.Common;
using Modules.Common.FeatureFlags;
using Modules.Common.Swagger;

/// <summary>
///     Startup.
/// </summary>
public sealed class Startup
{
    /// <summary>
    ///     Startup constructor.
    /// </summary>
    public Startup(IConfiguration configuration) => this.Configuration = configuration;

    private IConfiguration Configuration { get; }

    /// <summary>
    ///     Configure dependencies from application.
    /// </summary>
    public void ConfigureServices(IServiceCollection services) =>
        services
            .AddFeatureFlags(this.Configuration) // should be the first one.
            .AddInvalidRequestLogging()
            .AddCurrencyExchange(this.Configuration)
            .AddPostgreSql(this.Configuration)
            .AddHealthChecks(this.Configuration)
            .AddAuthentication(this.Configuration)
            .AddVersioning()
            .AddSwagger()
            .AddUseCases()
            .AddCustomControllers()
            .AddCustomCors()
            .AddProxy()
            .AddCustomDataProtection();

    /// <summary>
    ///     Configure http request pipeline.
    /// </summary>
    public void Configure(
        IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/api/V1/CustomError")
                .UseHsts();
        }

        app
            .UseProxy(this.Configuration)
            .UseHealthChecks()
            .UseCustomCors()
            .UseRouting()
            .UseAuthentication()
            .UseAuthorization()
            .UseEndpoints(endpoints =>
            {
                endpoints.MapOpenApiEndpoints();
                endpoints.MapControllers();
            });
    }
}
