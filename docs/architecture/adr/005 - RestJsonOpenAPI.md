# ADR 005 — REST, JSON et OpenAPI

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# Context

Le Frontend Next.js doit communiquer avec le Backend ASP.NET Core au travers d’une interface :

* standardisée ;
* sécurisée ;
* documentable ;
* testable ;
* adaptée à des ressources métier stables ;
* indépendante des entités Domain ;
* compatible avec plusieurs clients futurs ;
* simple à inspecter et diagnostiquer.

SignalLab manipule des ressources identifiables telles que :

```text
Organizations
Projects
Builds
Measures
Form Templates
Campaigns
Participations
Submissions
Results
SavedAnalysis
MAP Profiles
```

Les échanges doivent permettre :

* création ;
* lecture ;
* mise à jour ;
* transitions métier ;
* pagination ;
* filtrage ;
* erreurs structurées ;
* documentation des contrats.

Le choix doit également limiter la dépendance envers un fournisseur ou un protocole propriétaire.

---

# Decision

SignalLab utilise :

```text
HTTPS
REST
JSON
OpenAPI
```

Le Frontend communique avec le Backend exclusivement par HTTP sécurisé en HTTPS pour les opérations métier.

Les ressources sont exposées par des Endpoints REST.

Les Requests et Responses utilisent JSON.

Les contrats sont documentés avec OpenAPI.

Les entités Domain ne sont jamais sérialisées directement.

---

# REST

Les routes représentent des ressources relativement stables.

Exemples :

```http
GET /projects
GET /projects/{projectId}
POST /projects
PATCH /projects/{projectId}
DELETE /projects/{projectId}
```

Les actions métier qui ne correspondent pas naturellement à un CRUD utilisent un sous-segment verbal explicite :

```http
POST /campaigns/{campaignId}/activate
POST /organization-invitations/{invitationId}/accept
```

Les routes utilisent :

* ressources au pluriel ;
* `kebab-case` pour les segments composés ;
* paramètres portant les noms canoniques ;
* hiérarchies limitées ;
* méthodes HTTP cohérentes.

---

# JSON

Les échanges HTTP utilisent JSON.

Les propriétés utilisent le `camelCase`.

Exemple :

```json
{
  "projectId": "01H...",
  "name": "Usability Study",
  "status": "active"
}
```

Les identifiants sont transmis comme chaînes opaques.

Les instants utilisent ISO 8601 avec UTC ou offset explicite.

Les valeurs d’énumération textuelles constituent une partie stable du contrat.

---

# Contracts

Les contrats HTTP restent distincts du Domain et de l’Application.

```text
HTTP Request
↓
Application Command ou Query
↓
Domain
↓
Application Result ou projection
↓
HTTP Response
```

Exemples :

```text
CreateProjectRequest
CreateProjectCommand
CreateProjectResult
ProjectResponse
```

Les contrats HTTP :

* exposent uniquement les données nécessaires ;
* restent indépendants des tables ;
* ne reproduisent pas automatiquement une entité complète ;
* peuvent évoluer selon les besoins du client ;
* protègent les informations internes.

Les modèles TypeScript représentent ces contrats publics.

Ils ne constituent pas les entités Domain.

---

# OpenAPI

OpenAPI constitue la documentation contractuelle de l’API.

Chaque Endpoint doit documenter lorsque pertinent :

* méthode ;
* route ;
* résumé ;
* paramètres ;
* Request ;
* Response ;
* codes de statut ;
* Problem Details ;
* authentification ;
* autorisation ;
* pagination ;
* filtres.

La documentation OpenAPI doit correspondre au comportement réel.

Une modification d’Endpoint n’est pas terminée lorsque son contrat documenté est obsolète.

La génération automatique d’un client TypeScript n’est pas introduite pendant SL-013.

Elle peut être ajoutée ultérieurement par une décision dédiée si la maintenance manuelle devient coûteuse.

---

# Errors

Les erreurs HTTP utilisent Problem Details.

Exemple :

```json
{
  "type": "https://signallab.dev/problems/project-not-found",
  "title": "Project not found",
  "status": 404,
  "detail": "The requested Project does not exist.",
  "instance": "/projects/01H...",
  "code": "project.notFound"
}
```

Le champ `code` fournit un identifiant stable utilisable par le Frontend.

Le texte humain ne doit pas servir à déclencher une logique client.

Les informations sensibles ne sont jamais exposées.

---

# HTTP Status Codes

Conventions principales :

```text
200 OK
→ lecture ou opération réussie avec contenu

201 Created
→ création d’une ressource

204 No Content
→ réussite sans contenu utile

400 Bad Request
→ contrat techniquement invalide

401 Unauthorized
→ authentification absente ou invalide

403 Forbidden
→ identité valide mais opération interdite

404 Not Found
→ ressource absente ou masquée

409 Conflict
→ état actuel incompatible

500 Internal Server Error
→ erreur inattendue

503 Service Unavailable
→ service ou Health Check indisponible
```

L’ensemble des situations ne doit pas être traduit en `200 OK`.

---

# Pagination and Filtering

Les collections potentiellement importantes sont paginées.

Convention initiale :

```http
GET /projects?page=1&pageSize=25
```

Response cible :

```json
{
  "items": [],
  "page": 1,
  "pageSize": 25,
  "totalItems": 0,
  "totalPages": 0
}
```

Les filtres et tris utilisent les Query Parameters :

```http
GET /campaigns?status=active&sort=createdAt&direction=desc
```

Les champs filtrables et triables sont explicitement autorisés.

Le client ne peut pas transmettre une expression SQL ou un nom de colonne arbitraire.

---

# Security

HTTPS protège les échanges publics.

L’authentification future utilise les standards :

```text
OAuth 2.0
OpenID Connect
JWT
```

Le Backend valide les tokens et les Permissions.

Le Frontend ne constitue jamais la barrière de sécurité.

Les routes multi-tenant vérifient systématiquement l’accès à l’Organization concernée.

---

# Compatibility

Le MVP n’utilise pas de version explicite dans les routes.

Convention :

```text
/projects
```

et non :

```text
/api/v1/projects
```

Cette décision reste acceptable tant que :

* un seul client officiel existe ;
* Frontend et Backend évoluent dans le même repository ;
* les déploiements sont coordonnés ;
* aucune API publique n’est promise.

Un versioning formel devient nécessaire si :

* plusieurs clients évoluent indépendamment ;
* des intégrations externes utilisent l’API ;
* plusieurs versions doivent coexister ;
* une compatibilité longue durée est garantie.

Son introduction nécessite une nouvelle ADR.

---

# Consequences

## Positive

### Standards largement adoptés

HTTP, REST, JSON et OpenAPI sont compris par de nombreux outils et langages.

### Séparation claire

Le Frontend et le Backend communiquent par des contrats publics distincts du Domain.

### Débogage simple

Les requêtes sont inspectables avec des outils HTTP standards.

### Documentation générable

OpenAPI permet de documenter précisément les Endpoints et Responses.

### Tests directs

Les tests d’intégration peuvent envoyer des requêtes HTTP réelles et vérifier les contrats.

### Faible dépendance fournisseur

Le protocole reste indépendant de Next.js, ASP.NET Core, Clerk ou PostgreSQL.

### Compatibilité future

D’autres clients pourront consommer l’API sans accéder au code interne.

## Negative

### Mapping supplémentaire

Chaque opération peut nécessiter :

```text
Request
Command ou Query
Result
Response
Type TypeScript
```

Cette duplication explicite protège les frontières mais augmente le nombre de types.

### Over-fetching possible

Un contrat trop générique peut retourner plus de données que nécessaire.

Les Responses doivent rester adaptées aux parcours.

### Multiplication des Endpoints

Des besoins complexes peuvent nécessiter plusieurs Endpoints ou projections spécialisées.

### Synchronisation des contrats

Les types TypeScript doivent rester cohérents avec les contrats C# et OpenAPI.

### Absence de versioning initial

Les changements de contrat exigent une coordination stricte entre Frontend et Backend.

---

# Alternatives Considered

## GraphQL

### Rejected for MVP

GraphQL permettrait au client de sélectionner précisément les champs souhaités.

Il est rejeté car :

* les ressources et parcours sont compatibles avec REST ;
* il ajouterait un schéma et une chaîne d’outillage supplémentaires ;
* les autorisations et la performance de Queries complexes nécessiteraient davantage de discipline ;
* OpenAPI et Minimal APIs couvrent les besoins actuels ;
* aucun besoin de multiples clients hétérogènes n’est démontré.

## gRPC

### Rejected for Frontend Communication

gRPC est efficace pour la communication entre services.

Il est rejeté pour le navigateur principal car :

* REST/JSON est plus simple à inspecter ;
* le Frontend Web le consomme directement ;
* aucun service interne distribué n’existe ;
* la performance ne justifie pas ce protocole.

Il pourra être reconsidéré pour une communication interne future distincte du Frontend.

## Server Actions comme contrat principal

### Rejected

Les Server Actions coupleraient les opérations à Next.js et masqueraient la frontière publique ASP.NET Core.

Elles sont rejetées comme interface métier principale.

## Accès direct à PostgreSQL depuis le Frontend

### Rejected

Cette option contournerait :

* autorisations ;
* Application Layer ;
* Domain ;
* transactions ;
* audit ;
* contrats.

Elle est incompatible avec l’architecture autoritaire du Backend.

## API RPC générique

### Rejected

Une API fondée uniquement sur des routes comme :

```text
/execute
/run-command
/invoke
```

masquerait les ressources et réduirait la lisibilité des contrats.

Les actions métier restent possibles, mais elles sont attachées à une ressource claire.

## Message Bus comme interface principale

### Rejected for MVP

Une communication asynchrone par messages compliquerait les parcours interactifs et l’exploitation.

Elle ne remplace pas l’API REST synchrone du produit.

---

# Implementation Rules

* Utiliser HTTPS pour les communications publiques.
* Représenter les ressources avec des Endpoints REST.
* Utiliser JSON en `camelCase`.
* Utiliser les noms canoniques du Domain Model.
* Garder les contrats HTTP dans `SignalLab.Api`.
* Ne jamais exposer directement une entité Domain.
* Mapper explicitement Request, Application et Response.
* Utiliser Problem Details pour les erreurs.
* Fournir un code d’erreur stable.
* Paginer les collections potentiellement importantes.
* Autoriser explicitement les filtres et tris.
* Documenter tous les Endpoints métier dans OpenAPI.
* Propager les `CancellationToken`.
* Ne pas introduire de versioning explicite pendant le MVP.
* Ne pas utiliser GraphQL, gRPC ou Server Actions comme interface métier sans nouvelle ADR.
* Toute modification du style de communication nécessite une nouvelle ADR.

---

# References

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `002 - FrontendConventions.md`
* `003 - APIConventions.md`
* `06 - SoftwareArchitecture.md`
* `M - Milestones.md`
* `003 - NextJsFrontend.md`
* `004 - AspNetCoreBackend.md`
* `010 - MinimalAPIs.md`
