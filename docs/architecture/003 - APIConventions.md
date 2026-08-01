# Conventions API

**Projet :** SignalLab
**Document :** Conventions opérationnelles de l’API
**Version :** 1.0
**Statut :** Validé
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# 1. Objectif

Ce document définit les conventions applicables à l’API SignalLab.

Il précise :

* le style REST ;
* la structure des routes ;
* les méthodes HTTP ;
* les contrats JSON ;
* les Requests et Responses ;
* les codes de statut ;
* les Problem Details ;
* la pagination ;
* le filtrage et le tri ;
* l’annulation ;
* OpenAPI ;
* les règles de compatibilité.

Ces conventions s’appliquent à tous les Endpoints métier futurs.

---

# 2. Principes

L’API SignalLab utilise :

```text
HTTPS
REST
JSON
OpenAPI
Minimal APIs ASP.NET Core
```

L’API constitue l’unique frontière publique des opérations métier du Backend.

Le Frontend ne communique pas directement avec :

* PostgreSQL ;
* Entity Framework Core ;
* le Domain ;
* les implémentations Infrastructure.

Toutes les modifications de données transitent par l’API.

---

# 3. Ressources

Les routes représentent des ressources métier relativement stables.

Exemples cibles :

```http
GET /projects
GET /projects/{projectId}
POST /projects
PATCH /projects/{projectId}
DELETE /projects/{projectId}
```

Les noms des ressources utilisent la nomenclature canonique du Domain Model.

Une route ne doit pas inventer un second nom pour un concept déjà défini.

---

# 4. Structure des routes

## 4.1 Ressources au pluriel

Les collections utilisent un nom pluriel :

```text
/organizations
/projects
/campaigns
/form-templates
/saved-analyses
```

## 4.2 Kebab-case

Les segments composés utilisent le `kebab-case` :

```text
/form-templates
/campaign-participations
/saved-analyses
```

Les paramètres de route utilisent le `camelCase` dans le code :

```text
{organizationId}
{projectId}
{campaignId}
```

## 4.3 Relations hiérarchiques

Une relation forte peut être représentée dans la route lorsqu’elle clarifie le contexte.

Exemples :

```text
/organizations/{organizationId}/projects
/projects/{projectId}/campaigns
/projects/{projectId}/saved-analyses
```

La profondeur doit rester limitée.

Une route ne doit pas reproduire toute la chaîne de navigation si l’identifiant de la ressource suffit à l’identifier.

Exemple évité :

```text
/organizations/{organizationId}/projects/{projectId}/campaigns/{campaignId}/forms/{formId}/questions/{questionId}
```

## 4.4 Actions métier

Une action métier qui ne correspond pas naturellement à un simple CRUD peut utiliser un sous-segment verbal explicite.

Exemples :

```text
POST /campaigns/{campaignId}/activate
POST /campaigns/{campaignId}/archive
POST /organization-invitations/{invitationId}/accept
POST /organization-invitations/{invitationId}/decline
```

Cette convention est réservée aux transitions ou intentions métier réelles.

Elle ne doit pas remplacer systématiquement les ressources REST.

---

# 5. Méthodes HTTP

## 5.1 GET

`GET` lit une ressource ou une collection.

Il ne modifie pas l’état métier.

Exemples :

```http
GET /projects
GET /projects/{projectId}
```

## 5.2 POST

`POST` crée une ressource ou déclenche une intention métier non idempotente.

Exemples :

```http
POST /projects
POST /campaigns/{campaignId}/activate
```

## 5.3 PUT

`PUT` remplace intégralement une représentation lorsque ce comportement est réellement défini.

Il n’est pas utilisé par défaut pour une mise à jour partielle.

## 5.4 PATCH

`PATCH` modifie partiellement une ressource.

Le contrat doit décrire précisément les champs modifiables.

Une entité Domain complète n’est jamais reçue comme payload de mise à jour.

## 5.5 DELETE

`DELETE` supprime ou retire une ressource lorsque le Domain autorise réellement cette opération.

Une opération métier d’archivage ne doit pas être arbitrairement présentée comme une suppression si les deux concepts sont distincts.

---

# 6. Contrats HTTP

Les contrats HTTP appartiennent à `SignalLab.Api`.

Ils sont distincts des contrats Application et des entités Domain.

```text
HTTP Request
    ↓
Application Command ou Query
    ↓
Application Result
    ↓
HTTP Response
```

## 6.1 Requests

Une Request représente uniquement les données acceptées depuis le client.

Exemples :

```text
CreateProjectRequest
UpdateProjectRequest
ActivateCampaignRequest
```

Une Request :

* ne contient pas d’entité Domain ;
* n’accepte pas de propriété interne non modifiable ;
* ne reprend pas automatiquement toutes les colonnes persistées ;
* porte uniquement les données nécessaires à l’opération.

## 6.2 Responses

Une Response représente le contrat exposé au client.

Exemples :

```text
ProjectResponse
ProjectSummaryResponse
CreateProjectResponse
CampaignDetailsResponse
```

Une Response :

* ne sérialise pas directement une entité Domain ;
* n’expose pas les données internes inutiles ;
* peut être adaptée aux besoins du parcours ;
* reste stable indépendamment de la structure des tables.

## 6.3 Nommage

Les noms décrivent le rôle du contrat.

Exemples préférés :

```text
ProjectResponse
ProjectSummary
ProjectDetails
PagedProjectResponse
```

Le suffixe `Dto` est évité lorsqu’un nom plus précis existe.

---

# 7. JSON

Les échanges utilisent JSON.

Les propriétés JSON utilisent le `camelCase`.

Exemple :

```json
{
  "projectId": "01H...",
  "name": "Usability Study",
  "status": "active"
}
```

Les noms de concepts suivent les termes canoniques du Domain Model.

## 7.1 Valeurs enum

Les valeurs d’énumération exposées utilisent une convention stable et documentée.

La convention retenue doit rester homogène dans l’ensemble de l’API.

Par défaut, les valeurs textuelles utilisent le `camelCase` :

```json
{
  "status": "inPreparation"
}
```

Une modification de valeur textuelle constitue une modification de contrat.

## 7.2 Dates et heures

Les dates et heures utilisent ISO 8601.

Les instants sont transmis avec un offset ou en UTC.

Exemple :

```json
{
  "createdAt": "2026-08-01T14:30:00Z"
}
```

Une date civile sans heure utilise un format de date explicite :

```json
{
  "birthDate": "1998-04-12"
}
```

## 7.3 Identifiants

Les identifiants sont sérialisés comme chaînes.

Leur format interne ne doit pas être interprété par le Frontend.

Le client doit les traiter comme des valeurs opaques.

## 7.4 Null

`null` représente une absence explicitement autorisée par le contrat.

Une propriété obligatoire ne doit pas utiliser `null` comme état métier implicite lorsque le Domain possède un concept plus précis.

---

# 8. Validation

La validation est divisée en deux responsabilités.

## 8.1 Validation technique

La Presentation vérifie notamment :

* présence d’un champ obligatoire ;
* type de donnée ;
* format ;
* taille maximale ;
* valeur désérialisable ;
* structure du payload ;
* paramètre de route invalide.

Une erreur de validation technique produit une réponse `400 Bad Request`.

## 8.2 Validation métier

Le Domain ou le cas d’usage vérifie notamment :

* invariant ;
* transition d’état ;
* compatibilité entre ressources ;
* autorisation métier ;
* conflit avec l’état courant.

Ces erreurs sont traduites en réponse HTTP par la Presentation.

Le Frontend peut effectuer une validation ergonomique, mais le Backend reste l’autorité.

---

# 9. Codes de statut

## 9.1 Succès

### `200 OK`

Utilisé pour :

* lecture réussie ;
* mise à jour réussie avec Response ;
* action réussie avec résultat.

### `201 Created`

Utilisé lorsqu’une ressource est créée.

La réponse devrait exposer la ressource ou son identifiant.

Lorsque pertinent, le Header `Location` pointe vers la nouvelle ressource.

### `202 Accepted`

Réservé à une opération acceptée mais non terminée.

Il ne doit pas être utilisé tant qu’aucun traitement asynchrone réel n’existe.

### `204 No Content`

Utilisé lorsqu’une opération réussit sans Response utile.

Exemples possibles :

* suppression ;
* transition ne nécessitant pas de payload ;
* mise à jour lorsque le client n’a pas besoin de représentation.

## 9.2 Erreurs client

### `400 Bad Request`

Request techniquement invalide.

### `401 Unauthorized`

Authentification absente ou invalide.

### `403 Forbidden`

Utilisateur authentifié mais non autorisé à réaliser l’opération.

### `404 Not Found`

Ressource absente ou volontairement non révélée selon la politique d’autorisation.

### `409 Conflict`

Opération incompatible avec l’état actuel.

Exemples :

* transition déjà effectuée ;
* ressource portant déjà une valeur unique ;
* conflit de concurrence explicite.

### `422 Unprocessable Content`

Peut être utilisé pour une requête techniquement valide mais rejetée par plusieurs validations applicatives structurées.

Son utilisation doit rester homogène et être décidée avant son introduction dans les Endpoints métier.

## 9.3 Erreurs serveur

### `500 Internal Server Error`

Erreur inattendue.

La réponse ne doit pas exposer :

* stack trace ;
* secret ;
* chaîne de connexion ;
* détail Infrastructure ;
* données sensibles.

### `503 Service Unavailable`

Service temporairement indisponible ou Health Check non sain.

---

# 10. Problem Details

Les erreurs HTTP utilisent le standard Problem Details.

Structure générale :

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

Les extensions peuvent notamment contenir :

```text
code
errors
traceId
```

## 10.1 Code stable

Le champ `code` identifie l’erreur de manière stable pour le Frontend.

Exemples :

```text
project.notFound
campaign.invalidState
organization.forbidden
request.validationFailed
```

Le Frontend ne doit pas dépendre du texte humain de `title` ou `detail` pour déterminer un comportement.

## 10.2 Erreurs de champs

Les erreurs de validation peuvent utiliser une extension `errors` :

```json
{
  "title": "Validation failed",
  "status": 400,
  "code": "request.validationFailed",
  "errors": {
    "name": [
      "Name is required."
    ]
  }
}
```

Les clés correspondent aux champs du contrat HTTP.

## 10.3 Informations sensibles

Les Problem Details ne doivent jamais exposer :

* exception interne ;
* nom de table ;
* requête SQL ;
* secret ;
* token ;
* détail d’autorisation sensible ;
* existence d’une ressource lorsque cela créerait une fuite de sécurité.

---

# 11. Collections

Toute collection potentiellement importante doit être paginée.

Une collection n’est considérée comme durablement petite que si son maximum est garanti par le Domain.

## 11.1 Paramètres de pagination

Convention cible :

```http
GET /projects?page=1&pageSize=25
```

Paramètres :

```text
page
pageSize
```

`page` commence à `1`.

`pageSize` possède :

* une valeur par défaut ;
* une valeur minimale ;
* une valeur maximale.

Ces valeurs sont définies lorsque la première collection paginée est implémentée.

## 11.2 Response paginée

Structure cible :

```json
{
  "items": [],
  "page": 1,
  "pageSize": 25,
  "totalItems": 0,
  "totalPages": 0
}
```

Le contrat générique peut être représenté côté Application ou Presentation selon le niveau de réutilisation démontré.

## 11.3 Cursor Pagination

La Cursor Pagination n’est pas utilisée par défaut.

Elle peut être introduite pour une ressource nécessitant réellement :

* très grands volumes ;
* flux chronologique ;
* stabilité face aux insertions ;
* performance supérieure à la pagination par page.

Son introduction doit être documentée.

---

# 12. Filtrage

Les filtres simples utilisent les Query Parameters.

Exemples :

```http
GET /projects?status=active
GET /campaigns?projectId=01H...
GET /campaigns?status=active&buildId=01H...
```

Les filtres :

* utilisent les noms canoniques ;
* sont documentés dans OpenAPI ;
* possèdent des valeurs finies lorsque le Domain les définit ;
* ne permettent pas d’exprimer arbitrairement du SQL ou des propriétés internes.

Une recherche complexe peut utiliser un Endpoint spécifique lorsque les Query Parameters deviennent illisibles ou insuffisants.

---

# 13. Tri

Convention cible :

```http
GET /projects?sort=createdAt&direction=desc
```

Paramètres :

```text
sort
direction
```

Les champs triables sont explicitement autorisés.

Le client ne peut pas transmettre arbitrairement le nom d’une colonne persistée.

Valeurs de direction :

```text
asc
desc
```

Un tri par défaut stable doit être défini pour toute collection paginée.

---

# 14. Recherche

Une recherche textuelle simple peut utiliser :

```http
GET /projects?search=usability
```

Le comportement exact doit être documenté :

* champs concernés ;
* sensibilité à la casse ;
* normalisation ;
* recherche partielle ou exacte.

Une recherche analytique ou métier complexe doit posséder un contrat dédié.

---

# 15. Concurrence

Les opérations sensibles aux modifications concurrentes devront utiliser une stratégie explicite.

Options futures possibles :

* token de concurrence ;
* version ;
* ETag ;
* comparaison de date de modification.

Aucune stratégie n’est imposée avant l’introduction de la persistence et d’un cas d’usage nécessitant ce contrôle.

Une perte silencieuse de mise à jour ne doit pas être acceptée lorsqu’un conflit utilisateur est plausible.

---

# 16. Idempotence

Les méthodes naturellement idempotentes doivent conserver ce comportement.

Exemples :

```text
GET
PUT
DELETE
```

Une Command métier déclenchée par `POST` peut nécessiter ultérieurement une clé d’idempotence si :

* elle est rejouable par le réseau ;
* elle déclenche un effet externe ;
* elle crée un risque de duplication.

Aucun mécanisme générique n’est introduit sans cas d’usage concret.

---

# 17. CancellationToken

Chaque Endpoint asynchrone accepte et propage un `CancellationToken`.

Chaîne attendue :

```text
Endpoint
↓
Handler
↓
Persistence ou service externe
```

Une annulation client doit interrompre les opérations I/O lorsqu’elles peuvent être annulées.

Le token ne doit pas être remplacé par `CancellationToken.None` sans justification.

---

# 18. Minimal APIs

Chaque opération possède un Endpoint dédié.

Exemple cible :

```text
Endpoints/
└── Projects/
    └── CreateProject/
        ├── CreateProjectEndpoint.cs
        ├── CreateProjectRequest.cs
        └── CreateProjectResponse.cs
```

L’Endpoint :

* définit la route ;
* définit la méthode HTTP ;
* reçoit les paramètres ;
* effectue le mapping ;
* appelle le Handler ;
* traduit le résultat ;
* déclare OpenAPI ;
* ne contient pas de règle métier.

Les Endpoints d’un module sont regroupés par une méthode d’extension :

```text
MapProjectEndpoints()
MapCampaignEndpoints()
```

---

# 19. OpenAPI

OpenAPI documente le contrat public de l’API.

Chaque Endpoint métier doit préciser lorsque nécessaire :

* résumé ;
* description ;
* groupe ou tag ;
* paramètres ;
* Request ;
* Responses ;
* codes de statut ;
* authentification requise ;
* Problem Details possibles.

Le contrat OpenAPI doit correspondre au comportement réel.

Une modification d’Endpoint n’est pas terminée tant que sa documentation n’est pas cohérente.

OpenAPI reste disponible en Development pendant la fondation.

Son exposition dans Staging ou Production sera décidée avec la configuration des environnements.

---

# 20. Compatibilité et versioning

Le MVP n’utilise pas de version explicite dans les routes.

Exemple retenu :

```text
/projects
```

Exemple non retenu pendant le MVP :

```text
/api/v1/projects
```

Les contrats peuvent évoluer tant qu’un seul client officiel est développé dans le même repository.

Les modifications restent néanmoins explicites et coordonnées entre Backend et Frontend.

Un versioning formel devient nécessaire si :

* plusieurs clients évoluent indépendamment ;
* une API publique est exposée ;
* des intégrations externes exigent une compatibilité durable ;
* plusieurs versions doivent coexister.

Son introduction nécessite une ADR.

---

# 21. Authentification et autorisation

Clerk assure l’authentification lorsqu’il est introduit par sa Spec dédiée.

Le Backend valide les tokens.

L’autorisation reste contrôlée côté Backend.

Un Endpoint protégé doit déclarer :

* authentification requise ;
* Permission ou Policy attendue ;
* réponses `401` et `403`.

Le masquage d’un contrôle dans le Frontend ne remplace jamais l’autorisation Backend.

---

# 22. Multi-tenancy

Les données appartenant à une Organization doivent toujours être isolées selon les règles métier.

Un identifiant transmis par le client ne suffit pas à prouver l’accès.

Chaque cas d’usage vérifie :

* l’identité courante ;
* le Membership ;
* le Role ou les Permissions ;
* l’appartenance de la ressource à l’Organization attendue.

Les détails de cette implémentation appartiennent aux Milestones Identity et Organization.

---

# 23. Health Endpoint

Le Health Endpoint technique reste distinct des ressources métier.

Convention actuelle :

```http
GET /health
```

Il retourne :

* `200 OK` lorsque l’application est saine ;
* `503 Service Unavailable` lorsqu’un Health Check futur échoue.

Les Health Checks futurs peuvent couvrir :

* PostgreSQL ;
* services externes strictement nécessaires ;
* dépendances critiques.

Ils ne doivent pas exposer de secrets ni de détails d’infrastructure sensibles.

---

# 24. Patterns exclus par défaut

Les éléments suivants ne sont pas introduits sans besoin démontré :

* GraphQL ;
* gRPC pour la communication Frontend ;
* API publique ;
* versioning de route ;
* entité Domain sérialisée directement ;
* payload reprenant une table complète ;
* route basée sur un nom de Controller ;
* retour systématique de `200 OK` pour toutes les situations ;
* erreur métier transmise uniquement sous forme de texte libre ;
* filtre permettant une expression SQL arbitraire ;
* Endpoint contenant la règle métier ;
* accès du Frontend à PostgreSQL ;
* génération automatique de clients avant décision dédiée.

---

# 25. Checklist d’un nouvel Endpoint

Avant validation :

* [ ] la ressource ou intention métier est identifiée ;
* [ ] la route utilise les noms canoniques ;
* [ ] les segments utilisent le `kebab-case` ;
* [ ] la méthode HTTP correspond au comportement ;
* [ ] la Request n’expose aucune entité Domain ;
* [ ] la Response est un contrat dédié ;
* [ ] la validation technique est appliquée ;
* [ ] la validation métier reste dans le Domain ou l’Application ;
* [ ] les codes de statut sont explicites ;
* [ ] les erreurs utilisent Problem Details ;
* [ ] le `CancellationToken` est propagé ;
* [ ] l’autorisation est contrôlée côté Backend ;
* [ ] les collections importantes sont paginées ;
* [ ] OpenAPI décrit le contrat ;
* [ ] les tests d’intégration pertinents sont ajoutés ;
* [ ] aucune donnée sensible n’est exposée.

---

# 26. Références

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `002 - FrontendConventions.md`
* `004 - PersistenceConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `M - Milestones.md`
