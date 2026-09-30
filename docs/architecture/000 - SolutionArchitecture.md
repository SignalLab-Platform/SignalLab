# Architecture de la solution

**Projet :** SignalLab
**Document :** Architecture opérationnelle de la solution
**Version :** 1.0
**Statut :** Validé
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# 1. Objectif

Ce document définit l’architecture opérationnelle utilisée pour développer SignalLab.

Il traduit les principes du document `SoftwareArchitecture` en règles concrètes applicables au repository.

Il précise notamment :

* la structure générale de la solution ;
* la direction des dépendances ;
* les responsabilités du Backend et du Frontend ;
* l’organisation du Modular Monolith ;
* la relation entre Clean Architecture et Vertical Slices ;
* les frontières entre contrats HTTP, Application et Domain ;
* les environnements cibles ;
* les technologies volontairement différées.

Ce document ne décrit pas les fonctionnalités métier.

Le Domain Model, les User Journeys, l’Application Blueprint et les Milestones restent les références fonctionnelles du produit.

---

# 2. Hiérarchie documentaire

Les documents SignalLab possèdent des responsabilités distinctes.

```text
Product Foundation, Product Vision et Domain Model
    Définissent le produit, les concepts et les invariants
        ↓
Software Architecture
    Définit la vision technique et les choix de stack
        ↓
Documentation opérationnelle de docs/architecture
    Définit les règles concrètes applicables au repository
        ↓
Architecture Decision Records
    Conservent le contexte et la justification des décisions
        ↓
Code, tests et validation du repository
    Appliquent et vérifient les règles
```

La documentation opérationnelle décrit l’architecture actuellement applicable.

Les ADR conservent l’historique des décisions structurantes. Lorsqu’une décision change, une nouvelle ADR remplace l’ancienne au lieu d’effacer son contexte.

---

# 3. Vue générale

SignalLab est composé de trois éléments principaux :

```text
Navigateur
    ↓ HTTPS / REST / JSON
Frontend Next.js
    ↓ HTTPS / REST / JSON
API ASP.NET Core
    ↓
Application
    ↓
Domain
    ↑
Infrastructure
    ↓
PostgreSQL
```

Clerk assure l’authentification lorsque la Spec dédiée l’introduit.

Le Frontend constitue l’interface utilisateur.

L’API ASP.NET Core constitue l’autorité métier.

PostgreSQL constitue la source de vérité persistante.

---

# 4. Principes architecturaux

## 4.1 Backend autoritaire

Le Backend constitue l’autorité de toutes les données et règles métier.

Le Frontend représente l’état du système et transmet les intentions utilisateur.

Aucune règle métier importante ne doit être garantie exclusivement par le Frontend.

Les validations côté Frontend améliorent l’expérience utilisateur mais ne remplacent jamais les validations côté Backend.

## 4.2 Domaine indépendant des technologies

Le Domain exprime uniquement les concepts et invariants propres à SignalLab.

Il ne connaît pas :

* ASP.NET Core ;
* Entity Framework Core ;
* PostgreSQL ;
* Clerk ;
* SignalR ;
* React ;
* Next.js ;
* TanStack Query ;
* les formats HTTP ou JSON.

## 4.3 Dépendances dirigées vers le cœur

Les dépendances entre les projets Backend sont limitées à la direction suivante :

```text
SignalLab.Api
├── SignalLab.Application
└── SignalLab.Infrastructure

SignalLab.Infrastructure
├── SignalLab.Application
└── SignalLab.Domain

SignalLab.Application
└── SignalLab.Domain

SignalLab.Domain
└── aucune dépendance vers un autre projet SignalLab
```

Les couches internes ne connaissent pas les couches externes.

## 4.4 Simplicité progressive

Une technologie ou une abstraction n’est ajoutée que lorsqu’un besoin concret le justifie.

Le MVP n’introduit pas par anticipation :

* Microservices ;
* Redis ;
* système spécialisé de Background Jobs ;
* Event Bus distribué ;
* SignalR ;
* MediatR ;
* AutoMapper ;
* Generic Repository ;
* versioning explicite de l’API.

Toute introduction future d’une capacité structurante nécessite une Spec propriétaire et, lorsque la décision affecte durablement l’architecture, une ADR.

---

# 5. Modular Monolith

SignalLab est un Modular Monolith.

Cela signifie que le Backend possède :

* un seul processus ASP.NET Core ;
* un seul déploiement Backend ;
* une seule base de données PostgreSQL ;
* une seule solution .NET ;
* plusieurs modules métier séparés logiquement ;
* aucune communication réseau entre modules internes.

Les modules correspondent aux grandes capacités métier de SignalLab, notamment :

```text
Identity
Organizations
Projects
Builds
Measures
Forms
Campaigns
Participation
Results
Analytics
MAP
```

Cette liste représente l’architecture cible. SL-013 ne crée aucun de ces domaines métier.

Les modules sont organisés par dossiers cohérents dans les projets existants plutôt que par un projet .NET distinct pour chaque module.

```text
SignalLab.Domain/
├── Common/
├── Organizations/
├── Projects/
└── ...

SignalLab.Application/
├── Common/
├── Organizations/
├── Projects/
└── ...

SignalLab.Infrastructure/
├── Persistence/
├── Identity/
└── ...

SignalLab.Api/
├── Endpoints/
└── ...
```

Un module ne doit pas modifier directement les données internes d’un autre module en contournant ses cas d’usage et ses invariants.

Les dépendances transversales entre modules doivent rester explicites.

---

# 6. Architecture Backend

Le Backend suit la Clean Architecture autour de quatre responsabilités.

## 6.1 Presentation

Projet actuel :

```text
SignalLab.Api
```

Responsabilités :

* héberger l’application ASP.NET Core ;
* exposer les Endpoints HTTP ;
* recevoir et valider techniquement les Requests ;
* traduire les Requests en intentions Application ;
* sérialiser les Responses ;
* produire les Problem Details ;
* configurer les Middlewares ;
* exposer OpenAPI ;
* composer les dépendances de l’application.

La Presentation ne contient aucune règle métier.

`Program.cs` reste un Composition Root. Il ne doit pas accumuler les implémentations des Endpoints métier.

SignalLab utilise les Minimal APIs, organisées par opération.

## 6.2 Application

Projet actuel :

```text
SignalLab.Application
```

Responsabilités :

* représenter les cas d’usage ;
* orchestrer les opérations ;
* charger les données nécessaires par des interfaces ;
* vérifier les autorisations applicatives ;
* appeler les comportements du Domain ;
* demander la persistence ;
* retourner un résultat explicite ;
* propager l’annulation.

L’Application peut contenir :

* Commands ;
* Queries ;
* Handlers ;
* Results ;
* projections de lecture ;
* interfaces de persistence ;
* interfaces de services externes.

L’Application ne contient pas les invariants propres aux entités métier.

Les cas d’usage sont appelés explicitement. MediatR n’est pas utilisé.

## 6.3 Domain

Projet actuel :

```text
SignalLab.Domain
```

Responsabilités :

* entités ;
* Value Objects ;
* invariants ;
* comportements métier ;
* transitions d’état ;
* erreurs métier ;
* Domain Events uniquement lorsqu’un besoin démontré le justifie.

Le Domain ne dépend d’aucune technologie d’infrastructure ou de présentation.

Le Domain Model constitue la référence conceptuelle des noms, relations et invariants.

## 6.4 Infrastructure

Projet actuel :

```text
SignalLab.Infrastructure
```

Responsabilités actuelles :

- Entity Framework Core ;
- `SignalLabDbContext` ;
- configurations Fluent API ;
- migrations ;
- configuration de la persistence PostgreSQL ;
- implémentations techniques des interfaces définies par les couches internes.

Responsabilités introduites ultérieurement lorsqu'une Spec propriétaire le nécessite :

- intégration avec Clerk ;
- autres services externes ;
- services techniques supplémentaires justifiés par un besoin concret.

Infrastructure fournit les détails techniques attendus par les couches internes.

Aucune règle métier ne doit être implémentée exclusivement dans Infrastructure.

---

# 7. Clean Architecture et Vertical Slices

Clean Architecture et Vertical Slices répondent à deux problèmes différents.

La Clean Architecture définit la direction des dépendances entre couches.

Les Vertical Slices organisent les cas d’usage à l’intérieur des modules.

Exemple cible :

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
        ├── ArchiveProjectCommand.cs
        ├── ArchiveProjectHandler.cs
        └── ArchiveProjectResult.cs
```

Une Vertical Slice représente une intention métier précise.

Le repository ne doit pas être principalement organisé autour de catégories génériques comme :

```text
Services
Managers
Helpers
Models
Utils
```

Un code transversal est placé dans `Common` ou dans un dossier portant le nom précis de sa responsabilité.

---

# 8. Contrats et transformations

Les frontières entre couches utilisent des modèles distincts.

```text
HTTP Request
    ↓ mapping explicite
Application Command ou Query
    ↓
Domain
    ↓
Application Result ou projection
    ↓ mapping explicite
HTTP Response
```

Règles :

* les entités Domain ne sont jamais sérialisées directement ;
* les Requests et Responses HTTP appartiennent à `SignalLab.Api` ;
* les Commands, Queries et Results appartiennent à `SignalLab.Application` ;
* les modèles TypeScript représentent les contrats HTTP consommés par le Frontend ;
* les modèles TypeScript ne constituent pas le Domain Model ;
* les mappings sont explicites ;
* AutoMapper n’est pas utilisé sans besoin démontré et ADR ;
* un modèle possède un nom décrivant son rôle plutôt qu’un suffixe générique lorsque cela est possible.

Exemples :

```text
CreateProjectRequest
CreateProjectCommand
CreateProjectResult
ProjectResponse
ProjectSummary
ProjectDetails
```

---

# 9. Communication API

La communication entre Frontend et Backend utilise :

* HTTPS ;
* REST ;
* JSON ;
* OpenAPI.

Les conventions détaillées sont définies dans :

```text
003 - APIConventions.md
```

Principes généraux :

* routes fondées sur des ressources ;
* JSON en `camelCase` ;
* Problem Details pour les erreurs HTTP ;
* pagination des collections potentiellement importantes ;
* aucun versioning explicite de l’API pour le MVP ;
* propagation des `CancellationToken` ;
* aucun comportement métier dans les Endpoints.

---

# 10. Architecture Frontend

Le Frontend utilise :

* React ;
* Next.js App Router ;
* TypeScript ;
* Tailwind CSS ;
* shadcn/ui ;
* TanStack Query.

Next.js est utilisé comme framework d’application Frontend, pas comme Backend métier.

Le routing appartient exclusivement à l’App Router.

Structure cible :

```text
src/
├── app/
├── components/
│   └── ui/
├── features/
├── providers/
└── lib/
```

Responsabilités :

* `app/` contient les routes, layouts et compositions de pages ;
* `features/` contient les capacités liées à un domaine ou cas d’usage ;
* `components/ui/` contient les primitives génériques indépendantes du métier ;
* `providers/` contient les providers globaux ;
* `lib/` contient l’infrastructure Frontend transversale précisément nommée.

TanStack Query gère le Server State.

Le Client State reste local autant que possible.

Aucune règle métier importante ne doit dépendre exclusivement d’un composant React.

---

# 11. Persistence

La persistence utilise PostgreSQL et Entity Framework Core.

Leur configuration initiale a été réalisée respectivement par SL-016 et SL-017.

Les règles opérationnelles détaillées sont documentées dans :

```text
004 - PersistenceConventions.md
008 - PostgreSQL.md
009 - Entity Framework Core.md
```

Règles générales :

* EF Core reste dans Infrastructure ;
* `DbContext`, migrations et configurations appartiennent à Infrastructure ;
* Domain et Application ne référencent pas EF Core ;
* chaque entité persistée possède une configuration distincte ;
* les contraintes structurelles sont renforcées par la base ;
* les invariants métier restent exprimés dans le Domain ;
* les transactions sont délimitées par les cas d’usage ;
* aucun Generic Repository n’est introduit ;
* les formules analytiques métier ne sont pas définies exclusivement en SQL.

Les conventions détaillées sont définies dans :

```text
004 - PersistenceConventions.md
```

---

# 12. Tests

La stratégie couvre plusieurs niveaux :

```text
Tests unitaires Domain
Tests de cas d’usage Application
Tests d’architecture
Tests d’intégration API
Tests d’intégration Persistence
Tests de composants Frontend
Tests end-to-end pour les parcours critiques futurs
```

Les tests valident prioritairement les comportements, invariants et frontières architecturales.

Un pourcentage de couverture arbitraire ne remplace pas des scénarios pertinents.

Les conventions détaillées sont définies dans :

```text
005 - TestingStrategy.md
```

---

# 13. Environnements

SignalLab distingue trois environnements supportés :

```text
Development
Staging
Production
```

SL-013 a défini cette architecture cible.

SL-018 a introduit leur configuration concrète, la validation des variables, la gestion des secrets et les erreurs de démarrage.

Les règles opérationnelles sont documentées dans :

```text
010 - Environments.md
```

Aucun secret réel ne doit être commité.

---

# 14. Capacités différées

Les capacités suivantes ne sont pas introduites sans besoin fonctionnel et Spec propriétaire :

* SignalR ;
* collaboration temps réel ;
* Redis ;
* Event Bus distribué ;
* Microservices ;
* Hangfire ou autre système de Background Jobs ;
* stockage de fichiers binaires ;
* versioning explicite de l’API.

Le Backend reste stateless afin de permettre une évolution future sans imposer immédiatement une architecture distribuée.

---

# 15. Documentation opérationnelle

Les documents suivants constituent la documentation opérationnelle de la Platform Foundation :

```text
000 - SolutionArchitecture.md
001 - BackendConventions.md
002 - FrontendConventions.md
003 - APIConventions.md
004 - PersistenceConventions.md
005 - TestingStrategy.md
006 - ApplicationShell.md
007 - NavigationArchitecture.md
008 - PostgreSQL.md
009 - Entity Framework Core.md
010 - Environments.md
011 - Docker.md
012 - CI-CD.md
013 - FirstDeployment.md
014 - DeveloperGuide.md
```

Leur responsabilité est de décrire les règles actuellement applicables au repository.

Les décisions architecturales structurantes et leur justification sont conservées dans :

```text
docs/architecture/adr/
```

Les documents produit et conceptuels restent situés dans :

```text
docs/product/
```

Les Milestones et Specs définissent le périmètre et l'ordre d'implémentation dans :

```text
docs/milestones/
```

---

# 16. Évolution de l’architecture

Toute modification structurante doit :

1. être motivée par un besoin concret ;
2. identifier les documents et couches affectés ;
3. posséder une Spec propriétaire lorsque nécessaire ;
4. créer une nouvelle ADR si elle modifie une décision durable ;
5. mettre à jour les tests d’architecture ;
6. mettre à jour la documentation opérationnelle ;
7. être validée avant son utilisation dans un domaine métier.

Une documentation obsolète ne doit pas être conservée silencieusement comme règle active.

---

# 17. Références

* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `04 - ApplicationBlueprint.md`
* `05 - UXArchitecture.md`
* `M - Milestones.md`
* ADR présentes dans `docs/architecture/adr/`
