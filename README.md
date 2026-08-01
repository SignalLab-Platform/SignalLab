# SignalLab

SignalLab is a Game User Research platform designed to transform playtest feedback into structured, comparable, and durable knowledge.

This repository contains the product documentation and the future source code of the SignalLab MVP.

## Repository status

SignalLab is currently in the Platform Foundation milestone.

The ASP.NET Core Backend foundation and the Next.js Frontend foundation are initialized.

At this stage:

* the Backend follows the initial Clean Architecture project structure;
* the API exposes a health endpoint and Development OpenAPI document;
* the Frontend uses Next.js App Router, React, TypeScript, Tailwind CSS, shadcn/ui, and TanStack Query;
* Backend integration tests and Frontend component tests are configured;
* PostgreSQL and Entity Framework Core are not configured;
* the Application Shell and navigation architecture are not implemented;
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
* .NET SDK 10.0.302 or a compatible .NET 10 SDK allowed by `global.json`;
* Node.js 24.18.0 or a compatible Node.js 24 version;
* npm 11 or a compatible version.

Docker and PostgreSQL will be documented when their corresponding infrastructure is initialized.

Node.js, Docker, and PostgreSQL will be documented when their corresponding applications and infrastructure are initialized.

## Repository validation

Run the structural validation from the repository root:

bash scripts/validate-repository.sh

The script verifies that the directories and foundational files required are present.

## Development commands

### Validate the repository structure:

```bash
bash scripts/validate-repository.sh
```

### Backend

```powershell
dotnet restore apps/api/SignalLab.Api.sln
dotnet build apps/api/SignalLab.Api.sln --configuration Release
dotnet test apps/api/SignalLab.Api.sln --configuration Release
dotnet run --launch-profile https --project apps/api/src/SignalLab.Api/SignalLab.Api.csproj
```

### Frontend

```powershell
npm.cmd --prefix apps/web clean-install
npm.cmd --prefix apps/web run dev
npm.cmd --prefix apps/web run lint
npm.cmd --prefix apps/web run typecheck
npm.cmd --prefix apps/web test
npm.cmd --prefix apps/web run build
```

## License

SignalLab is proprietary software.

Copyright © 2026 Jimmy Fromonot. All rights reserved.

See LICENSE for details.
