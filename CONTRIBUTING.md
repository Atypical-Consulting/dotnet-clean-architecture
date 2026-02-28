# Contributing to dotnet-clean-architecture

Thank you for your interest in contributing to this project! This guide covers everything you need to get started.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (see `DOTNET_VERSION` in the CI workflow)
- [Docker](https://www.docker.com/) (required for integration and end-to-end tests via Testcontainers)
- A code editor with C# support (e.g., Visual Studio, Rider, or VS Code with the C# Dev Kit)

## Development Setup

1. **Fork and clone** the repository:

   ```bash
   git clone https://github.com/<your-username>/dotnet-clean-architecture.git
   cd dotnet-clean-architecture
   ```

2. **Restore dependencies**:

   ```bash
   dotnet restore
   ```

3. **Build the solution**:

   ```bash
   dotnet build
   ```

4. **Run the tests**:

   ```bash
   dotnet test
   ```

## Project Structure

The solution follows Clean Architecture with these layers:

| Project | Layer | Purpose |
|---------|-------|---------|
| `CleanArchitecture.Domain` | Domain | Entities, value objects, aggregate roots, repository interfaces |
| `CleanArchitecture.Application` | Application | Use cases, input/output ports, presenters |
| `CleanArchitecture.Infrastructure` | Infrastructure | EF Core, PostgreSQL, external services |
| `CleanArchitecture.WebApi` | Presentation | REST API with OpenAPI/Scalar docs |
| `CleanArchitecture.WalletApp` | Presentation | Blazor Web App with Tailwind CSS |
| `CleanArchitecture.AppHost` | Orchestration | .NET Aspire host |
| `CleanArchitecture.ServiceDefaults` | Orchestration | Shared service configuration |

Tests are organized by scope:

| Project | Scope |
|---------|-------|
| `CleanArchitecture.UnitTests` | Unit tests |
| `CleanArchitecture.IntegrationTests` | Integration tests |
| `CleanArchitecture.ComponentTests` | Component tests |
| `CleanArchitecture.EndToEndTests` | End-to-end tests |

## Code Style

- This project uses an `.editorconfig` file. Make sure your editor respects it.
- Run `dotnet format` before committing to ensure consistent formatting:

  ```bash
  dotnet format
  ```

- Use **file-scoped namespaces**.
- Enable **nullable reference types** (configured globally in `Directory.Build.props`).
- The project uses C# preview language features (`LangVersion` is set to `preview`).
- Package versions are managed centrally in `Directory.Packages.props`. When adding a new NuGet package, add the version entry there.

## Making Changes

1. **Create a branch** from `main`:

   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Follow the architecture**. Domain and Application layers must not depend on Infrastructure or Presentation layers. If you are unsure where a change belongs, refer to the [Architecture](#project-structure) section above.

3. **Write or update tests** as needed. All new logic should have corresponding unit tests at a minimum.

4. **Ensure all tests pass**:

   ```bash
   dotnet test --configuration Release
   ```

5. **Commit your changes** with a clear, descriptive message.

## Pull Request Process

1. Push your branch to your fork and open a pull request against the `main` branch.
2. Fill in a clear description of what your PR does and why.
3. Make sure CI passes (the pipeline runs `dotnet build` and `dotnet test` in Release configuration).
4. Address any review feedback from maintainers.
5. Once approved, a maintainer will merge your PR.

## Reporting Issues

- Use the [GitHub issue tracker](https://github.com/Atypical-Consulting/dotnet-clean-architecture/issues).
- Check existing issues before creating a new one.
- Include steps to reproduce, expected behavior, and actual behavior when reporting bugs.

## License

By contributing, you agree that your contributions will be licensed under the [Apache License 2.0](LICENSE).
