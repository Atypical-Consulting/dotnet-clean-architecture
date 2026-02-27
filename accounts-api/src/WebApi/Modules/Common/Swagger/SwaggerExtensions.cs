namespace WebApi.Modules.Common.Swagger;

using System;
using System.Threading.Tasks;
using FeatureFlags;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

/// <summary>
///     OpenAPI Extensions.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    ///     Add OpenAPI Configuration dependencies.
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
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "Clean Architecture Manga API";
                    document.Info.Description = "Clean Architecture, DDD and TDD implementation.";
                    document.Info.Contact = new OpenApiContact
                    {
                        Name = "Ivan Paulovich",
                        Email = "ivan@paulovich.net"
                    };
                    document.Info.License = new OpenApiLicense
                    {
                        Name = "Apache License",
                        Url = new Uri(
                            "https://raw.githubusercontent.com/ivanpaulovich/clean-architecture-manga/master/README.md")
                    };
                    document.Info.TermsOfService = new Uri("http://paulovich.net");
                    return Task.CompletedTask;
                });
            });
        }

        return services;
    }

    /// <summary>
    ///     Map OpenAPI and Scalar API reference endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapOpenApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOpenApi();
        endpoints.MapScalarApiReference(options =>
        {
            options.Title = "Clean Architecture Manga API";
        });
        return endpoints;
    }
}
