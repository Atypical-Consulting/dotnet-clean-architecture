namespace CleanArchitecture.WebApi.Modules;

using CleanArchitecture.Application.Services;
using Common.FeatureFlags;
using CleanArchitecture.Infrastructure.CurrencyExchange;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

/// <summary>
///     Currency Exchange Extensions.
/// </summary>
public static class CurrencyExchangeExtensions
{
    /// <summary>
    ///     Add Currency Exchange dependencies varying on configuration.
    /// </summary>
    public static IServiceCollection AddCurrencyExchange(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        IFeatureManager featureManager = services
            .BuildServiceProvider()
            .GetRequiredService<IFeatureManager>();

        bool isEnabled = featureManager
            .IsEnabledAsync(nameof(CustomFeature.CurrencyExchange))
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        if (isEnabled)
        {
            services.Configure<CurrencyExchangeOptions>(
                configuration.GetSection(CurrencyExchangeOptions.SectionName));
            services.AddHttpClient(CurrencyExchangeService.HttpClientName);
            services.AddScoped<ICurrencyExchange, CurrencyExchangeService>();
        }
        else
        {
            services.AddScoped<ICurrencyExchange, CurrencyExchangeFake>();
        }

        return services;
    }
}
