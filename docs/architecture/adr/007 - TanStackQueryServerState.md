# ADR 007 — TanStack Query Server State

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire de la décision :** SL-013 — Figer l’architecture de la solution
**Spec propriétaire de l’initialisation :** SL-012 — Initialiser le Frontend

---

# Context

Le Frontend SignalLab consomme des données dont la source de vérité se situe dans le Backend.

Ces données comprennent notamment :

```text
Organizations
Projects
Campaigns
Form Templates
Participations
Results
SavedAnalysis
MAP Profiles
Analytical Projections
```

Le Frontend doit gérer :

* requêtes HTTP ;
* cache ;
* fraîcheur ;
* déduplication ;
* invalidation après mutation ;
* chargement ;
* erreurs ;
* annulation ;
* conservation temporaire des projections ;
* synchronisation avec l’état serveur.

Ces responsabilités ne correspondent pas à du Client State ordinaire.

Les reconstruire manuellement dans des Contexts React ou un store global créerait une seconde gestion de cache difficile à maintenir.

---

# Decision

SignalLab utilise **TanStack Query** comme solution canonique de Server State côté client.

Le Server State désigne les données dont l’autorité appartient au Backend.

TanStack Query prend en charge :

* exécution des requêtes ;
* cache ;
* déduplication ;
* gestion de la fraîcheur ;
* invalidation ;
* mutations ;
* états de chargement ;
* erreurs ;
* refetch ;
* annulation lorsque la fonction de requête la supporte.

Le `QueryClientProvider` est configuré à la racine du Frontend.

---

# Server State and Client State

## Server State

Exemples :

```text
ProjectResponse
CampaignDetails
Results Projection
SavedAnalysis
Current Organization Membership
```

Ces données proviennent de l’API et peuvent devenir obsolètes.

Elles sont gérées par TanStack Query.

## Client State

Exemples :

```text
Dialog ouvert
Élément sélectionné
Panneau replié
Valeur temporaire de formulaire
État de survol
```

Ces données appartiennent à l’interface.

Elles restent locales autant que possible.

TanStack Query ne doit pas servir de store générique de Client State.

---

# Query Keys

Les Query Keys sont :

* stables ;
* structurées ;
* liées aux ressources ;
* définies par la Feature propriétaire.

Exemples :

```typescript
["projects"]
["projects", projectId]
["projects", projectId, "campaigns"]
["campaigns", campaignId]
["saved-analyses", savedAnalysisId]
```

Lorsque leur nombre augmente, une Query Key Factory est créée dans la Feature.

Les Query Keys ne doivent pas dépendre de valeurs instables recréées sans contrôle.

---

# Mutations and Invalidation

Une mutation invalide ou met à jour uniquement les données affectées.

Exemple conceptuel :

```text
ArchiveProject Mutation
↓
Invalidate Project Details
↓
Invalidate Project Lists
```

L’invalidation globale du cache n’est pas utilisée comme solution par défaut.

La réponse de mutation peut être utilisée pour actualiser directement le cache lorsque le contrat garantit la nouvelle représentation.

---

# Optimistic Updates

Les Optimistic Updates ne sont pas activés systématiquement.

Ils sont utilisés uniquement lorsque :

* la valeur UX est importante ;
* le rollback est fiable ;
* le comportement est simple ;
* les conflits métier sont limités ;
* le Backend reste l’autorité finale.

Une transition métier complexe ne doit pas être considérée comme réussie uniquement parce que le Frontend l’a anticipée.

---

# Analytical Projections

Les projections analytiques restent du Server State.

TanStack Query peut gérer :

* annulation ;
* déduplication ;
* fraîcheur ;
* invalidation après une nouvelle `Submission` ;
* invalidation après une modification de partage MAP ;
* conservation temporaire pendant un changement de vue.

Le Frontend ne reconstruit pas les résultats analytiques comme source de vérité locale.

---

# Error Management

Les fonctions de requête transforment les réponses HTTP invalides en erreurs exploitables.

Les Problem Details restent le contrat principal des erreurs Backend.

Le Frontend peut utiliser le `code` stable pour déterminer un comportement.

Il ne doit pas dépendre du texte libre de `title` ou `detail`.

Les erreurs doivent produire un état visible dans l’interface plutôt qu’une simple écriture en console.

---

# Query Client Configuration

La configuration globale reste minimale.

Les options sont ajoutées uniquement lorsqu’un besoin transversal est démontré.

Exemples d’options pouvant être définies ultérieurement :

```text
staleTime
gcTime
retry
refetchOnWindowFocus
```

Une configuration globale ne doit pas masquer les besoins particuliers d’une Feature.

Les tests utilisent un `QueryClient` isolé et désactivent les retries lorsque nécessaire.

---

# Next.js Relationship

Next.js et TanStack Query possèdent des responsabilités différentes.

```text
Next.js
→ routing, layouts, rendu et composition

TanStack Query
→ Server State interactif côté client
```

TanStack Query ne remplace pas App Router.

Next.js ne remplace pas le cache métier interactif de TanStack Query.

Les Server Components restent utilisés par défaut lorsqu’aucune interaction client ne nécessite TanStack Query.

---

# Realtime Evolution

Le MVP repose sur HTTP, TanStack Query et des rafraîchissements raisonnables.

Si SignalR est introduit ultérieurement :

```text
SignalR Event
↓
Informer qu’un état a changé
↓
TanStack Query invalide ou refetch
↓
API REST retourne la projection canonique
```

SignalR ne deviendra pas la source de vérité et ne remplacera pas TanStack Query ou REST.

---

# Consequences

## Positive

### Séparation claire des états

Le Server State et le Client State possèdent des responsabilités distinctes.

### Cache standardisé

Les Features ne réimplémentent pas chacune un système de cache.

### Déduplication

Les appels identiques peuvent être regroupés.

### Invalidation explicite

Les mutations déclarent les données devenues obsolètes.

### Gestion uniforme

Chargement, erreur, succès et refetch suivent les mêmes conventions.

### Compatible avec Next.js

TanStack Query complète App Router sans remplacer ses responsabilités.

### Préparation au temps réel

Une future notification SignalR peut déclencher une invalidation ciblée.

## Negative

### Cache supplémentaire

Les développeurs doivent comprendre la relation entre :

* rendu Next.js ;
* cache Next.js ;
* cache TanStack Query ;
* source Backend.

### Query Keys à maintenir

Des clés incohérentes produiraient des données difficiles à invalider.

### Mauvaises invalidations possibles

Une mutation peut laisser des vues obsolètes si les Queries affectées sont mal identifiées.

### Frontière client nécessaire

Les Hooks TanStack Query nécessitent des Client Components.

Cette frontière doit rester locale.

### Risque d’abus

TanStack Query pourrait être utilisé à tort comme store global de Client State.

Les conventions doivent empêcher cette dérive.

---

# Alternatives Considered

## `fetch` manuel avec `useEffect`

### Rejected

Cette approche nécessiterait de reconstruire :

* cache ;
* déduplication ;
* annulation ;
* invalidation ;
* gestion des retries ;
* synchronisation.

Elle produirait des implémentations différentes selon les Features.

## React Context pour les données serveur

### Rejected

Context est adapté à certaines dépendances transversales, pas à la gestion complète de données distantes et potentiellement obsolètes.

## Redux Toolkit Query

### Not Selected

RTK Query offre des capacités comparables.

Il imposerait toutefois Redux comme store global alors qu’aucun besoin général de Redux n’est démontré.

## Zustand

### Rejected for Server State

Zustand convient au Client State partagé, mais ne fournit pas nativement le modèle complet de cache serveur retenu.

## Cache Next.js uniquement

### Rejected as Sole Client Strategy

Le cache Next.js ne couvre pas à lui seul les mutations interactives, invalidations et états client nécessaires aux Workspaces riches.

## Apollo Client

### Rejected

Apollo Client est principalement associé à GraphQL, qui n’est pas retenu pour le MVP.

---

# Implementation Rules

* Utiliser TanStack Query pour le Server State côté client.
* Conserver le Client State local autant que possible.
* Ne pas utiliser TanStack Query comme store générique d’interface.
* Définir des Query Keys stables.
* Regrouper les Query Keys complexes par Feature.
* Invalider uniquement les Queries affectées.
* Ne pas activer les Optimistic Updates par défaut.
* Utiliser les Problem Details pour les erreurs Backend.
* Isoler le `QueryClient` dans les tests.
* Limiter les Client Components au besoin réel.
* Ne pas dupliquer le cache serveur dans React Context.
* Une autre solution canonique de Server State nécessite une nouvelle ADR.

---

# References

* `000 - SolutionArchitecture.md`
* `002 - FrontendConventions.md`
* `003 - APIConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `M - Milestones.md`
* `003 - NextJsFrontend.md`
* `009 - DeferredRealtimeDistributedInfrastructure.md`
