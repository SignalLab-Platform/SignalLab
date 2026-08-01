# ADR 003 — Next.js Frontend

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# Context

SignalLab nécessite une application Frontend capable de supporter :

* un Application Shell permanent ;
* une navigation hiérarchique ;
* des layouts persistants ;
* des routes restaurables par URL ;
* des modèles Explorer, Workspace et Document ;
* des interfaces riches de construction et d’analyse ;
* des interactions client complexes ;
* l’intégration future de Clerk ;
* la séparation entre Server State et Client State ;
* une évolution progressive vers le rendu serveur lorsque cela apporte une valeur réelle.

Le Frontend doit rester un client de l’API ASP.NET Core.

Il ne doit pas devenir une seconde autorité métier ni introduire une Application Layer concurrente.

La solution doit également éviter de combiner plusieurs systèmes de routing.

---

# Decision

SignalLab utilise **React avec Next.js App Router** comme framework Frontend canonique.

La stack Frontend comprend :

```text
React
Next.js App Router
TypeScript
TanStack Query
Tailwind CSS
shadcn/ui
```

Next.js est utilisé comme framework d’application pour :

* le routing ;
* les layouts ;
* l’organisation des routes ;
* les métadonnées ;
* les Server Components lorsque pertinents ;
* les Client Components interactifs ;
* les états `loading`, `error` et `not-found` ;
* l’intégration future avec Clerk ;
* les optimisations de build et de rendu.

Next.js n’est pas utilisé comme Backend métier.

Toutes les règles métier et modifications de données restent contrôlées par l’API ASP.NET Core.

Le routing appartient exclusivement à Next.js App Router.

SignalLab n’utilise pas :

* Vite comme framework d’application ;
* TanStack Router ;
* React Router ;
* Pages Router pour les nouvelles routes ;
* Server Actions comme remplacement de l’Application Layer Backend ;
* accès direct du Frontend à PostgreSQL.

---

# Architecture Boundaries

Le flux canonique reste :

```text
Navigateur
↓
Next.js
↓ HTTPS / REST / JSON
API ASP.NET Core
↓
Application
↓
Domain
```

Les modèles TypeScript représentent les contrats HTTP consommés par le Frontend.

Ils ne constituent pas le Domain Model.

Les validations côté Frontend améliorent l’expérience utilisateur mais ne garantissent jamais seules une règle métier ou une autorisation.

---

# App Router

Les routes utilisent les conventions App Router :

```text
app/
├── layout.tsx
├── page.tsx
├── loading.tsx
├── error.tsx
├── not-found.tsx
└── [resourceId]/
```

Les layouts représentent des structures persistantes entre plusieurs routes.

Les pages composent les Features et Workspaces sans concentrer toute leur logique.

Les paramètres dynamiques utilisent la nomenclature canonique :

```text
[organizationId]
[projectId]
[campaignId]
[savedAnalysisId]
```

Les Search Params représentent les états partageables et restaurables qui ne justifient pas une route distincte.

---

# Server Components

Les Server Components sont utilisés par défaut lorsqu’un composant ne nécessite pas :

* de Hook client ;
* d’interaction navigateur ;
* de Client State ;
* d’API navigateur ;
* de TanStack Query côté client.

La directive `"use client"` est placée à la frontière interactive la plus locale possible.

Une page complète ne doit pas devenir un Client Component uniquement parce qu’un contrôle interactif y est présent.

Exemple :

```text
Server Page
└── CampaignHeader
    └── ArchiveCampaignButton client
```

Les Server Components peuvent composer et préparer une vue, mais ils n’implémentent pas de règle métier.

---

# Client Components

Les Client Components sont utilisés pour :

* formulaires interactifs ;
* Dialogs ;
* menus ;
* sélection ;
* drag and drop ;
* visualisations interactives ;
* éditeurs ;
* état local ;
* interactions nécessitant les APIs navigateur.

Ils continuent de consommer l’API ASP.NET Core pour toute donnée métier.

---

# Server State

TanStack Query constitue la solution canonique de Server State côté client.

Il prend en charge :

* cache ;
* déduplication ;
* fraîcheur ;
* invalidation ;
* mutations ;
* erreurs ;
* refetch.

Le Client State reste local autant que possible.

Next.js et TanStack Query possèdent des responsabilités complémentaires :

```text
Next.js
→ routes, layouts, rendu et composition

TanStack Query
→ Server State interactif côté client
```

Aucun des deux ne remplace l’API Backend.

---

# Project Structure

Structure cible :

```text
src/
├── app/
├── components/
│   └── ui/
├── features/
├── hooks/
├── lib/
├── providers/
└── types/
```

Responsabilités :

* `app` porte les routes et layouts ;
* `features` porte les capacités liées aux cas d’usage ;
* `components/ui` porte les primitives génériques ;
* `providers` porte les providers globaux ;
* `lib` porte l’infrastructure Frontend transversale précisément nommée ;
* `hooks` et `types` racines sont réservés aux usages réellement transversaux.

Les domaines métier ne sont créés que par leur Spec propriétaire.

---

# Styling and Components

Tailwind CSS constitue l’outil principal de styling.

shadcn/ui fournit les primitives d’interface intégrées directement au code source.

Ces outils constituent une base technique et non l’identité visuelle définitive du produit.

Geist constitue la police fonctionnelle de SignalLab.

Faune est réservée à l’identité visuelle forte et sera intégrée par la Spec qui en devient propriétaire.

---

# Consequences

## Positive

### Routing unifié

Un seul framework contrôle :

* routes ;
* layouts ;
* paramètres ;
* navigation ;
* métadonnées ;
* erreurs de route.

### Application Shell naturel

Les layouts persistants correspondent au besoin d’un Header, d’une Sidebar et d’un Content permanents.

### Navigation restaurable

Les routes et Search Params permettent de reconstruire un contexte à partir d’une URL.

### Intégration React complète

Les interfaces riches restent développées avec React et son écosystème.

### Rendu progressif

SignalLab peut utiliser Server Components ou rendu client selon les besoins réels.

### Intégration Clerk

Next.js dispose d’une intégration adaptée au fournisseur d’identité retenu.

### Structure conventionnelle

L’App Router impose une organisation connue et documentée.

## Negative

### Complexité Server/Client

Les développeurs doivent comprendre :

* Server Components ;
* Client Components ;
* frontières `"use client"` ;
* hydratation ;
* accès aux APIs navigateur.

### Dépendance au framework

Les routes et layouts utilisent les conventions propres à Next.js.

Un remplacement demanderait une réorganisation significative du Frontend.

### Cache à clarifier

Next.js et TanStack Query disposent chacun de mécanismes de cache.

Leur responsabilité doit rester explicite pour éviter :

* données obsolètes ;
* invalidations incohérentes ;
* duplication ;
* comportements difficiles à prévoir.

### Risque de logique Backend dans Next.js

Les Route Handlers ou Server Actions pourraient encourager l’ajout de règles métier dans le Frontend.

Cette utilisation est interdite pour les opérations métier SignalLab.

---

# Alternatives Considered

## React avec Vite et TanStack Router

### Rejected

Cette option fournirait une Single-Page Application légère et un routing fortement typé.

Elle est rejetée car :

* la stack validée retient Next.js ;
* elle ajouterait un système de routing distinct ;
* elle ne fournit pas nativement les layouts et conventions App Router ;
* elle rendrait l’intégration future de Clerk et du rendu serveur moins directe ;
* elle contredirait SL-012.

## React avec Vite et React Router

### Rejected

Cette option est mature et flexible.

Elle est rejetée pour les mêmes raisons et parce qu’elle imposerait davantage de conventions propres au projet pour organiser les routes et layouts.

## Next.js Pages Router

### Rejected for New Development

Le Pages Router reste supporté par Next.js mais ne correspond pas à l’architecture retenue.

App Router est préféré pour :

* les layouts imbriqués ;
* les Server Components ;
* les conventions modernes ;
* la structure cible du Shell.

## Next.js Full-Stack comme Backend métier

### Rejected

Cette option réduirait le nombre apparent de composants déployés.

Elle est rejetée car :

* ASP.NET Core constitue l’autorité métier ;
* le Domain et l’Application Layer sont développés en C# ;
* elle dupliquerait les responsabilités ;
* elle créerait deux chemins possibles pour modifier les données ;
* elle affaiblirait la Clean Architecture.

## Application Desktop native

### Rejected

Le produit doit être accessible depuis un navigateur et déployable comme plateforme Web.

Une application native augmenterait les coûts de distribution et de compatibilité sans bénéfice MVP démontré.

---

# Implementation Rules

* Utiliser Next.js App Router.
* Ne pas utiliser Vite comme framework d’application.
* Ne pas installer TanStack Router ou React Router.
* Utiliser les Server Components par défaut.
* Limiter `"use client"` à la frontière interactive nécessaire.
* Conserver les routes et layouts dans `src/app`.
* Conserver les capacités fonctionnelles dans `src/features`.
* Ne jamais accéder directement à PostgreSQL depuis Next.js.
* Ne pas implémenter de règle métier dans une Server Action ou un Route Handler.
* Faire transiter toutes les opérations métier par l’API ASP.NET Core.
* Utiliser TanStack Query pour le Server State côté client.
* Conserver le Client State local autant que possible.
* Représenter les contrats HTTP par des types TypeScript distincts.
* Toute modification de framework ou de système de routing nécessite une nouvelle ADR.

---

# References

* `000 - SolutionArchitecture.md`
* `002 - FrontendConventions.md`
* `003 - APIConventions.md`
* `06 - SoftwareArchitecture.md`
* `M - Milestones.md`
* `004 - AspNetCoreBackend.md`
* `007 - TanStackQueryServerState.md`
