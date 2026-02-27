namespace WebApi.Modules;

using Application.Services;
using Common.FeatureFlags;
using Domain;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

/// <summary>
///     Persistence Extensions.
/// </summary>
public static class PostgreSqlExtensions
{
    /// <summary>
    ///     Add Persistence dependencies varying on configuration.
    /// </summary>
    public static IServiceCollection AddPostgreSql(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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
            services.AddDbContext<MangaContext>(
                options =>
                {
                    options.UseNpgsql(
                        configuration.GetValue<string>("PersistenceModule:DefaultConnection"));
                    options.ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
                });
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IAccountRepository, AccountRepository>();
        }
        else
        {
            services.AddSingleton<MangaContextFake, MangaContextFake>();
            services.AddScoped<IUnitOfWork, UnitOfWorkFake>();
            services.AddScoped<IAccountRepository, AccountRepositoryFake>();
        }

        services.AddScoped<IAccountFactory, EntityFactory>();

        return services;
    }
}
