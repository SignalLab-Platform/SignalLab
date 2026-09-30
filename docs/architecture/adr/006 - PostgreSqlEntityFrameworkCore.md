# ADR 006 — PostgreSQL et Entity Framework Core

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire de la décision :** SL-013 — Figer l’architecture de la solution
**Specs propriétaires de l’implémentation :**

* SL-016 — Configurer PostgreSQL
* SL-017 — Configurer Entity Framework Core

---

# Context

Le modèle métier de SignalLab est principalement relationnel.

Il comporte notamment des relations structurées entre :

```text
User
Organization
OrganizationMember
Project
Build
Measure
FormTemplate
Campaign
Participation
Submission
Response
MAPProfile
SavedAnalysis
```

Le système doit garantir :

* intégrité référentielle ;
* transactions atomiques ;
* contraintes d’unicité ;
* isolation des Organizations ;
* conservation des données historiques ;
* requêtes filtrées et paginées ;
* migrations reproductibles ;
* persistence des configurations métier ;
* recalcul des projections analytiques à partir des données persistées.

Le Backend est développé en C# avec ASP.NET Core.

La solution doit conserver le Domain indépendant de la technologie de Persistence.

---

# Decision

SignalLab utilise :

```text
PostgreSQL
Entity Framework Core
```

PostgreSQL constitue la base de données relationnelle principale et la source de vérité persistante.

Entity Framework Core constitue l’Object-Relational Mapper utilisé par Infrastructure.

Le flux canonique est :

```text
Application Handler
↓
Interface Application
↓
Implémentation Infrastructure
↓
Entity Framework Core
↓
PostgreSQL
```

Le Domain ne référence ni PostgreSQL ni Entity Framework Core.

---

# PostgreSQL Responsibilities

PostgreSQL garantit les contraintes structurelles, notamment :

* Primary Keys ;
* Foreign Keys ;
* colonnes obligatoires ;
* contraintes d’unicité ;
* Check Constraints lorsque pertinentes ;
* index ;
* atomicité des transactions.

PostgreSQL ne constitue pas l’emplacement exclusif des règles métier.

Les invariants restent exprimés dans le Domain.

Une contrainte de base peut renforcer un invariant lorsqu’elle est représentable sans ambiguïté.

---

# Entity Framework Core Responsibilities

Entity Framework Core prend en charge :

* mapping entre objets C# et tables PostgreSQL ;
* génération des requêtes SQL ;
* suivi des modifications ;
* Persistence des changements ;
* gestion du `DbContext` ;
* transactions ;
* migrations ;
* projections de lecture ;
* matérialisation asynchrone.

Entity Framework Core observe et persiste les modifications produites par le Domain.

Il ne remplace pas les comportements Domain.

---

# Layer Boundaries

## Domain

Le Domain contient :

* entités ;
* Value Objects ;
* invariants ;
* comportements ;
* transitions d’état.

Il ne contient pas :

```text
DbContext
DbSet
Migration
EntityTypeBuilder
IQueryable EF Core
Attributs de mapping EF Core
```

## Application

Application définit les capacités nécessaires aux cas d’usage.

Exemples :

```text
IProjectReader
IProjectWriter
ICampaignReader
ICampaignWriter
```

Application ne référence pas Entity Framework Core.

Elle ne reçoit pas de `DbContext` ou de `IQueryable` fourni par Infrastructure.

## Infrastructure

Infrastructure contient :

* `SignalLabDbContext` ;
* configurations Fluent API ;
* migrations ;
* implémentations de Persistence ;
* requêtes EF Core ;
* configuration PostgreSQL.

## Api

Api configure la Persistence dans le Composition Root.

Les Endpoints métier n’accèdent pas directement au `DbContext`.

---

# DbContext

SignalLab utilise initialement un `DbContext` principal :

```text
SignalLabDbContext
```

Dans une requête HTTP classique, sa durée de vie est Scoped.

Le `DbContext` constitue déjà une Unit of Work technique.

SignalLab n’ajoute pas par défaut une abstraction générique supplémentaire d’Unit of Work au-dessus d’EF Core.

---

# Mapping

Le mapping utilise prioritairement Fluent API dans Infrastructure.

Chaque entité persistée dispose d’une configuration distincte lorsque nécessaire.

Exemple cible :

```text
Persistence/
├── SignalLabDbContext.cs
├── Configurations/
│   ├── OrganizationConfiguration.cs
│   ├── ProjectConfiguration.cs
│   └── CampaignConfiguration.cs
└── Migrations/
```

Les attributs EF Core ne sont pas ajoutés aux entités Domain.

Les conventions exactes de nommage des tables, colonnes, clés et index seront fixées par SL-017 avant la première migration.

---

# Reading Strategy

Les lectures utilisent :

* `AsNoTracking` lorsqu’aucune modification n’est nécessaire ;
* projections vers des modèles Application ;
* pagination avant matérialisation ;
* sélection des colonnes nécessaires ;
* exécution asynchrone ;
* propagation du `CancellationToken`.

`IQueryable` ne traverse pas la frontière Infrastructure → Application.

Infrastructure exécute les requêtes et retourne des résultats matérialisés ou des projections explicites.

---

# Writing Strategy

Une écriture suit généralement ce flux :

```text
Charger l’Aggregate
↓
Exécuter le comportement Domain
↓
Observer les modifications
↓
SaveChangesAsync
```

Infrastructure ne contourne pas les comportements Domain par des modifications directes arbitraires.

Un cas d’usage cohérent cherche à produire une sauvegarde logique unique.

Plusieurs appels à `SaveChangesAsync` doivent être justifiés.

---

# Transactions

Une transaction correspond à une opération devant rester atomique.

Exemples futurs :

* créer une `Organization` et son premier `OrganizationMember Owner` ;
* accepter une invitation et créer le Membership ;
* activer une `Campaign` et figer ses références ;
* soumettre une `Submission` et ses `Responses`.

Un appel unique à `SaveChangesAsync` utilise la transaction EF Core lorsqu’elle suffit.

Une transaction explicite est ajoutée lorsque plusieurs étapes doivent rester atomiques.

Le MVP n’utilise pas de transaction distribuée.

---

# Persistence Interfaces

SignalLab utilise des interfaces spécifiques aux capacités nécessaires.

Exemples acceptés :

```text
IProjectReader
IProjectWriter
IOrganizationReader
```

Pattern non retenu :

```text
IRepository<TEntity>
```

Un Generic Repository masquerait les besoins réels des cas d’usage et dupliquerait les capacités déjà fournies par EF Core.

---

# Migrations

Les évolutions du schéma utilisent les Migrations Entity Framework Core.

Chaque migration est :

* nommée explicitement ;
* versionnée ;
* revue ;
* reproductible ;
* testée ;
* applicable sur une base propre.

Exemples de noms futurs :

```text
InitialPlatformSchema
AddOrganizations
AddProjectStatus
AddCampaignBuildReference
```

Une migration déjà partagée ou appliquée dans un environnement commun ne doit pas être réécrite silencieusement.

Une nouvelle migration corrige l’état précédent.

---

# Analytical Data

Les données métier structurées sont persistées dans PostgreSQL.

Les résultats analytiques calculés ne sont pas persistés comme source de vérité.

Une `SavedAnalysis` conserve sa configuration.

La projection est recalculée à partir :

```text
SavedAnalysis Configuration
+
Persisted Research Data
+
Domain Metadata
+
AnalyticalEngineVersion
```

Les formules analytiques ne sont pas définies exclusivement en SQL.

---

# Binary Storage

Le MVP ne stocke pas dans PostgreSQL :

* artefacts binaires de Build ;
* pièces jointes ;
* exports ;
* archives de fichiers.

Un futur stockage objet constituerait une responsabilité distincte et nécessiterait une décision dédiée.

---

# Consequences

## Positive

### Modèle adapté aux relations métier

PostgreSQL correspond naturellement au modèle Organization, Project, Campaign, Submission et Analysis.

### Intégrité structurelle

Les contraintes relationnelles renforcent la cohérence des données.

### Transactions locales

Le Modular Monolith peut garantir des opérations atomiques sans infrastructure distribuée.

### Intégration .NET

Entity Framework Core s’intègre directement à ASP.NET Core, C# et la Dependency Injection.

### Migrations versionnées

L’évolution du schéma reste reproductible et auditable.

### Requêtes contrôlées

Les projections, filtres et paginations peuvent être optimisés progressivement.

## Negative

### Dépendance à un ORM

Infrastructure dépend des conventions et comportements d’Entity Framework Core.

### Risque de fuite technologique

Sans discipline, des types EF Core peuvent atteindre Application ou Domain.

Les tests d’architecture doivent empêcher cette dérive.

### Mappings supplémentaires

Le Domain et la Persistence peuvent nécessiter des configurations explicites.

### Migrations à maintenir

Chaque évolution structurelle exige une migration cohérente et testée.

### Risque de requêtes inefficaces

Les Includes excessifs, N+1 et projections trop larges doivent être surveillés.

---

# Alternatives Considered

## SQL manuel avec Npgsql

### Deferred

Cette option offre un contrôle direct sur les requêtes.

Elle n’est pas retenue comme stratégie principale car EF Core couvre les besoins CRUD, le suivi et les migrations.

Du SQL ciblé peut être introduit dans Infrastructure si une requête mesurée le nécessite, sans remplacer l’architecture générale.

## Dapper

### Not Selected

Dapper pourrait être utile pour des projections de lecture spécifiques.

Il n’est pas introduit par défaut afin d’éviter deux stratégies d’accès aux données avant qu’un besoin de performance soit démontré.

## Base NoSQL

### Rejected

Le modèle SignalLab possède de nombreuses relations, contraintes et transactions.

Une base NoSQL ne simplifierait pas le modèle principal.

## SQLite

### Rejected as Production Database

SQLite peut convenir à des outils locaux mais ne correspond pas à la base SaaS multi-tenant cible.

## EF Core InMemory

### Rejected as Integration Proof

Le provider InMemory ne reproduit pas le comportement relationnel de PostgreSQL.

Les tests de Persistence importants utiliseront PostgreSQL.

## Generic Repository

### Rejected

Il dupliquerait les capacités d’EF Core et produirait des interfaces trop génériques.

---

# Implementation Rules

* PostgreSQL constitue la base relationnelle principale.
* Entity Framework Core reste dans Infrastructure.
* Domain et Application ne référencent pas EF Core.
* Utiliser Fluent API pour le mapping.
* Prévoir une configuration distincte par entité.
* Ne pas exposer `IQueryable` hors Infrastructure.
* Utiliser `AsNoTracking` pour les lectures appropriées.
* Paginer avant matérialisation.
* Propager les `CancellationToken`.
* Utiliser `SaveChangesAsync`.
* Délimiter les transactions par cas d’usage.
* Ne pas utiliser de Generic Repository.
* Versionner et tester les migrations.
* Ne pas persister les résultats analytiques comme source de vérité.
* Toute autre technologie principale de Persistence nécessite une nouvelle ADR.

---

# References

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `004 - PersistenceConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `M - Milestones.md`
* `001 - ModularMonolith.md`
* `002 - CleanArchitectureVerticalSlices.md`
