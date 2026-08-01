# ADR 002 — Clean Architecture et Vertical Slices

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# Context

SignalLab doit protéger deux propriétés distinctes :

1. les règles métier ne doivent pas dépendre des technologies ;
2. le code d’un cas d’usage doit rester localisable et compréhensible.

Une organisation uniquement fondée sur les couches techniques peut produire :

```text
Controllers/
Services/
Repositories/
Models/
```

Dans cette structure, une fonctionnalité est dispersée dans plusieurs dossiers génériques.

À l’inverse, une organisation uniquement fondée sur les Features peut laisser remonter les dépendances technologiques jusque dans les règles métier.

SignalLab doit donc distinguer :

* la direction des dépendances ;
* l’organisation des cas d’usage.

---

# Decision

SignalLab combine :

* **Clean Architecture** pour définir les couches et la direction des dépendances ;
* **Vertical Slices** pour organiser les fonctionnalités à l’intérieur des couches.

## Clean Architecture

Le Backend contient quatre projets :

```text
SignalLab.Api
SignalLab.Application
SignalLab.Domain
SignalLab.Infrastructure
```

Direction autorisée :

```text
Api
├── Application
└── Infrastructure

Infrastructure
├── Application
└── Domain

Application
└── Domain

Domain
└── aucune dépendance vers les autres projets SignalLab
```

Responsabilités :

### Api

Presentation HTTP, Minimal APIs, contrats HTTP, OpenAPI et Composition Root.

### Application

Cas d’usage, Commands, Queries, Handlers, Results et interfaces nécessaires à l’orchestration.

### Domain

Entités, Value Objects, invariants, comportements et transitions métier.

### Infrastructure

Persistence, Entity Framework Core, PostgreSQL et intégrations externes.

## Vertical Slices

À l’intérieur d’un module, chaque cas d’usage possède un dossier dédié.

Exemple :

```text
SignalLab.Application/
└── Projects/
    ├── CreateProject/
    │   ├── CreateProjectCommand.cs
    │   ├── CreateProjectHandler.cs
    │   └── CreateProjectResult.cs
    ├── GetProject/
    │   ├── GetProjectQuery.cs
    │   ├── GetProjectHandler.cs
    │   └── ProjectDetails.cs
    └── ArchiveProject/
```

La Presentation reflète la même intention :

```text
SignalLab.Api/
└── Endpoints/
    └── Projects/
        └── CreateProject/
            ├── CreateProjectEndpoint.cs
            ├── CreateProjectRequest.cs
            └── CreateProjectResponse.cs
```

Une Vertical Slice représente une intention précise, par exemple :

```text
CreateProject
GetProject
ArchiveProject
ActivateCampaign
SubmitParticipation
```

Elle ne représente pas une catégorie générique comme `Services` ou `Managers`.

---

# Consequences

## Positive

### Direction des dépendances explicite

Le Domain reste indépendant d’ASP.NET Core, Entity Framework Core, PostgreSQL et des contrats HTTP.

### Localisation des fonctionnalités

Les types associés à un cas d’usage sont regroupés autour de la même intention.

### Changements plus ciblés

Une évolution fonctionnelle affecte principalement sa Slice au lieu d’un service global contenant plusieurs responsabilités.

### Nommage métier visible

L’arborescence reflète les capacités du produit.

### Tests alignés sur les comportements

Les tests peuvent être organisés selon les mêmes modules et cas d’usage.

### Réduction des services trop larges

Les Handlers possèdent une responsabilité limitée.

## Negative

### Duplication acceptable

Deux Slices peuvent contenir des transformations ou validations techniques proches.

Cette duplication limitée est préférée à une abstraction prématurée.

### Nombre de fichiers plus important

Un cas d’usage peut nécessiter plusieurs petits fichiers :

```text
Request
Command
Handler
Result
Response
```

Cette granularité est acceptée lorsqu’elle rend les frontières explicites.

### Discipline nécessaire

Les développeurs doivent distinguer :

* contrat HTTP ;
* contrat Application ;
* modèle Domain ;
* modèle de Persistence.

### Risque de Common générique

Le code partagé peut être déplacé trop tôt dans `Common`.

Une abstraction transversale n’est créée qu’après l’apparition d’un besoin réel et cohérent.

---

# Alternatives Considered

## Architecture en couches uniquement

### Rejected

Une organisation principalement composée de :

```text
Controllers
Services
Repositories
Models
```

disperse les fonctionnalités et favorise les services génériques.

Les couches restent nécessaires pour les dépendances, mais elles sont complétées par les modules et Vertical Slices.

## Vertical Slice sans séparation de projets

### Rejected

Placer Endpoint, logique métier et Persistence dans une même Slice physique simplifierait localement la navigation.

Cette option est rejetée car elle permettrait aux règles métier de dépendre facilement d’ASP.NET Core ou d’Entity Framework Core.

## Hexagonal Architecture avec ports et adapters formalisés partout

### Not Selected

Les principes sont compatibles avec SignalLab, notamment les interfaces Application et les implémentations Infrastructure.

Cependant, une terminologie et une structure supplémentaire de ports/adapters ne sont pas imposées tant que Clean Architecture exprime suffisamment les frontières.

## MediatR

### Deferred

MediatR pourrait dispatcher les Commands et Queries et fournir des pipelines transversaux.

Il n’est pas retenu actuellement car :

* les Handlers peuvent être appelés explicitement ;
* aucun pipeline complexe n’est nécessaire ;
* il ajouterait une abstraction implicite ;
* il ne constitue pas une exigence de la Software Architecture.

Son introduction future nécessite une nouvelle ADR.

## Feature folders uniquement dans Api

### Rejected

Cette option organiserait la Presentation sans organiser Application et Domain selon les capacités métier.

Les modules doivent rester visibles dans toutes les couches concernées.

---

# Implementation Rules

* Les dépendances suivent la matrice Clean Architecture.
* Les cas d’usage sont organisés par module puis intention.
* Les Commands modifient l’état.
* Les Queries lisent ou projettent l’état.
* Les Handlers sont appelés explicitement.
* MediatR n’est pas utilisé.
* Les Requests et Responses restent dans Api.
* Les Commands, Queries et Results restent dans Application.
* Les invariants restent dans Domain.
* Les détails techniques restent dans Infrastructure.
* Les dossiers génériques sont évités.
* `Common` ne devient pas un propriétaire par défaut.
* Toute nouvelle dépendance de projet est couverte par les tests d’architecture.

---

# References

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `003 - APIConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `001 - ModularMonolith.md`
* `010 - MinimalAPIs.md`
