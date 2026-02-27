namespace IntegrationTests.EntityFrameworkTests;

using System;
using System.Threading.Tasks;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;
using Xunit;

public sealed class StandardFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder("postgres:17")
        .Build();

    public MangaContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await this._postgresContainer.StartAsync();

        DbContextOptions<MangaContext> options = new DbContextOptionsBuilder<MangaContext>()
            .UseNpgsql(this._postgresContainer.GetConnectionString())
            .ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        this.Context = new MangaContext(options);
        await this.Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        this.Context?.Dispose();
        await this._postgresContainer.DisposeAsync();
    }
}
