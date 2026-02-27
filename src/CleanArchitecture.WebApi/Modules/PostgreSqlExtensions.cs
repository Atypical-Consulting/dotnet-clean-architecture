namespace CleanArchitecture.WebApi.Modules;

using CleanArchitecture.Application.Services;
using Common.FeatureFlags;
using CleanArchitecture.Domain;
using CleanArchitecture.Infrastructure.DataAccess;
using CleanArchitecture.Infrastructure.DataAccess.Repositories;
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
            // Check if running under Aspire (connection string provided via Aspire service discovery)
            var aspireConnectionString = configuration.GetConnectionString("mangadb");
            if (!string.IsNullOrEmpty(aspireConnectionString))
            {
                services.AddDbContext<MangaContext>(
                    options =>
                    {
                        options.UseNpgsql(aspireConnectionString);
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
                    });
            }
            else
            {
                services.AddDbContext<MangaContext>(
                    options =>
                    {
                        options.UseNpgsql(
                            configuration.GetValue<string>("PersistenceModule:DefaultConnection"));
                        options.ConfigureWarnings(warnings =>
                            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
                    });
            }

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
