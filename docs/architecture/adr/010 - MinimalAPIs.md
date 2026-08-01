# ADR 010 — Minimal APIs

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# Context

La Presentation Layer de SignalLab doit exposer les opérations métier au travers d’une API ASP.NET Core.

ASP.NET Core permet notamment deux styles principaux :

```text
Controllers MVC
Minimal APIs
```

Les deux styles permettent :

* routing ;
* Dependency Injection ;
* authentification ;
* autorisation ;
* validation ;
* sérialisation ;
* OpenAPI ;
* Problem Details ;
* tests d’intégration.

SignalLab organise son Backend avec :

* Clean Architecture ;
* Modular Monolith ;
* Vertical Slices ;
* un cas d’usage par intention ;
* des contrats HTTP distincts ;
* un `Program.cs` limité au Composition Root.

Le style de Presentation doit renforcer cette organisation sans produire de Controllers regroupant progressivement de nombreuses responsabilités différentes.

---

# Decision

SignalLab utilise les **Minimal APIs ASP.NET Core** pour les nouveaux Endpoints HTTP.

Chaque opération possède un Endpoint dédié.

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

Un Endpoint définit :

* méthode HTTP ;
* route ;
* paramètres ;
* Request ;
* mapping vers Application ;
* appel du Handler ;
* mapping du résultat ;
* Response HTTP ;
* métadonnées OpenAPI ;
* authentification et autorisation.

Un Endpoint ne contient aucune règle métier.

---

# Presentation Structure

Structure cible :

```text
SignalLab.Api/
├── Endpoints/
│   ├── Platform/
│   │   └── Health/
│   ├── Organizations/
│   ├── Projects/
│   ├── Campaigns/
│   └── ...
├── Errors/
├── Middleware/
├── OpenApi/
├── DependencyInjection.cs
└── Program.cs
```

Les dossiers ne sont créés que lorsqu’une responsabilité réelle apparaît.

Les domaines métier ne sont pas créés par SL-013.

---

# Endpoint per Operation

Chaque intention HTTP possède son propre Endpoint.

Exemples :

```text
CreateProjectEndpoint
GetProjectEndpoint
ListProjectsEndpoint
UpdateProjectEndpoint
ArchiveProjectEndpoint
RestoreProjectEndpoint
ActivateCampaignEndpoint
SubmitParticipationEndpoint
```

Cette convention aligne la Presentation sur les Vertical Slices Application.

```text
CreateProjectEndpoint
↓
CreateProjectCommand
↓
CreateProjectHandler
↓
CreateProjectResult
```

Une ressource ne doit pas être regroupée automatiquement dans une classe contenant toutes ses opérations.

---

# Endpoint Registration

Chaque Endpoint expose une méthode de mapping explicite.

Exemple cible :

```csharp
public static class CreateProjectEndpoint
{
    public static IEndpointRouteBuilder MapCreateProjectEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/projects", HandleAsync)
            .WithName("CreateProject")
            .WithTags("Projects");

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        CreateProjectRequest request,
        CreateProjectHandler handler,
        CancellationToken cancellationToken)
    {
        // Mapping et exécution du cas d’usage.
    }
}
```

Les Endpoints d’un module sont regroupés par une méthode d’extension.

Exemple :

```csharp
public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapCreateProjectEndpoint();
        endpoints.MapGetProjectEndpoint();
        endpoints.MapListProjectsEndpoint();

        return endpoints;
    }
}
```

`Program.cs` appelle uniquement les groupes de haut niveau :

```csharp
app.MapPlatformEndpoints();
app.MapProjectEndpoints();
app.MapCampaignEndpoints();
```

---

# Program Composition Root

`Program.cs` reste le Composition Root de l’application.

Il est responsable de :

* construire le Host ;
* charger la configuration ;
* enregistrer les couches ;
* configurer les Middlewares ;
* mapper les groupes d’Endpoints ;
* démarrer l’application.

Il ne doit pas contenir :

* implémentation complète d’un Endpoint métier ;
* mapping complexe ;
* validation métier ;
* requête EF Core ;
* formule analytique ;
* autorisation métier détaillée.

La lisibilité de `Program.cs` doit permettre de comprendre rapidement la composition globale de l’application.

---

# Route Groups

Les Route Groups peuvent être utilisés pour mutualiser une configuration réellement commune.

Exemple conceptuel :

```csharp
var projects = endpoints
    .MapGroup("/projects")
    .WithTags("Projects")
    .RequireAuthorization();
```

Ils peuvent centraliser :

* préfixe de route ;
* tag OpenAPI ;
* authentification ;
* Policy commune ;
* filtres techniques communs.

Un groupe ne doit pas masquer les autorisations spécifiques à chaque opération.

La présence d’un Membership dans une Organization ou d’une Permission précise reste vérifiée par le cas d’usage approprié.

---

# Requests

Les Requests HTTP appartiennent à `SignalLab.Api`.

Exemple :

```csharp
public sealed record CreateProjectRequest(
    string Name);
```

Une Request :

* représente uniquement les données acceptées ;
* ne contient pas d’entité Domain ;
* ne contient pas de `DbContext` ;
* ne dépend pas de Persistence ;
* ne porte pas un objet complet uniquement parce qu’il existe en base ;
* reste spécifique à l’opération.

Les paramètres de route, Query Parameters et Headers restent explicitement identifiables.

---

# Responses

Les Responses HTTP appartiennent à `SignalLab.Api`.

Exemple :

```csharp
public sealed record ProjectResponse(
    string ProjectId,
    string Name,
    string Status);
```

Une Response :

* représente le contrat public ;
* ne sérialise pas directement l’entité Domain ;
* n’expose pas les colonnes internes ;
* ne dépend pas d’Entity Framework Core ;
* peut être adaptée au parcours utilisateur ;
* reste documentée dans OpenAPI.

---

# Application Mapping

L’Endpoint construit explicitement une Command ou Query Application.

Exemple conceptuel :

```csharp
var command = new CreateProjectCommand(
    currentOrganizationId,
    request.Name);
```

Le Handler retourne un Result ou une projection Application.

L’Endpoint transforme ensuite ce résultat en HTTP.

```text
Request
↓
Command ou Query
↓
Handler
↓
Result
↓
Response
```

AutoMapper n’est pas utilisé par défaut.

Les mappings simples restent visibles dans la Slice.

---

# Handler Resolution

Les Handlers sont injectés explicitement par la Dependency Injection.

Exemple :

```csharp
private static Task<IResult> HandleAsync(
    CreateProjectRequest request,
    CreateProjectHandler handler,
    CancellationToken cancellationToken)
```

SignalLab n’utilise pas MediatR.

L’Endpoint ne publie pas une Command vers un Dispatcher générique.

L’appel explicite rend la dépendance visible et limite l’infrastructure implicite.

---

# Validation

La validation technique peut être appliquée dans la Presentation ou au niveau de la frontière Application selon la nature de la règle.

Validation technique :

* champ absent ;
* format incorrect ;
* longueur maximale ;
* payload invalide ;
* paramètre non désérialisable.

Validation métier :

* transition interdite ;
* Permission absente ;
* ressource incompatible ;
* invariant violé ;
* conflit d’état.

Les Endpoints ne réimplémentent pas les invariants Domain.

Une validation technique commune pourra utiliser un Endpoint Filter ou un mécanisme dédié lorsqu’un besoin répétitif réel apparaît.

Aucune bibliothèque supplémentaire de validation n’est imposée par SL-013.

---

# Endpoint Filters

Les Endpoint Filters peuvent être utilisés pour des préoccupations HTTP transversales et limitées.

Exemples potentiels :

* validation technique uniforme ;
* métriques ;
* corrélation ;
* transformation technique ;
* contrôle d’un prérequis Presentation.

Ils ne doivent pas contenir :

* autorisation métier spécifique ;
* chargement d’Aggregate ;
* comportement Domain ;
* transaction ;
* logique analytique.

Un Filter ne doit pas créer un pipeline implicite difficile à suivre.

---

# Authentication

L’authentification peut être déclarée sur :

* un Route Group ;
* un Endpoint ;
* une Policy.

Exemple :

```csharp
endpoint.RequireAuthorization();
```

Cette déclaration vérifie l’identité authentifiée.

Elle ne remplace pas l’autorisation métier.

Un Endpoint protégé doit encore appeler un cas d’usage qui vérifie :

* Membership ;
* Permission ;
* Organization ;
* ownership de la ressource ;
* état métier.

---

# Authorization

Les Policies ASP.NET Core peuvent protéger des prérequis techniques ou génériques.

Les Permissions métier restent contrôlées par le Backend Application.

Un contrôle visuel côté Frontend ou une Policy de route ne remplace pas l’autorisation liée à la ressource réelle.

Exemple :

```text
JWT valide
↓
Endpoint accessible techniquement
↓
Handler résout Membership et Permission
↓
Opération autorisée ou refusée
```

---

# Error Mapping

Les Endpoints traduisent les résultats Application en réponses HTTP cohérentes.

Exemple conceptuel :

```text
Success
→ 200, 201 ou 204

Validation
→ 400 ou convention 422 validée

Unauthorized
→ 401

Forbidden
→ 403

NotFound
→ 404

Conflict
→ 409

Unexpected
→ gestionnaire global, 500
```

Les Problem Details constituent le contrat d’erreur.

Le Domain et Application ne retournent pas directement un `IResult`, un code HTTP ou un `ProblemDetails`.

---

# Typed Results

Les Typed Results peuvent être utilisés lorsque leur apport améliore :

* lisibilité ;
* typage ;
* documentation OpenAPI ;
* tests ;
* cohérence du contrat.

Exemple conceptuel :

```csharp
private static async Task<Results<
    Created<CreateProjectResponse>,
    BadRequest<ProblemDetails>,
    ForbiddenHttpResult,
    Conflict<ProblemDetails>>> HandleAsync(...)
```

Ils ne doivent pas produire des signatures illisibles pour des cas très complexes.

Une convention homogène devra être conservée entre les Endpoints comparables.

---

# OpenAPI

Chaque Endpoint métier déclare ses métadonnées OpenAPI.

Selon le besoin :

```csharp
.WithName("CreateProject")
.WithSummary("Create a Project")
.WithDescription("Creates a Project in the current Organization.")
.WithTags("Projects")
.Produces<CreateProjectResponse>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status403Forbidden)
.ProducesProblem(StatusCodes.Status409Conflict);
```

Le nom OpenAPI doit rester :

* unique ;
* stable ;
* lié au cas d’usage.

Le document OpenAPI doit refléter les Requests, Responses et codes réellement implémentés.

---

# CancellationToken

Chaque Endpoint asynchrone accepte le `CancellationToken` fourni par ASP.NET Core.

Il le transmet au Handler.

```text
ASP.NET Core RequestAborted
↓
Endpoint
↓
Application Handler
↓
Persistence ou service externe
```

Le token ne doit pas être remplacé par `CancellationToken.None` sans justification.

---

# Dependency Injection

Les dépendances nécessaires à l’Endpoint sont injectées comme paramètres ou par la forme recommandée par ASP.NET Core.

L’Endpoint ne résout pas manuellement ses services depuis `IServiceProvider`.

Le Service Locator est interdit.

Les enregistrements sont regroupés par couche :

```text
AddApplication()
AddInfrastructure()
```

Les Handlers sont enregistrés explicitement ou par une convention contrôlée et documentée.

---

# Endpoint Naming

Convention :

```text
<Intent>Endpoint
```

Exemples :

```text
CreateProjectEndpoint
GetProjectEndpoint
ListProjectsEndpoint
ArchiveProjectEndpoint
```

Méthodes de mapping :

```text
MapCreateProjectEndpoint
MapGetProjectEndpoint
MapProjectEndpoints
```

Nom OpenAPI :

```text
CreateProject
GetProject
ListProjects
ArchiveProject
```

Les noms doivent reprendre la nomenclature canonique du Domain Model.

---

# Route Naming

Les routes suivent `003 - APIConventions.md`.

Règles principales :

* ressources au pluriel ;
* segments en `kebab-case` ;
* paramètres en `camelCase` dans le code ;
* profondeur limitée ;
* actions métier explicites ;
* aucun nom de Controller dans la route.

Exemples :

```text
/projects
/projects/{projectId}
/projects/{projectId}/campaigns
/campaigns/{campaignId}/activate
/form-templates
/saved-analyses
```

---

# Tests

Les Endpoints sont principalement validés par les tests d’intégration API.

Ils vérifient :

* méthode HTTP ;
* route ;
* binding ;
* validation ;
* code de statut ;
* contrat JSON ;
* Problem Details ;
* authentification ;
* autorisation ;
* métadonnées importantes lorsque nécessaire.

Le test envoie une vraie requête HTTP au travers de :

```text
WebApplicationFactory<Program>
```

Les tests unitaires de l’Endpoint ne sont pas obligatoires lorsque le comportement est entièrement couvert par l’intégration et que l’Endpoint reste mince.

Les règles métier restent couvertes aux niveaux Domain et Application.

---

# Architecture Tests

Les tests d’architecture doivent protéger les frontières suivantes :

* `SignalLab.Api` ne devient pas la source des invariants métier ;
* Domain ne référence pas ASP.NET Core ;
* Application ne référence pas ASP.NET Core ;
* Domain ne référence pas les contrats HTTP ;
* les projets internes ne retournent pas de types Presentation.

Certaines règles de contenu nécessiteront des contrôles par conventions ou revues tant qu’elles ne sont pas automatiquement vérifiables simplement.

---

# Consequences

## Positive

### Alignement avec les Vertical Slices

Chaque Endpoint correspond directement à une intention Application.

### Faible cérémonie

Aucune hiérarchie de Controller ou Base Controller n’est nécessaire.

### Dépendances visibles

Le Handler et les paramètres apparaissent directement dans la signature.

### Composition explicite

Les méthodes `Map...Endpoints` rendent les capacités HTTP visibles.

### Program.cs réduit

Le Composition Root ne contient que les groupes de haut niveau.

### OpenAPI intégré

Les métadonnées sont déclarées près de l’opération.

### Tests d’intégration simples

Les Endpoints fonctionnent avec le pipeline ASP.NET Core standard.

### Évolution progressive

Endpoint Filters, Route Groups et Typed Results peuvent être ajoutés selon les besoins.

## Negative

### Nombre de fichiers

Une opération peut nécessiter :

```text
Endpoint
Request
Response
Command ou Query
Handler
Result
```

Cette granularité est acceptée afin de rendre les frontières explicites.

### Conventions moins imposées par le framework

Les Minimal APIs permettent plusieurs styles.

SignalLab doit donc documenter et respecter sa propre convention.

### Métadonnées répétitives

Les déclarations OpenAPI et réponses peuvent produire de la répétition.

Une abstraction ne sera ajoutée qu’après observation d’un pattern stable.

### Signatures potentiellement longues

Les Typed Results et dépendances peuvent rendre certaines signatures volumineuses.

### Risque de Program.cs monolithique

Sans méthodes de regroupement, tous les mappings pourraient s’accumuler dans `Program.cs`.

Cette dérive est explicitement interdite.

---

# Alternatives Considered

## Controllers MVC

### Rejected for New Endpoints

Les Controllers constituent une solution robuste et mature.

Ils sont rejetés comme convention principale car ils encouragent souvent le regroupement par ressource :

```text
ProjectsController
├── Create
├── Get
├── Update
├── Archive
└── Restore
```

Cette structure est moins alignée avec un fichier par Vertical Slice.

Les Controllers ne sont pas techniquement incompatibles avec SignalLab, mais mélanger les deux styles réduirait la cohérence du Backend.

## Controller par opération

### Not Selected

Un Controller par cas d’usage pourrait conserver l’alignement Vertical Slice.

Il ajouterait toutefois les conventions MVC et l’héritage `ControllerBase` sans valeur démontrée pour les Endpoints actuels.

## FastEndpoints

### Not Selected

Cette bibliothèque fournit une architecture Endpoint structurée.

Elle n’est pas retenue car les Minimal APIs natives couvrent les besoins sans dépendance supplémentaire.

## Carter

### Not Selected

Carter facilite l’organisation des modules Minimal API.

Il n’est pas introduit afin de conserver une solution native et explicite tant que les méthodes d’extension suffisent.

## MediatR Dispatch from Endpoints

### Rejected

Cette approche masquerait l’appel réel derrière un Dispatcher.

Les Handlers sont appelés explicitement.

## Endpoint Definitions in Program.cs

### Rejected

Cette approche convient à une démonstration minimale mais ne passe pas à l’échelle d’un produit organisé en Vertical Slices.

---

# Implementation Rules

* Utiliser les Minimal APIs pour les nouveaux Endpoints.
* Créer un Endpoint par opération.
* Regrouper les Endpoints par module.
* Conserver Requests et Responses dans `SignalLab.Api`.
* Appeler explicitement les Handlers Application.
* Ne pas utiliser MediatR.
* Ne pas placer de règle métier dans un Endpoint.
* Garder `Program.cs` comme Composition Root.
* Utiliser des méthodes `Map...Endpoints`.
* Utiliser des Route Groups uniquement pour une configuration réellement commune.
* Propager les `CancellationToken`.
* Utiliser Problem Details pour les erreurs.
* Déclarer les métadonnées OpenAPI.
* Ne pas injecter directement `DbContext` dans un Endpoint.
* Ne pas utiliser Service Locator.
* Ne pas mélanger Controllers et Minimal APIs sans nouvelle ADR.
* Toute modification de style Presentation nécessite une nouvelle décision architecturale.

---

# Supersession Criteria

Cette ADR devra être remplacée si SignalLab adopte comme convention principale :

* Controllers MVC ;
* une bibliothèque Endpoint externe ;
* GraphQL ;
* gRPC ;
* un autre framework HTTP ;
* un Dispatcher obligatoire entre Presentation et Application.

La nouvelle ADR devra justifier la valeur ajoutée et le coût de migration.

---

# References

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `003 - APIConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `M - Milestones.md`
* `002 - CleanArchitectureVerticalSlices.md`
* `004 - AspNetCoreBackend.md`
* `005 - RestJsonOpenAPI.md`
