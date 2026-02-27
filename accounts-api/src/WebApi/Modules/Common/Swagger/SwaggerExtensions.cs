namespace WebApi.Modules.Common.Swagger;

using System;
using System.IO;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using FeatureFlags;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
///     Swagger Extensions.
/// </summary>
public static class SwaggerExtensions
{
    private static string XmlCommentsFilePath
    {
        get
        {
            string basePath = AppContext.BaseDirectory;
            string fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
            return Path.Combine(basePath, fileName);
        }
    }


    /// <summary>
    ///     Add Swagger Configuration dependencies.
    /// </summary>
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        IFeatureManager featureManager = services
            .BuildServiceProvider()
            .GetRequiredService<IFeatureManager>();

        bool isEnabled = featureManager
            .IsEnabledAsync(nameof(CustomFeature.Swagger))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        if (isEnabled)
        {
            services
                .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
                .AddSwaggerGen(
                    c =>
                    {
                        c.IncludeXmlComments(XmlCommentsFilePath);
                        c.AddSecurityDefinition("Bearer",
                            new OpenApiSecurityScheme
                            {
                                In = ParameterLocation.Header,
                                Description = "Please insert JWT with Bearer into field",
                                Name = "Authorization",
                                Type = SecuritySchemeType.ApiKey
                            });
                        c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
                        {
                                {
                                    new OpenApiSecuritySchemeReference("Bearer"),
                                    new System.Collections.Generic.List<string>()
                                }
                        });
                    });
        }

        return services;
    }

    /// <summary>
    ///     Add Swagger dependencies.
    /// </summary>
    public static IApplicationBuilder UseVersionedSwagger(
        this IApplicationBuilder app,
        IApiVersionDescriptionProvider provider,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI(
            options =>
            {
                foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                {
                    string swaggerEndpoint;

                    string basePath = configuration["ASPNETCORE_BASEPATH"];

                    if (!string.IsNullOrEmpty(basePath))
                    {
                        swaggerEndpoint = $"{basePath}/swagger/{description.GroupName}/swagger.json";
                    }
                    else
                    {
                        swaggerEndpoint = $"/swagger/{description.GroupName}/swagger.json";
                    }

                    options.SwaggerEndpoint(swaggerEndpoint, description.GroupName.ToUpperInvariant());
                }
            });

        return app;
    }
}
