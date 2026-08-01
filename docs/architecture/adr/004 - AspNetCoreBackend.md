# ADR 004 — ASP.NET Core Backend

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# Context

SignalLab nécessite un Backend capable de porter :

* les règles métier ;
* les autorisations ;
* le multi-tenant par Organization ;
* la Persistence relationnelle ;
* les transactions ;
* les calculs analytiques ;
* les contrats REST ;
* l’authentification future via validation de JWT Clerk ;
* la journalisation ;
* les Health Checks ;
* OpenAPI ;
* une évolution future vers SignalR si un besoin post-MVP le justifie.

Le Backend doit constituer l’unique autorité métier.

Il doit pouvoir isoler le Domain des technologies et permettre de tester les règles sans démarrer l’infrastructure complète.

La solution doit rester compatible avec un Modular Monolith et une équipe initiale réduite.

---

# Decision

SignalLab utilise **ASP.NET Core avec C#** comme plateforme Backend canonique.

Le Backend est organisé selon la Clean Architecture dans les projets suivants :

```text
SignalLab.Api
SignalLab.Application
SignalLab.Domain
SignalLab.Infrastructure
```

ASP.NET Core fournit :

* l’hébergement HTTP ;
* les Minimal APIs ;
* la Dependency Injection ;
* les Middlewares ;
* la configuration ;
* les Health Checks ;
* OpenAPI ;
* l’authentification et l’autorisation ;
* la gestion centralisée des erreurs ;
* l’intégration future avec l’observabilité ;
* la possibilité d’introduire SignalR ultérieurement.

C# exprime :

* le Domain ;
* les cas d’usage ;
* les contrats Application ;
* les intégrations Infrastructure ;
* les Endpoints HTTP.

Toutes les modifications de données transitent par l’API ASP.NET Core.

---

# Authority

L’API ASP.NET Core constitue l’autorité des opérations métier.

Le Backend vérifie :

* identité ;
* Permissions ;
* appartenance à une Organization ;
* invariants ;
* transitions d’état ;
* cohérence transactionnelle ;
* validation technique ;
* contraintes applicatives.

Le Frontend peut anticiper ou représenter ces règles, mais ne peut pas les garantir seul.

---

# Clean Architecture

Direction des dépendances :

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
└── aucune référence vers un autre projet SignalLab
```

## SignalLab.Api

Responsable de :

* Presentation HTTP ;
* Minimal APIs ;
* Requests et Responses ;
* Problem Details ;
* OpenAPI ;
* Middlewares ;
* Composition Root.

## SignalLab.Application

Responsable de :

* Commands ;
* Queries ;
* Handlers ;
* Results ;
* orchestration ;
* autorisations applicatives ;
* interfaces de Persistence ;
* interfaces de services externes.

## SignalLab.Domain

Responsable de :

* entités ;
* Value Objects ;
* invariants ;
* comportements ;
* transitions métier.

## SignalLab.Infrastructure

Responsable de :

* Entity Framework Core ;
* PostgreSQL ;
* migrations ;
* intégrations externes ;
* services techniques.

---

# API Style

La Presentation HTTP utilise les Minimal APIs.

Chaque opération métier possède un Endpoint dédié et appelle explicitement un Handler Application.

`Program.cs` reste limité à la composition.

Exemple cible :

```text
SignalLab.Api/
└── Endpoints/
    └── Projects/
        └── CreateProject/
            ├── CreateProjectEndpoint.cs
            ├── CreateProjectRequest.cs
            └── CreateProjectResponse.cs
```

Les Controllers MVC ne sont pas utilisés pour les nouveaux Endpoints SignalLab.

Ce choix est détaillé dans :

```text
010 - MinimalAPIs.md
```

---

# Asynchronism

Les opérations I/O sont asynchrones.

Les `CancellationToken` sont propagés depuis l’Endpoint jusqu’à :

* Application ;
* Persistence ;
* services externes ;
* moteur analytique lorsque pertinent.

Les calculs analytiques doivent être :

* déterministes ;
* annulables ;
* bornés ;
* mesurés.

Le calcul synchrone reste accepté tant que les budgets documentés sont respectés.

---

# Dependency Injection

La Dependency Injection native d’ASP.NET Core constitue le mécanisme canonique de composition.

Les dépendances sont injectées par constructeur.

Le Service Locator est interdit dans le code applicatif.

Les enregistrements sont regroupés par responsabilité :

```text
AddApplication()
AddInfrastructure()
MapPlatformEndpoints()
MapProjectEndpoints()
```

Les modules ne résolvent pas eux-mêmes des services depuis `IServiceProvider`.

---

# Configuration

La configuration est externalisée selon les environnements :

```text
Development
Staging
Production
```

Les secrets ne sont jamais commitées.

La validation des variables au démarrage appartient à SL-018.

Le Backend doit échouer explicitement lorsqu’une configuration obligatoire est invalide ou absente.

---

# Error Management

Les erreurs HTTP utilisent Problem Details.

Les erreurs sont classées selon leur nature :

```text
Validation
Unauthorized
Forbidden
NotFound
Conflict
Unexpected
```

Le Domain et Application ne dépendent pas des codes HTTP.

La Presentation traduit leurs résultats vers le protocole HTTP.

Les erreurs inattendues sont gérées de manière centralisée.

Les réponses ne doivent pas exposer :

* stack traces ;
* chaînes de connexion ;
* requêtes SQL ;
* tokens ;
* secrets ;
* détails Infrastructure sensibles.

---

# Health and OpenAPI

Le Backend expose un Health Endpoint technique.

Convention actuelle :

```http
GET /health
```

OpenAPI documente les contrats publics.

Son exposition selon les environnements est configurée par les Specs d’environnement et de déploiement.

Le Health Endpoint et OpenAPI ne contiennent aucune règle métier.

---

# Consequences

## Positive

### Autorité métier unique

Toutes les règles importantes sont exécutées dans une plateforme Backend cohérente.

### Typage statique

C# fournit une forte sécurité lors des refactorings et de la modélisation du Domain.

### Écosystème mature

ASP.NET Core fournit nativement les capacités HTTP, DI, sécurité, configuration et diagnostic nécessaires.

### Performance

La plateforme est adaptée aux API HTTP, aux accès PostgreSQL et aux calculs synchrones bornés du MVP.

### Clean Architecture

Les projets .NET permettent de contrôler explicitement la direction des dépendances.

### Tests d’intégration

`WebApplicationFactory<Program>` permet de tester l’application HTTP réelle.

### Évolution progressive

SignalR, Background Jobs ou infrastructure supplémentaire peuvent être ajoutés plus tard sans être nécessaires au MVP.

## Negative

### Deux écosystèmes

Le repository utilise :

```text
TypeScript / Node.js côté Frontend
C# / .NET côté Backend
```

Le développeur doit maintenir deux chaînes d’outillage.

### Mapping des contrats

Les contrats C# et TypeScript doivent être coordonnés.

La génération automatique n’est pas introduite pendant SL-013.

### Déploiements distincts

Frontend et Backend constituent deux applications à construire, configurer et déployer.

### Discipline architecturale

ASP.NET Core permettrait techniquement de placer toute la logique dans les Endpoints.

Les conventions et tests doivent empêcher cette dérive.

---

# Alternatives Considered

## Next.js Full-Stack

### Rejected

Cette option permettrait d’utiliser TypeScript sur toute la stack.

Elle est rejetée car :

* le Backend C# est déjà validé ;
* le Domain exige une modélisation indépendante du Frontend ;
* les capacités relationnelles et analytiques sont mieux centralisées dans l’API ;
* Next.js ne doit pas devenir l’autorité métier ;
* elle mélangerait les responsabilités de déploiement et d’interface.

## Node.js avec NestJS

### Rejected

NestJS offre une organisation modulaire et une Dependency Injection structurée.

Il n’est pas retenu car :

* ASP.NET Core constitue la stack validée ;
* C# est retenu pour le Domain ;
* .NET fournit l’intégration canonique avec Entity Framework Core ;
* changer de plateforme n’apporte aucune valeur MVP démontrée.

## Java avec Spring Boot

### Rejected

Spring Boot possède un écosystème robuste.

Il est rejeté car :

* il ajouterait une stack différente sans avantage spécifique ;
* l’expertise et l’architecture validée reposent sur .NET ;
* Entity Framework Core est déjà retenu.

## Python avec Django ou FastAPI

### Rejected

Ces frameworks permettraient une mise en œuvre rapide.

Ils ne sont pas retenus car :

* le typage et la modélisation du Domain seraient moins alignés avec la décision C# ;
* la stack canonique est ASP.NET Core ;
* aucun besoin data-science exécuté côté serveur ne justifie un Backend Python.

## Backend as a Service

### Rejected

Un Backend as a Service réduirait l’infrastructure initiale.

Il est rejeté car SignalLab possède :

* des invariants complexes ;
* des autorisations multi-tenant ;
* des transactions métier ;
* un moteur analytique déterministe ;
* un besoin de contrôle des contrats et de la Persistence.

---

# Implementation Rules

* Utiliser ASP.NET Core et C#.
* Conserver les quatre projets Clean Architecture.
* Maintenir Domain indépendant des technologies.
* Faire transiter toutes les opérations métier par l’API.
* Utiliser les Minimal APIs pour les nouveaux Endpoints.
* Garder `Program.cs` comme Composition Root.
* Appeler explicitement les Handlers Application.
* Ne pas introduire MediatR sans nouvelle ADR.
* Propager les `CancellationToken`.
* Utiliser la Dependency Injection par constructeur.
* Ne pas résoudre les dépendances par Service Locator.
* Centraliser les Problem Details.
* Documenter les Endpoints avec OpenAPI.
* Ne pas installer SignalR avant sa Milestone et son ADR dédiées.
* Ne pas introduire de logique métier dans Infrastructure ou Api.
* Toute modification de plateforme Backend nécessite une nouvelle ADR.

---

# References

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `003 - APIConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `M - Milestones.md`
* `002 - CleanArchitectureVerticalSlices.md`
* `003 - NextJsFrontend.md`
* `010 - MinimalAPIs.md`
