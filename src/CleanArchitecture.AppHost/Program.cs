var builder = DistributedApplication.CreateBuilder(args);

// Container registry configuration (overridable via environment or appsettings)
var containerRegistry = builder.Configuration["ContainerRegistry"] ?? "ghcr.io/atypical-consulting";

// Add PostgreSQL server and database
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume("clean-arch-postgres-data")
    .PublishAsConnectionString();

var mangadb = postgres.AddDatabase("mangadb", databaseName: "Accounts");

// Add WebApi project with database reference
var webapi = builder.AddProject<Projects.CleanArchitecture_WebApi>("webapi")
    .WithReference(mangadb)
    .WaitFor(mangadb)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

// Add WalletApp (Blazor) project with reference to WebApi
builder.AddProject<Projects.CleanArchitecture_WalletApp>("walletapp")
    .WithReference(webapi)
    .WaitFor(webapi)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();
