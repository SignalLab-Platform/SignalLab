# Conventions Backend

**Projet :** SignalLab
**Document :** Conventions opérationnelles du Backend
**Version :** 1.0
**Statut :** Validé
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# 1. Objectif

Ce document définit les conventions applicables au Backend SignalLab.

Il précise :

* les responsabilités des projets ;
* la direction des dépendances ;
* l’organisation des modules ;
* la structure des Vertical Slices ;
* le nommage des cas d’usage ;
* l’organisation des Minimal APIs ;
* les frontières entre contrats ;
* les pratiques asynchrones ;
* les patterns volontairement exclus.

Ces conventions s’appliquent à toute nouvelle fonctionnalité Backend.

---

# 2. Projets Backend

La solution Backend contient quatre projets principaux :

```text
SignalLab.Api
SignalLab.Application
SignalLab.Domain
SignalLab.Infrastructure
```

## 2.1 SignalLab.Api

Responsabilités :

* démarrage ASP.NET Core ;
* Composition Root ;
* Minimal APIs ;
* Requests et Responses HTTP ;
* validation technique ;
* sérialisation ;
* Problem Details ;
* OpenAPI ;
* Middlewares.

Références autorisées :

```text
SignalLab.Application
SignalLab.Infrastructure
```

## 2.2 SignalLab.Application

Responsabilités :

* Commands ;
* Queries ;
* Handlers ;
* Results ;
* projections de lecture ;
* orchestration des cas d’usage ;
* interfaces de persistence ;
* interfaces des services externes ;
* autorisations applicatives.

Référence autorisée :

```text
SignalLab.Domain
```

## 2.3 SignalLab.Domain

Responsabilités :

* entités ;
* Value Objects ;
* invariants ;
* comportements métier ;
* transitions d’état ;
* erreurs métier ;
* Domain Events lorsque leur nécessité est démontrée.

Références autorisées vers les projets SignalLab :

```text
Aucune
```

Dépendances technologiques interdites :

* ASP.NET Core ;
* Entity Framework Core ;
* PostgreSQL ;
* Clerk ;
* SignalR ;
* framework de sérialisation HTTP ;
* code Frontend.

## 2.4 SignalLab.Infrastructure

Responsabilités :

* persistence ;
* Entity Framework Core ;
* `DbContext` ;
* migrations ;
* configurations Fluent API ;
* implémentations des interfaces Application ;
* intégrations externes ;
* services techniques.

Références autorisées :

```text
SignalLab.Application
SignalLab.Domain
```

---

# 3. Direction des dépendances

Matrice des dépendances autorisées :

| Projet source  |                     Domain | Application | Infrastructure | Api |
| -------------- | -------------------------: | ----------: | -------------: | --: |
| Domain         |                          — |         Non |            Non | Non |
| Application    |                        Oui |           — |            Non | Non |
| Infrastructure |                        Oui |         Oui |              — | Non |
| Api            | Non directe par convention |         Oui |            Oui |   — |

`SignalLab.Api` peut techniquement accéder aux types publics de Domain au travers des références transitives, mais ne doit pas utiliser directement les entités Domain comme contrats HTTP.

Toute nouvelle référence projet doit être justifiée par cette matrice.

---

# 4. Organisation par module

Les fonctionnalités sont organisées par module métier dans chaque couche.

Exemple cible :

```text
SignalLab.Domain/
└── Projects/
    ├── Project.cs
    ├── ProjectId.cs
    ├── ProjectStatus.cs
    └── ProjectErrors.cs

SignalLab.Application/
└── Projects/
    ├── CreateProject/
    ├── GetProject/
    └── ArchiveProject/

SignalLab.Api/
└── Endpoints/
    └── Projects/
        ├── CreateProject/
        ├── GetProject/
        └── ArchiveProject/

SignalLab.Infrastructure/
└── Persistence/
    └── Projects/
```

Les dossiers métier ne sont créés que par leur Spec propriétaire.

SL-013 définit la convention mais n’introduit aucun domaine métier.

---

# 5. Vertical Slices

Une Vertical Slice représente une intention métier ou un cas d’usage précis.

Exemples :

```text
CreateProject
GetProject
ArchiveProject
RestoreProject
ListOrganizationProjects
```

Une Slice ne représente pas une catégorie technique générique.

## 5.1 Command

Une Command modifie l’état du système.

Structure cible :

```text
Projects/
└── CreateProject/
    ├── CreateProjectCommand.cs
    ├── CreateProjectHandler.cs
    └── CreateProjectResult.cs
```

Responsabilités :

* `CreateProjectCommand` contient l’intention Application ;
* `CreateProjectHandler` orchestre le cas d’usage ;
* `CreateProjectResult` représente le résultat Application.

## 5.2 Query

Une Query lit ou projette l’état sans modifier le Domain.

Structure cible :

```text
Projects/
└── GetProject/
    ├── GetProjectQuery.cs
    ├── GetProjectHandler.cs
    └── ProjectDetails.cs
```

Responsabilités :

* `GetProjectQuery` contient les critères de lecture ;
* `GetProjectHandler` orchestre la lecture ;
* `ProjectDetails` représente la projection retournée.

## 5.3 Handler explicite

Les Handlers sont des classes ordinaires appelées explicitement.

SignalLab n’utilise pas MediatR.

Exemple de forme cible :

```csharp
public sealed class CreateProjectHandler
{
    public async Task<CreateProjectResult> HandleAsync(
        CreateProjectCommand command,
        CancellationToken cancellationToken)
    {
        // Orchestration du cas d’usage.
    }
}
```

Les dépendances du Handler sont injectées par constructeur.

Un Handler ne doit pas devenir un conteneur de règles métier qui appartiennent au Domain.

---

# 6. Minimal APIs

SignalLab utilise les Minimal APIs plutôt que les Controllers MVC.

Chaque opération HTTP possède un Endpoint dédié.

Structure cible :

```text
SignalLab.Api/
└── Endpoints/
    └── Projects/
        └── CreateProject/
            ├── CreateProjectEndpoint.cs
            ├── CreateProjectRequest.cs
            └── CreateProjectResponse.cs
```

Le fichier Endpoint :

* définit la méthode HTTP ;
* définit la route ;
* reçoit la Request ;
* applique la validation technique ;
* construit la Command ou Query ;
* appelle le Handler ;
* transforme le résultat en Response HTTP ;
* déclare les métadonnées OpenAPI ;
* ne contient aucune règle métier.

Exemple de convention :

```csharp
public static class CreateProjectEndpoint
{
    public static IEndpointRouteBuilder MapCreateProjectEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/projects", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateProjectRequest request,
        CreateProjectHandler handler,
        CancellationToken cancellationToken)
    {
        // Mapping, appel Application et réponse HTTP.
    }
}
```

Chaque module expose une méthode d’enregistrement regroupée :

```text
MapProjectEndpoints()
MapOrganizationEndpoints()
MapCampaignEndpoints()
```

`Program.cs` appelle ces méthodes sans contenir l’implémentation détaillée des Endpoints.

---

# 7. Contrats

## 7.1 Contrats HTTP

Les contrats HTTP appartiennent à `SignalLab.Api`.

Exemples :

```text
CreateProjectRequest
CreateProjectResponse
ProjectResponse
ProjectListResponse
```

Une Request décrit ce que le client peut transmettre.

Une Response décrit ce que l’API expose publiquement.

Les entités Domain ne sont jamais utilisées directement comme Request ou Response.

## 7.2 Contrats Application

Les contrats Application appartiennent à `SignalLab.Application`.

Exemples :

```text
CreateProjectCommand
GetProjectQuery
CreateProjectResult
ProjectDetails
ProjectSummary
```

Ils ne dépendent pas des concepts HTTP comme :

* codes de statut ;
* Headers ;
* Cookies ;
* `HttpContext` ;
* types ASP.NET Core.

## 7.3 Domain

Le Domain ne connaît ni les Requests HTTP ni les contrats TypeScript.

Une entité Domain n’est pas façonnée uniquement pour correspondre à un payload JSON ou à une table SQL.

## 7.4 Mapping

Les transformations entre frontières sont explicites :

```text
Request → Command ou Query
Result ou projection → Response
```

Le mapping simple reste écrit localement dans la Slice concernée.

AutoMapper n’est pas introduit sans besoin démontré et ADR.

---

# 8. Conventions de nommage

## 8.1 Cas d’usage

Les cas d’usage utilisent un verbe suivi de la ressource ou de l’intention :

```text
CreateProject
GetProject
ListProjects
UpdateProject
ArchiveProject
RestoreProject
ActivateCampaign
SubmitParticipation
```

## 8.2 Commands

```text
<CreateIntent>Command
<CreateIntent>Handler
<CreateIntent>Result
```

Exemples :

```text
CreateProjectCommand
CreateProjectHandler
CreateProjectResult
```

## 8.3 Queries

```text
<GetIntent>Query
<GetIntent>Handler
<Resource>Details
<Resource>Summary
```

Exemples :

```text
GetProjectQuery
GetProjectHandler
ProjectDetails
ProjectSummary
```

## 8.4 HTTP

```text
<Intent>Request
<Intent>Response
<Resource>Response
```

Exemples :

```text
CreateProjectRequest
CreateProjectResponse
ProjectResponse
```

## 8.5 Interfaces

Les interfaces C# utilisent le préfixe `I`.

Leur nom décrit une capacité précise :

```text
IProjectReader
IProjectWriter
ICurrentUser
IClock
```

Les noms trop génériques sont évités :

```text
IService
IManager
IHelper
IRepository<T>
```

## 8.6 Méthodes asynchrones

Les méthodes asynchrones portent le suffixe `Async`.

```text
HandleAsync
GetByIdAsync
SaveChangesAsync
```

---

# 9. Code transversal

Le code transversal doit porter un nom décrivant sa responsabilité.

Exemples acceptables :

```text
Common/Errors
Common/Results
Common/Time
Common/Pagination
```

Les dossiers suivants sont évités lorsqu’ils servent de fourre-tout :

```text
Helpers
Utils
Managers
Services
Models
Misc
```

`Common` ne doit pas devenir un module métier implicite ni un emplacement par défaut.

Un type utilisé par un seul module reste dans ce module.

---

# 10. Validation

La validation est séparée en deux niveaux.

## 10.1 Validation technique

Responsabilité de la Presentation :

* structure de la Request ;
* valeur absente ;
* format invalide ;
* valeur hors limite technique ;
* payload non désérialisable.

## 10.2 Validation métier

Responsabilité du Domain ou du cas d’usage :

* transition interdite ;
* invariant violé ;
* ressource incompatible ;
* état métier empêchant l’opération ;
* contrainte fonctionnelle.

Une validation Frontend ne remplace jamais ces validations Backend.

---

# 11. Erreurs

Les erreurs possèdent une signification explicite.

Catégories générales :

```text
Validation
NotFound
Conflict
Forbidden
Unauthorized
Unexpected
```

Le Domain et l’Application ne retournent pas directement des codes HTTP.

La Presentation transforme les erreurs Application en Problem Details selon les conventions API.

Les exceptions ne sont pas utilisées comme mécanisme ordinaire de contrôle de flux lorsqu’un résultat explicite est plus adapté.

Les erreurs inattendues peuvent remonter vers le gestionnaire global afin d’être journalisées et traduites en réponse sûre.

---

# 12. Asynchronisme et annulation

Toute opération I/O utilise une API asynchrone.

Un `CancellationToken` doit être propagé depuis l’Endpoint vers :

* le Handler ;
* les interfaces de persistence ;
* les appels externes ;
* les requêtes EF Core futures.

Exemple :

```csharp
public Task<ProjectDetails?> GetByIdAsync(
    ProjectId projectId,
    CancellationToken cancellationToken);
```

Un `CancellationToken` ne doit pas être remplacé silencieusement par `CancellationToken.None` dans une chaîne déjà annulable.

---

# 13. Dependency Injection

Les dépendances sont injectées par constructeur.

Les classes ne doivent pas résoudre elles-mêmes leurs dépendances depuis `IServiceProvider`.

Le Service Locator est interdit dans le code applicatif.

Les enregistrements sont regroupés par couche ou module :

```text
AddApplication()
AddInfrastructure()
MapProjectEndpoints()
```

`Program.cs` reste lisible et limité à la composition de l’application.

---

# 14. Persistence

Les interfaces nécessaires aux cas d’usage appartiennent à Application.

Leur granularité correspond aux besoins réels des modules.

SignalLab n’utilise pas de Generic Repository.

Exemple acceptable :

```text
IProjectReader
IProjectWriter
```

Exemple non retenu :

```text
IRepository<TEntity>
```

Les détails EF Core restent dans Infrastructure.

Les conventions détaillées sont définies dans :

```text
004 - PersistenceConventions.md
```

---

# 15. Patterns exclus par défaut

Les éléments suivants ne sont pas utilisés sans besoin démontré, Spec propriétaire et éventuellement ADR :

* MediatR ;
* AutoMapper ;
* Generic Repository ;
* Unit of Work générique ajoutée au-dessus d’EF Core ;
* Base Controller ;
* classe de service métier globale ;
* Service Locator ;
* Domain dépendant d’EF Core ;
* entité Domain exposée par l’API ;
* communication réseau entre modules internes ;
* accès direct du Frontend à la base de données ;
* règle métier uniquement dans un Endpoint.

---

# 16. Checklist d’une nouvelle Vertical Slice

Avant validation d’une Slice Backend :

* [ ] le module propriétaire est identifié ;
* [ ] le nom représente une intention métier précise ;
* [ ] la Request HTTP reste dans Api ;
* [ ] la Command ou Query reste dans Application ;
* [ ] les invariants restent dans Domain ;
* [ ] les détails techniques restent dans Infrastructure ;
* [ ] l’entité Domain n’est pas exposée ;
* [ ] le Handler ne dépend pas d’ASP.NET Core ;
* [ ] l’Endpoint ne contient pas de règle métier ;
* [ ] le `CancellationToken` est propagé ;
* [ ] les erreurs sont traduites en Problem Details ;
* [ ] OpenAPI décrit le contrat ;
* [ ] les tests pertinents sont ajoutés ;
* [ ] aucune dépendance interdite n’est introduite.

---

# 17. Références

* `000 - SolutionArchitecture.md`
* `003 - APIConventions.md`
* `004 - PersistenceConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `M - Milestones.md`
* ADR présentes dans `docs/architecture/adr/`
