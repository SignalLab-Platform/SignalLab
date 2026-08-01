SignalLab

SignalLab is a Game User Research platform designed to transform playtest feedback into structured, comparable, and durable knowledge.

This repository contains the product documentation and the future source code of the SignalLab MVP.

Repository status

SignalLab is currently in the Platform Foundation milestone.

The repository structure and development conventions are being established before the Backend, Frontend, persistence, and deployment environments are initialized.

At this stage:

* the ASP.NET Core API is not initialized;
* the Next.js application is not initialized;
* PostgreSQL and Entity Framework Core are not configured;
* Docker and CI/CD are not configured;
* no business domain is implemented.
* Target architecture

SignalLab is designed as a Modular Monolith composed of:

* an ASP.NET Core Backend;
*  a Next.js Frontend using React and TypeScript;
*  PostgreSQL as the persistent data store;
* Entity Framework Core for persistence;
* Docker for reproducible local environments.

The Backend follows Clean Architecture principles with Presentation, Application, Domain, and Infrastructure responsibilities.

Repository structure
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

Documentation

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

Current requirements

The current repository validation requires:

* Git;
* Bash.

Additional requirements such as the .NET SDK, Node.js, Docker, and PostgreSQL will be documented when their corresponding applications and infrastructure are initialized.

Repository validation

Run the structural validation from the repository root:

bash scripts/validate-repository.sh

The script verifies that the directories and foundational files required are present.

Development commands

No Backend or Frontend development command is available yet.

Commands for building, testing, and running the applications will be added as the corresponding specifications are implemented.

License

SignalLab is proprietary software.

Copyright © 2026 Jimmy Fromonot. All rights reserved.

See LICENSE for details.
