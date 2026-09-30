# SignalLab

SignalLab is a Game User Research platform designed to transform playtest feedback into structured, comparable, and durable knowledge.

This repository contains the SignalLab MVP source code, product specifications, technical architecture, and implementation roadmap.

## Repository status

The Platform Foundation is implemented.

SignalLab currently provides:

- an ASP.NET Core Backend following Clean Architecture;
- a Next.js Frontend using React and TypeScript;
- the permanent Application Shell;
- the navigation architecture;
- PostgreSQL persistence;
- Entity Framework Core and migrations;
- Development, Staging, and Production environment conventions;
- Docker images and a reproducible local Docker Compose stack;
- GitHub Actions validation for Backend and Frontend;
- a public Staging deployment.

No business domain is implemented yet.

Authentication, Organizations, Projects, Campaigns, Builds, Forms, Participation, Results, Analytics, and MAP are introduced by later Milestones.

SignalLab is designed as a Modular Monolith. The Platform Foundation is intentionally stable before business capabilities are added.

---

## Architecture

The main runtime architecture is:

```text
Browser
    ↓
Next.js
    ↓
ASP.NET Core API
    ↓
Application
    ↓
Domain
    ↑
Infrastructure
    ↓
Entity Framework Core
    ↓
PostgreSQL
```

The Backend is the authority for business data and rules.

The Frontend represents application state and user intentions.

The Domain remains independent from presentation, persistence, and infrastructure technologies.

Clean Architecture controls dependency direction, while Vertical Slices organize business use cases inside their owning modules.

---

## Repository structure

```text
SignalLab/
├── apps/
│   ├── api/                    # ASP.NET Core Backend
│   └── web/                    # Next.js Frontend
├── docs/
│   ├── architecture/           # Operational architecture and conventions
│   ├── milestones/             # MVP roadmap and implementation specifications
│   └── product/                # Product and conceptual source documents
├── scripts/                    # Repository validation and automation scripts
├── compose.yaml                # Local Docker Compose stack
├── global.json                 # .NET SDK policy
├── .env.example                # Local Docker configuration template
└── README.md
```

---

## Requirements

For local development:

- Git;
- .NET SDK 10 compatible with `global.json`;
- Node.js 24;
- npm;
- Docker Desktop.

The current reference versions include:

```text
.NET SDK baseline: 10.0.302
Node.js:           24.18.0
Next.js:           16.3.7
React:             19.2.4
PostgreSQL:        18.6
```

Bash is only required for repository scripts written in shell.

---

## Getting started

The recommended way to start the complete Platform Foundation is Docker Compose.

Create the local environment file:

```powershell
Copy-Item .env.example .env
```

Set a local PostgreSQL password in `.env`, then run:

```powershell
docker compose up --build -d
```

Check the services:

```powershell
docker compose ps
```

Expected local endpoints:

```text
Frontend
http://localhost:3000/home

API health
http://localhost:8080/health
```

Verify the API:

```powershell
Invoke-WebRequest `
  http://localhost:8080/health `
  -UseBasicParsing
```

Expected result:

```text
StatusCode : 200
Content    : Healthy
```

For the complete setup procedure, native development workflow, secrets, validation commands, and Git workflow, see:

[Developer Guide](docs/architecture/014%20-%20DeveloperGuide.md)

---

## Development validation

### Backend

```powershell
dotnet restore apps/api/SignalLab.Api.sln

dotnet build `
  apps/api/SignalLab.Api.sln `
  --configuration Release `
  --no-restore

dotnet test `
  apps/api/SignalLab.Api.sln `
  --configuration Release `
  --no-build `
  --no-restore
```

### Frontend

```powershell
npm --prefix apps/web ci
npm --prefix apps/web run lint
npm --prefix apps/web run typecheck
npm --prefix apps/web test
npm --prefix apps/web run build
```

### Docker

```powershell
docker compose config --quiet
docker compose up --build -d
docker compose ps
docker compose down
```

Pull Requests targeting `main` must pass the required GitHub Actions checks:

```text
Backend
Frontend
```

---

## Documentation

SignalLab separates product intent, implementation scope, operational architecture, and architectural decisions.

### Product documentation

Located in:

```text
docs/product/
```

Main references include:

- Product Foundation;
- Product Vision;
- Domain Model;
- User Journeys;
- Application Blueprint;
- UX Architecture;
- Software Architecture;
- MAP Form.

These documents describe what SignalLab is and how the product is intended to behave.

### Milestones

Located in:

```text
docs/milestones/
```

Milestones define:

- implementation order;
- feature ownership;
- scope;
- acceptance criteria;
- MVP and post-MVP boundaries.

### Operational architecture

Located in:

```text
docs/architecture/
```

Main documents:

- [Solution Architecture](docs/architecture/000%20-%20SolutionArchitecture.md)
- [Backend Conventions](docs/architecture/001%20-%20BackendConventions.md)
- [Frontend Conventions](docs/architecture/002%20-%20FrontendConventions.md)
- [API Conventions](docs/architecture/003%20-%20APIConventions.md)
- [Persistence Conventions](docs/architecture/004%20-%20PersistenceConventions.md)
- [Testing Strategy](docs/architecture/005%20-%20TestingStrategy.md)
- [Application Shell](docs/architecture/006%20-%20ApplicationShell.md)
- [Navigation Architecture](docs/architecture/007%20-%20NavigationArchitecture.md)
- [PostgreSQL](docs/architecture/008%20-%20PostgreSQL.md)
- [Entity Framework Core](docs/architecture/009%20-%20Entity%20Framework%20Core.md)
- [Environments](docs/architecture/010%20-%20Environments.md)
- [Docker](docs/architecture/011%20-%20Docker.md)
- [CI/CD](docs/architecture/012%20-%20CI-CD.md)
- [First Deployment](docs/architecture/013%20-%20FirstDeployment.md)
- [Developer Guide](docs/architecture/014%20-%20DeveloperGuide.md)

The conceptual technical reference remains:

```text
docs/product/06 - SoftwareArchitecture.md
```

### Architecture Decision Records

Architectural decisions are stored in:

```text
docs/architecture/adr/
```

The ADR index is:

[Architecture Decision Records](docs/architecture/adr/000%20-%20ADRIndex.md)

ADR documents preserve why structural decisions were made.

Operational architecture documents describe how those decisions currently apply to the repository.

---

## Environments

SignalLab recognizes exactly:

```text
Development
Staging
Production
```

The current Staging environment is available at:

```text
Frontend
https://staging.signallab.dev

API
https://api.staging.signallab.dev
```

The complete production MVP is not deployed yet.

Production infrastructure is finalized by a later Milestone.

---

## MVP boundaries

The current Platform Foundation intentionally contains no business domain.

Some technologies are part of the MVP architecture but belong to later Milestones, such as Clerk authentication.

Other capabilities are explicitly deferred until a demonstrated need exists, including:

- SignalR;
- real-time collaboration;
- ResearchBoards;
- Redis;
- Microservices;
- distributed Event Bus infrastructure;
- specialized Background Jobs;
- binary Build storage;
- explicit API versioning.

Features or infrastructure must not be implemented early only to anticipate future needs.

---

## License

SignalLab is proprietary software.

Copyright © 2026 Jimmy Fromonot. All rights reserved.

See `LICENSE` for details.