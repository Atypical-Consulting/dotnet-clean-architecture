# Clean Architecture with .NET 10

[![Build](https://github.com/Atypical-Consulting/dotnet-clean-architecture/actions/workflows/dotnet-core.yml/badge.svg)](https://github.com/Atypical-Consulting/dotnet-clean-architecture/actions/workflows/dotnet-core.yml)
[![License](https://img.shields.io/github/license/Atypical-Consulting/dotnet-clean-architecture)](LICENSE)

A production-ready reference implementation of **Clean Architecture** on the .NET 10 stack. Use cases serve as the central organizing structure, fully decoupled from frameworks and infrastructure details. The solution is orchestrated with **.NET Aspire**, rendered with **Blazor Web App + Tailwind CSS**, backed by **PostgreSQL**, and deployable to **Kubernetes** via Helm.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Development Guide](#development-guide)
- [Testing](#testing)
- [Deployment](#deployment)
- [Architecture Decisions](#architecture-decisions)
- [Contributing](#contributing)
- [License](#license)

---

## Project Overview

This project demonstrates how to build a modular, testable, and maintainable application using Clean Architecture principles on modern .NET. The domain models a **Virtual Wallet** where customers can register accounts and manage their balance through Deposit, Withdraw, and Transfer operations.

Key goals:

- **Separation of concerns** -- Domain and Application layers have zero dependency on infrastructure or UI frameworks.
- **Testability** -- Every layer is independently testable with unit, integration, component, and end-to-end tests.
- **Cloud-native readiness** -- .NET Aspire orchestration, OpenTelemetry observability, container publishing, and Kubernetes deployment are built in from the start.

---

## Architecture

```
+--------------------------------------------------------------+
|                        Presentation                          |
|  +------------------------+  +----------------------------+  |
|  |   WalletApp (Blazor)   |  |   WebApi (REST + OpenAPI)  |  |
|  |   Tailwind CSS          |  |   Scalar API Docs          |  |
|  +----------+-------------+  +------------+---------------+  |
|             |                             |                  |
+--------------------------------------------------------------+
              |                             |
              v                             v
+--------------------------------------------------------------+
|                        Application                           |
|  Use Cases / Input & Output Ports / Presenters               |
+-------------------------------+------------------------------+
                                |
                                v
+--------------------------------------------------------------+
|                          Domain                              |
|  Entities / Value Objects / Aggregate Roots / Repository      |
|  Interfaces / Domain Events                                  |
+--------------------------------------------------------------+
                                ^
                                |
+--------------------------------------------------------------+
|                       Infrastructure                         |
|  EF Core (PostgreSQL) / Repository Implementations           |
+--------------------------------------------------------------+

+--------------------------------------------------------------+
|                       Cross-Cutting                          |
|  AppHost (.NET Aspire) / ServiceDefaults (OpenTelemetry,     |
|  Resilience, Service Discovery)                              |
+--------------------------------------------------------------+
```

**Dependency rule:** Dependencies always point inward. Domain has no external references. Application depends only on Domain. Infrastructure and Presentation depend on Application (and transitively on Domain), but never the other way around.

---

## Tech Stack

| Layer            | Technology                                         |
| ---------------- | -------------------------------------------------- |
| Runtime          | .NET 10 / C# (preview)                            |
| Frontend         | Blazor Web App + Tailwind CSS                      |
| API              | ASP.NET Core Web API + Scalar (OpenAPI docs)       |
| Auth             | ASP.NET Core Identity + OpenIddict                 |
| Database         | PostgreSQL (via Npgsql + EF Core 10)               |
| Orchestration    | .NET Aspire 13.1                                   |
| Observability    | OpenTelemetry (traces, metrics, logs)              |
| Logging          | Serilog (structured, OTLP sink)                    |
| Resilience       | Microsoft.Extensions.Http.Resilience               |
| Feature Flags    | Microsoft.FeatureManagement                        |
| API Versioning   | Asp.Versioning.Mvc                                 |
| Testing          | xUnit, Testcontainers, Coverlet                    |
| Containerization | .NET SDK container publishing (OCI)                |
| Deployment       | Kubernetes + Helm                                  |
| CI/CD            | GitHub Actions                                     |

---

## Project Structure

```
accounts-api/
  src/
    AppHost/            .NET Aspire orchestrator (entry point)
    ServiceDefaults/    Shared OpenTelemetry, resilience, service discovery
    Domain/             Entities, value objects, repository interfaces
    Application/        Use cases, input/output ports, presenters
    Infrastructure/     EF Core DbContext, repository implementations
    WebApi/             REST controllers, auth, OpenAPI, feature flags
    WalletApp/          Blazor Web App frontend (Tailwind CSS)
  test/
    UnitTests/          Domain and Application unit tests
    IntegrationTests/   Infrastructure tests (Testcontainers + PostgreSQL)
    ComponentTests/     HTTP-level component tests
    EndToEndTests/      Full-stack E2E tests
k8s/
  charts/
    clean-architecture/ Helm chart (staging + production values)
```

---

## Getting Started

### Prerequisites

| Requirement           | Minimum Version | Notes                                    |
| --------------------- | --------------- | ---------------------------------------- |
| .NET SDK              | 10.0            | [Download](https://dot.net/download)     |
| Docker                | 24+             | Required for PostgreSQL via Aspire        |
| Node.js (optional)    | 20+             | Only if modifying Tailwind CSS styles     |

### Run with .NET Aspire

```bash
# Clone the repository
git clone https://github.com/Atypical-Consulting/dotnet-clean-architecture.git
cd dotnet-clean-architecture

# Start the full stack (PostgreSQL, WebApi, WalletApp)
dotnet run --project accounts-api/src/AppHost
```

Aspire will automatically provision a PostgreSQL container, run database migrations, and start all services.

### Access URLs

Once running, open the **Aspire Dashboard** URL shown in the terminal (typically `https://localhost:17222`) to see all resources. From there you can navigate to:

| Service           | Description                              |
| ----------------- | ---------------------------------------- |
| Aspire Dashboard  | Orchestration dashboard with logs, traces, metrics |
| WalletApp         | Blazor Web App frontend                  |
| WebApi            | REST API (append `/scalar` for API docs) |
| PgAdmin           | PostgreSQL administration UI             |

Ports are dynamically assigned by Aspire. Check the dashboard for exact URLs.

---

## Development Guide

### How to Add a New Use Case

1. **Define the use case interface** in `Application/` (e.g., `ITransferUseCase`).
2. **Create input/output ports** -- an input DTO and an output port (presenter interface).
3. **Implement the use case** in `Application/UseCases/`, injecting repository interfaces from `Domain/`.
4. **Wire the presenter** in `WebApi/` -- create a controller action that instantiates the presenter and calls the use case.
5. **Register in DI** -- add the use case to the service collection.

### How to Add a New Entity

1. **Create the entity** in `Domain/` following the existing patterns (see `Account.cs`).
2. **Define the repository interface** in `Domain/` (e.g., `IAccountRepository`).
3. **Implement the repository** in `Infrastructure/` using EF Core.
4. **Add EF Core configuration** -- create an `EntityTypeConfiguration<T>` class.
5. **Create and apply a migration:**

   ```bash
   cd accounts-api/src/WebApi
   dotnet ef migrations add AddNewEntity --project ../Infrastructure
   ```

### Feature Flag Configuration

Feature flags are managed via `Microsoft.FeatureManagement`. Define flags in `appsettings.json`:

```json
{
  "FeatureManagement": {
    "Transfer": true,
    "ExcelExport": false
  }
}
```

Use `[FeatureGate("Transfer")]` on controllers or check programmatically with `IFeatureManager`.

---

## Testing

### Prerequisites

- **Docker** must be running (integration tests use Testcontainers to spin up PostgreSQL).

### Run All Tests

```bash
# From the repository root
dotnet test accounts-api/

# With coverage report
dotnet test accounts-api/ --collect:"XPlat Code Coverage"
```

### Test Types

| Type          | Project               | What It Tests                                  |
| ------------- | --------------------- | ---------------------------------------------- |
| Unit          | `test/UnitTests`      | Domain entities, value objects, use cases       |
| Integration   | `test/IntegrationTests` | EF Core repositories against real PostgreSQL  |
| Component     | `test/ComponentTests` | HTTP pipeline (controllers, middleware, DI)     |
| End-to-End    | `test/EndToEndTests`  | Full application stack                         |

Integration tests use **Testcontainers** to launch a real PostgreSQL instance in Docker, ensuring database behavior matches production.

---

## Deployment

### Kubernetes with Helm

The `k8s/charts/clean-architecture/` Helm chart deploys the full stack (WebApi, WalletApp, PostgreSQL) to any Kubernetes cluster.

```bash
# Build and publish container images
cd k8s
./build-images.sh

# Deploy to staging
helm upgrade --install clean-arch ./charts/clean-architecture \
  -f ./charts/clean-architecture/values.staging.yaml \
  -n clean-arch --create-namespace

# Deploy to production
helm upgrade --install clean-arch ./charts/clean-architecture \
  -f ./charts/clean-architecture/values.production.yaml \
  -n clean-arch --create-namespace
```

### Container Images

The solution uses .NET SDK container publishing. Each service declares its container registry and image name in its `.csproj`:

| Service    | Image                                          |
| ---------- | ---------------------------------------------- |
| WebApi     | `ghcr.io/atypical-consulting/accounts-api`     |
| WalletApp  | `ghcr.io/atypical-consulting/wallet-app`       |

### Environment Configuration

| Environment | Values File              | Key Differences                      |
| ----------- | ------------------------ | ------------------------------------ |
| Staging     | `values.staging.yaml`    | Reduced replicas, debug logging      |
| Production  | `values.production.yaml` | HA replicas, production secrets      |

---

## Architecture Decisions

| Decision                          | Rationale                                                                                          |
| --------------------------------- | -------------------------------------------------------------------------------------------------- |
| **.NET 10**                       | Latest LTS-track release with performance improvements, minimal API enhancements, and C# language features. |
| **Blazor Web App**                | Server and client rendering in a single project, eliminating the need for a separate SPA framework. |
| **Tailwind CSS**                  | Utility-first CSS integrated into the .NET build pipeline; no runtime CSS framework dependency.     |
| **ASP.NET Core Identity + OpenIddict** | Standards-based OAuth 2.0 / OpenID Connect without external identity provider dependency.      |
| **.NET Aspire**                   | Declarative orchestration of services and dependencies (PostgreSQL, PgAdmin) with built-in service discovery, health checks, and dashboard. |
| **PostgreSQL**                    | Open-source, battle-tested relational database with excellent .NET support via Npgsql.             |
| **OpenTelemetry**                 | Vendor-neutral observability (traces, metrics, logs) with OTLP export to any compatible backend.   |
| **Serilog**                       | Structured logging with rich sinks; OTLP sink feeds directly into the OpenTelemetry pipeline.      |
| **Testcontainers**                | Real database instances in tests; no in-memory fakes that mask production behavior.                |
| **Kubernetes + Helm**             | Production-grade container orchestration with environment-specific configuration via values files.  |
| **Clean Architecture**            | Framework-independent domain and application layers enable long-term maintainability and testability. |

---

## Contributing

Contributions are welcome. Please:

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/my-feature`).
3. Write tests for your changes.
4. Ensure all tests pass (`dotnet test accounts-api/`).
5. Submit a pull request.

---

## License

This project is licensed under the terms of the [Apache 2.0 License](LICENSE).
