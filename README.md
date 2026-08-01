# SignalLab

SignalLab is a Game User Research platform designed to transform playtest feedback into structured, comparable, and durable knowledge.

This repository contains the product documentation and the future source code of the SignalLab MVP.

## Repository status

SignalLab is currently in the Platform Foundation milestone.

The ASP.NET Core Backend foundation is initialized with Clean Architecture projects, a health endpoint, runtime OpenAPI generation, and integration tests.

At this stage:

* the ASP.NET Core API foundation is initialized;
* the Next.js application is not initialized;
* PostgreSQL and Entity Framework Core are not configured;
* Docker and CI/CD are not configured;
* no business domain is implemented.

SignalLab is designed as a Modular Monolith composed of:

* an ASP.NET Core Backend;
*  a Next.js Frontend using React and TypeScript;
*  PostgreSQL as the persistent data store;
* Entity Framework Core for persistence;
* Docker for reproducible local environments.

The Backend follows Clean Architecture principles with Presentation, Application, Domain, and Infrastructure responsibilities.

## Repository structure

SignalLab/
├── apps/
│   ├── api/             # ASP.NET Core Backend
│   └── web/             # Next.js Frontend
├── infrastructure/      # Docker and deployment infrastructure
├── tests/               # Cross-application and repository-level tests
├── docs/
│   ├── milestones/      # MVP roadmap and implementation specifications
│   └── product/         # Product and architecture source documents
├── scripts/             # Repository automation and validation scripts
├── .editorconfig
├── .gitattributes
├── .gitignore
├── LICENSE
└── README.md

## Documentation

The product documentation is the source of truth for SignalLab.

The main documents are located in docs/product/ and define:

* the Product Foundation;
* the Product Vision;
* the Domain Model;
* the User Journeys;
* the Application Blueprint;
* the UX Architecture;
* the Software Architecture;
* the MAP questionnaire.

Implementation scope and acceptance criteria are defined in docs/milestones/.

## Current requirements

* Git;
* Bash;
* .NET SDK 10.0.302 or a compatible .NET 10 SDK allowed by `global.json`.

Node.js, Docker, and PostgreSQL will be documented when their corresponding applications and infrastructure are initialized.

## Repository validation

Run the structural validation from the repository root:

bash scripts/validate-repository.sh

The script verifies that the directories and foundational files required are present.

## Development commands

Validate the repository structure:

```bash
bash scripts/validate-repository.sh

dotnet restore apps/api/SignalLab.Api.sln
dotnet build apps/api/SignalLab.Api.sln --configuration Release
dotnet test apps/api/SignalLab.Api.sln --configuration Release
dotnet run --launch-profile https --project apps/api/src/SignalLab.Api/SignalLab.Api.csproj
```

The Frontend has not been initialized yet.

## License

SignalLab is proprietary software.

Copyright © 2026 Jimmy Fromonot. All rights reserved.

See LICENSE for details.
