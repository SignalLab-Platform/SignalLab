# SignalLab API

The SignalLab Backend is an ASP.NET Core application targeting .NET 10.

It currently provides the technical foundation of the API without implementing any SignalLab business domain.

## Architecture

The Backend follows Clean Architecture principles and is divided into four responsibilities:

```text
SignalLab.Api
├── SignalLab.Application
└── SignalLab.Infrastructure
    ├── SignalLab.Application
    └── SignalLab.Domain

SignalLab.Application
└── SignalLab.Domain

SignalLab.Domain
└── no project or technology dependency
```

### SignalLab.Api

ASP.NET Core executable and Presentation layer.

It is responsible for:

* hosting the HTTP application;
* exposing technical and future business endpoints;
* configuring the application dependency graph;
* exposing OpenAPI in Development.

### SignalLab.Application

Application orchestration layer.

It will contain future use cases, commands, queries, DTOs, and interfaces required by the application.

### SignalLab.Domain

Technology-independent business layer.

It must not depend on ASP.NET Core, Entity Framework Core, PostgreSQL, Clerk, or any other infrastructure technology.

### SignalLab.Infrastructure

Technical implementation layer.

It will later contain persistence and external-service implementations required by the internal layers.

## Architecture conventions

The Backend is a Modular Monolith combining Clean Architecture and Vertical Slices.

Clean Architecture defines the dependency direction between the Api, Application, Domain, and Infrastructure projects.

Vertical Slices organize individual use cases inside their owning business module.

The operational rules are documented in:

- [Solution Architecture](../../docs/architecture/000%20-%20SolutionArchitecture.md)
- [Backend Conventions](../../docs/architecture/001%20-%20BackendConventions.md)
- [API Conventions](../../docs/architecture/003%20-%20APIConventions.md)
- [Persistence Conventions](../../docs/architecture/004%20-%20PersistenceConventions.md)
- [Testing Strategy](../../docs/architecture/005%20-%20TestingStrategy.md)
- [Architecture Decision Records](../../docs/architecture/adr/000%20-%20ADRIndex.md)

The main enforced rules include:

- Domain has no dependency on another SignalLab project or infrastructure technology;
- Application only references Domain;
- Infrastructure references Application and Domain;
- Api references Application and Infrastructure;
- HTTP contracts remain separate from Application contracts and Domain entities;
- Minimal APIs are organized by operation;
- use cases are implemented as explicit Commands, Queries, and Handlers;
- MediatR, AutoMapper, and Generic Repository are not used by default.

## Test projects

### SignalLab.Api.IntegrationTests

Validates the real ASP.NET Core HTTP pipeline, including the health and OpenAPI endpoints.

### SignalLab.ArchitectureTests

Validates durable repository rules such as:

- allowed project references;
- absence of technology dependencies in Domain;
- absence of Presentation and Persistence packages in Application;
- presence of the canonical Frontend packages;
- absence of Vite, TanStack Router, and React Router.

The architecture test project intentionally has no `ProjectReference` to the production projects. It inspects the repository configuration as an external observer.

Run every Backend test suite:

```powershell
dotnet test SignalLab.Api.sln --configuration Release
```

Run only the architecture tests:

```powershell
dotnet test tests/SignalLab.ArchitectureTests/SignalLab.ArchitectureTests.csproj --configuration Release
```

## Requirements

* .NET SDK 10.0.302 or a compatible .NET 10 SDK allowed by the repository `global.json`.

Verify the active SDK from the repository root:

```powershell
dotnet --version
```

## Restore

From `apps/api/`:

```powershell
dotnet restore SignalLab.Api.sln
```

## Build

```powershell
dotnet build SignalLab.Api.sln --configuration Release
```

## Test

```powershell
dotnet test SignalLab.Api.sln --configuration Release
```

## Run

Start the API with the local HTTPS launch profile:

```powershell
dotnet run --launch-profile https --project src/SignalLab.Api/SignalLab.Api.csproj
```

The default development addresses are:

```text
https://localhost:7281
http://localhost:5260
```

## Platform endpoints

### Health

```http
GET /health
```

Returns:

* `200 OK` when the registered health checks are healthy;
* `503 Service Unavailable` when at least one future health check is unhealthy.

The current health check only validates that the API process can respond.

### OpenAPI

Available in the Development environment:

```http
GET /openapi/v1.json
```

The document is generated dynamically at runtime and is not stored as a repository file.

## Dependency security

List known vulnerable direct and transitive packages:

```powershell
dotnet list SignalLab.Api.sln package --vulnerable --include-transitive
```

Warnings are treated as errors across the Backend projects.

## Current scope

The Backend currently contains no:

* business domain;
* database configuration;
* Entity Framework Core integration;
* authentication or authorization;
* SignalR integration;
* business endpoint.

These capabilities are introduced only by their dedicated specifications.
