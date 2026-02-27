namespace WebApi.Modules.Common;

using Application.Services;
using FeatureFlags;
using Infrastructure.ExternalAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

/// <summary>
///     Authentication Extensions.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    ///     Add Authentication Extensions.
    /// </summary>
    public static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IFeatureManager featureManager = services
            .BuildServiceProvider()
            .GetRequiredService<IFeatureManager>();

        bool isEnabled = featureManager
            .IsEnabledAsync(nameof(CustomFeature.Authentication))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        if (isEnabled)
        {
            services.AddScoped<IUserService, ExternalUserService>();

            services
                .AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                        // set the Identity.API service as the authority on authentication/authorization
                        options.Authority = configuration["AuthenticationModule:AuthorityUrl"];

                    options.RequireHttpsMetadata = false;

                    options.Audience = "api1";
                });
        }
        else
        {
            services.AddScoped<IUserService, TestUserService>();

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = "Test";
                x.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                "Test", options => { });
        }

        return services;
    }
}
