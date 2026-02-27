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
            "Server=localhost;User Id=sa;Password=<YourStrong!Passw0rd>;Database=Accounts;";

        DbContextOptions<MangaContext> options = new DbContextOptionsBuilder<MangaContext>()
            .UseSqlServer(connectionString)
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
