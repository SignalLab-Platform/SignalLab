# SignalLab — Entity Framework Core

## 1. Objectif

Entity Framework Core constitue la couche de mapping relationnel entre le modèle C# de SignalLab et PostgreSQL.

Il appartient exclusivement à la couche `SignalLab.Infrastructure`.

Le Domain ne dépend jamais :

- d'Entity Framework Core ;
- de Npgsql ;
- de PostgreSQL ;
- des migrations ;
- du `DbContext`.

Les règles métier restent implémentées dans le Domain. Entity Framework Core est uniquement responsable de la persistence technique.

---

# 2. Stack

SignalLab utilise :

- Entity Framework Core 10 ;
- Npgsql comme provider PostgreSQL ;
- PostgreSQL comme base relationnelle principale ;
- `dotnet-ef` comme outil local au repository.

Versions initiales de SL-017 :

- `Microsoft.EntityFrameworkCore.Design` : `10.0.12` ;
- `Microsoft.EntityFrameworkCore.Relational` : `10.0.12` ;
- `Npgsql.EntityFrameworkCore.PostgreSQL` : `10.0.3` ;
- `dotnet-ef` : `10.0.12`.

`dotnet-ef` est enregistré dans le tool manifest du repository afin que sa version soit reproductible entre les postes de développement.

Restaurer les outils :

```powershell
dotnet tool restore
```

---

# 3. DbContext

Le contexte principal est :

```text
SignalLab.Infrastructure/Persistence/SignalLabDbContext.cs
```

`SignalLabDbContext` représente l'unité de travail EF Core avec la base de données.

Il est enregistré avec `AddDbContext`, ce qui lui donne par défaut un lifetime `Scoped`, adapté à une utilisation par requête HTTP.

Le DbContext ne contient aucune configuration de connexion en dur.

La chaîne utilisée est :

```text
ConnectionStrings:Postgres
```

Elle est fournie depuis la configuration extérieure de l'application.

---

# 4. Composition Infrastructure

La configuration de la persistence est enregistrée depuis :

```text
SignalLab.Infrastructure/DependencyInjection.cs
```

L'API appelle :

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

Infrastructure configure ensuite :

```text
SignalLabDbContext
↓
Npgsql
↓
PostgreSQL
```

La chaîne de connexion PostgreSQL est obligatoire.

La validation avancée des environnements et variables de configuration appartient à SL-018.

---

# 5. Configurations Fluent API

Les mappings Entity Framework Core sont définis hors des entités Domain.

Chaque entité persistée dispose d'une configuration dédiée lorsque son mapping est introduit :

```text
Persistence/
├── SignalLabDbContext.cs
├── Configurations/
│   ├── UserConfiguration.cs
│   ├── OrganizationConfiguration.cs
│   ├── ProjectConfiguration.cs
│   └── ...
└── Migrations/
```

Une configuration implémente :

```csharp
IEntityTypeConfiguration<TEntity>
```

Convention de nommage :

```text
<EntityName>Configuration
```

Exemples :

```text
User
→ UserConfiguration

Organization
→ OrganizationConfiguration
```

`SignalLabDbContext` découvre automatiquement ces configurations avec :

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(SignalLabDbContext).Assembly);
```

Aucune configuration ne doit être enregistrée manuellement dans `OnModelCreating`.

Les Value Objects ou types owned peuvent être configurés depuis la configuration de leur entité propriétaire lorsqu'une configuration distincte n'apporte pas de valeur.

---

# 6. Conventions de mapping

Les décisions structurelles importantes doivent être explicites dans les configurations Fluent API.

## Primary Keys

Les clés primaires sont configurées explicitement pour les entités persistées.

Exemple :

```csharp
builder.HasKey(entity => entity.Id);
```

## Foreign Keys

Les relations et Foreign Keys importantes sont configurées explicitement.

Le comportement de suppression doit être choisi consciemment pour chaque relation.

Une suppression en cascade ne doit jamais être introduite uniquement parce qu'Entity Framework Core la propose par convention.

## Propriétés obligatoires

Les propriétés structurellement obligatoires sont configurées avec :

```csharp
.IsRequired()
```

PostgreSQL doit garantir les contraintes `NOT NULL` correspondantes.

## Longueurs

Une longueur maximale est définie lorsqu'une limite existe réellement dans le domaine ou dans le contrat technique.

Les limites arbitraires ne doivent pas être ajoutées uniquement pour satisfaire la persistence.

## Unicité

Les contraintes d'unicité structurelles sont exprimées à travers des index uniques :

```csharp
builder
    .HasIndex(...)
    .IsUnique();
```

La contrainte PostgreSQL complète la validation métier mais ne la remplace pas.

## Index

Un index est ajouté lorsqu'il répond :

- à une contrainte structurelle ;
- à un accès identifié ;
- ou à un besoin de performance mesuré.

Les index spéculatifs ne sont pas ajoutés par anticipation.

## Règles métier

Les configurations EF Core ne contiennent jamais de règles métier.

Le Domain décide ce qui est valide.

PostgreSQL protège l'intégrité structurelle des données persistées.

---

# 7. Migrations

Les migrations sont stockées dans :

```text
SignalLab.Infrastructure/Persistence/Migrations
```

Elles constituent l'historique versionné du schéma PostgreSQL.

Chaque évolution du modèle persistant doit produire une migration dédiée, revue et testée.

La première migration est :

```text
20260929133350_InitialPersistence
```

Elle est volontairement vide car aucune entité métier n'est encore implémentée à la fin de M01.

Elle valide néanmoins tout le pipeline :

```text
dotnet-ef
↓
SignalLab.Api
↓
AddInfrastructure
↓
SignalLabDbContext
↓
Npgsql
↓
PostgreSQL
↓
__EFMigrationsHistory
```

---

# 8. Créer une migration

Depuis la racine du repository :

```powershell
dotnet ef migrations add <MigrationName> `
  --project apps\api\src\SignalLab.Infrastructure\SignalLab.Infrastructure.csproj `
  --startup-project apps\api\src\SignalLab.Api\SignalLab.Api.csproj `
  --context SignalLabDbContext `
  --output-dir Persistence\Migrations
```

Avant de générer une migration, la chaîne `ConnectionStrings:Postgres` doit être disponible dans la configuration du processus.

Les fichiers de migration générés par EF Core ne doivent pas être renommés manuellement.

---

# 9. Lister les migrations

```powershell
dotnet ef migrations list `
  --project apps\api\src\SignalLab.Infrastructure\SignalLab.Infrastructure.csproj `
  --startup-project apps\api\src\SignalLab.Api\SignalLab.Api.csproj `
  --context SignalLabDbContext
```

---

# 10. Appliquer les migrations

```powershell
dotnet ef database update `
  --project apps\api\src\SignalLab.Infrastructure\SignalLab.Infrastructure.csproj `
  --startup-project apps\api\src\SignalLab.Api\SignalLab.Api.csproj `
  --context SignalLabDbContext
```

Cette opération est idempotente : une migration déjà appliquée n'est pas rejouée.

EF Core conserve l'état des migrations dans :

```text
__EFMigrationsHistory
```

---

# 11. Supprimer la dernière migration non appliquée

Lorsqu'une migration vient d'être générée mais n'a pas encore été appliquée :

```powershell
dotnet ef migrations remove `
  --project apps\api\src\SignalLab.Infrastructure\SignalLab.Infrastructure.csproj `
  --startup-project apps\api\src\SignalLab.Api\SignalLab.Api.csproj `
  --context SignalLabDbContext
```

Une migration déjà partagée ou utilisée par un environnement ne doit pas être réécrite pour masquer une évolution de schéma.

Une nouvelle migration doit alors exprimer l'évolution suivante.

---

# 12. EnsureCreated

`Database.EnsureCreated()` n'est pas utilisé pour créer le schéma SignalLab.

Le schéma évolue exclusivement au moyen des migrations EF Core.

Cela garantit :

- un historique explicite ;
- des évolutions reproductibles ;
- des changements revus ;
- une cohérence entre les environnements.

---

# 13. Tests d'architecture

Les tests d'architecture garantissent notamment que :

- Domain ne dépend d'aucune technologie de persistence ;
- Application ne dépend ni d'Entity Framework Core ni de Npgsql ;
- Infrastructure possède les dépendances EF Core/PostgreSQL attendues ;
- deux configurations ne peuvent pas cibler indépendamment la même entité ;
- les configurations respectent la convention `<EntityName>Configuration`.

Ces tests protègent la séparation entre modèle métier et persistence au fil de l'évolution de SignalLab.

---

# 14. État à la fin de SL-017

À la fin de SL-017 :

- PostgreSQL reste la source de vérité persistante ;
- `SignalLabDbContext` est opérationnel ;
- Npgsql connecte EF Core à PostgreSQL ;
- la configuration est externalisée ;
- les mappings futurs sont préparés via Fluent API ;
- les conventions de persistence sont définies ;
- les migrations sont versionnées dans Infrastructure ;
- `InitialPersistence` est appliquée ;
- aucune table métier n'est encore créée ;
- `__EFMigrationsHistory` constitue la seule table technique initiale ;
- les tests d'architecture protègent les frontières de persistence.