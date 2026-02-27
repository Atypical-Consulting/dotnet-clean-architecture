var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL server and database
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume("clean-arch-postgres-data");

var mangadb = postgres.AddDatabase("mangadb", databaseName: "Accounts");

// Add WebApi project with database reference
var webapi = builder.AddProject<Projects.WebApi>("webapi")
    .WithReference(mangadb)
    .WaitFor(mangadb)
    .WithExternalHttpEndpoints();

// Add WalletApp (Blazor) project with reference to WebApi
builder.AddProject<Projects.WalletApp>("walletapp")
    .WithReference(webapi)
    .WaitFor(webapi)
    .WithExternalHttpEndpoints();

builder.Build().Run();
