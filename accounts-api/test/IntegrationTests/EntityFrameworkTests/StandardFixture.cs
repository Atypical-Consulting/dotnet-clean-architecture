namespace IntegrationTests.EntityFrameworkTests;

using System;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public sealed class StandardFixture : IDisposable
{
    public StandardFixture()
    {
        const string connectionString =
            "Host=localhost;Port=5432;Database=Accounts;Username=postgres;Password=YourStrong!Passw0rd";

        DbContextOptions<MangaContext> options = new DbContextOptionsBuilder<MangaContext>()
            .UseNpgsql(connectionString)
            .ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        this.Context = new MangaContext(options);
        this.Context
            .Database
            .EnsureCreated();
    }

    public MangaContext Context { get; }

    public void Dispose() => this.Context.Dispose();
}
