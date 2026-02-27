namespace CleanArchitecture.WebApi.Modules.Common;

using CleanArchitecture.Application.Services;
using FeatureFlags;
using CleanArchitecture.Infrastructure.ExternalAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using OpenIddict.Validation.AspNetCore;

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

            services.AddOpenIddict()
                .AddValidation(options =>
                {
                    options.SetIssuer(
                        configuration["AuthenticationModule:AuthorityUrl"]!);

                    options.UseSystemNetHttp();
                    options.UseAspNetCore();
                });

            services
                .AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
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
