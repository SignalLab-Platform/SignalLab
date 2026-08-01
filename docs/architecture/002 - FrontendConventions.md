# Conventions Frontend

**Projet :** SignalLab
**Document :** Conventions opérationnelles du Frontend
**Version :** 1.0
**Statut :** Validé
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# 1. Objectif

Ce document définit les conventions applicables au Frontend SignalLab.

Il précise :

* les responsabilités du Frontend ;
* l’organisation des routes et des fonctionnalités ;
* la séparation entre Server State et Client State ;
* l’utilisation de Next.js App Router ;
* l’organisation des composants ;
* les règles relatives aux contrats API ;
* les conventions TypeScript ;
* les règles de styling et de typographie ;
* les patterns volontairement exclus.

Ces conventions s’appliquent à toute nouvelle fonctionnalité Frontend.

---

# 2. Responsabilité du Frontend

Le Frontend constitue l’interface utilisateur de SignalLab.

Il est responsable de :

* représenter l’état fourni par le Backend ;
* recueillir les intentions utilisateur ;
* déclencher les appels API ;
* gérer les états de chargement et d’erreur ;
* composer les routes, layouts et Workspaces ;
* gérer les interactions purement visuelles ;
* améliorer l’expérience utilisateur par des validations locales ;
* maintenir les états temporaires propres à l’interface.

Le Frontend n’est pas l’autorité métier.

Toute règle métier importante doit être validée par le Backend.

Une restriction affichée dans l’interface ne constitue jamais une barrière de sécurité.

---

# 3. Stack Frontend

Le Frontend canonique utilise :

```text
React
Next.js App Router
TypeScript
TanStack Query
Tailwind CSS
shadcn/ui
Jest
React Testing Library
```

Le routing dépend exclusivement de Next.js App Router.

SignalLab n’utilise pas :

* Vite comme framework d’application ;
* TanStack Router ;
* React Router ;
* Next.js comme Backend métier ;
* un store global supplémentaire sans besoin démontré.

---

# 4. Structure générale

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

Tous les dossiers ne sont créés que lorsqu’un besoin réel apparaît.

## 4.1 `app`

Le dossier `app` appartient à Next.js App Router.

Il contient :

* les routes ;
* les layouts ;
* les pages ;
* les fichiers `loading` ;
* les fichiers `error` ;
* les métadonnées ;
* la composition de haut niveau.

Une route compose les fonctionnalités nécessaires, mais ne doit pas concentrer tout leur code métier et visuel.

Exemple cible :

```text
app/
└── projects/
    └── [projectId]/
        ├── layout.tsx
        ├── page.tsx
        ├── loading.tsx
        └── error.tsx
```

## 4.2 `features`

Le dossier `features` contient les capacités liées à un domaine ou à un cas d’usage.

Exemple cible :

```text
features/
└── projects/
    ├── api/
    ├── components/
    ├── hooks/
    ├── types/
    └── utils/
```

Une Feature ne doit contenir que le code lié à sa responsabilité.

Un module métier n’est pas créé avant sa Spec propriétaire.

SL-013 définit uniquement la convention.

## 4.3 `components/ui`

Le dossier `components/ui` contient les primitives d’interface génériques.

Exemples :

```text
Button
Dialog
Input
Select
Tooltip
Tabs
Popover
```

Ces composants :

* ne connaissent aucun domaine métier ;
* ne déclenchent pas directement de cas d’usage ;
* peuvent être adaptés à l’identité visuelle SignalLab ;
* servent de base réutilisable aux composants de Feature.

Les composants shadcn/ui sont intégrés au code source et restent modifiables.

## 4.4 `providers`

Le dossier `providers` contient les providers globaux nécessaires à l’application.

Exemples futurs :

```text
QueryProvider
AuthenticationProvider
ThemeProvider
```

Un provider n’est ajouté que lorsqu’une capacité transversale le justifie.

## 4.5 `lib`

Le dossier `lib` contient l’infrastructure Frontend transversale.

Exemples acceptables :

```text
lib/api
lib/errors
lib/query
lib/formatting
```

Le dossier `lib` ne doit pas devenir un emplacement générique pour du code sans propriétaire.

## 4.6 `hooks`

Le dossier racine `hooks` est réservé aux Hooks véritablement transversaux.

Un Hook utilisé par une seule Feature reste dans cette Feature.

## 4.7 `types`

Le dossier racine `types` est réservé aux types transversaux ne possédant pas de meilleur propriétaire.

Les contrats propres à une Feature restent dans sa Feature.

---

# 5. App Router

Next.js App Router constitue l’unique système de routing.

Les conventions Next.js sont privilégiées :

```text
layout.tsx
page.tsx
loading.tsx
error.tsx
not-found.tsx
route.ts
```

Une URL doit représenter un état navigable et restaurable lorsque cet état appartient à l’architecture de navigation.

La définition détaillée de Context Navigation, Local Navigation et des modèles Explorer, Workspace et Document appartient à SL-015.

## 5.1 Layouts

Les layouts représentent des structures persistantes entre plusieurs routes.

Ils ne doivent pas être utilisés pour stocker implicitement un état métier non restaurable.

L’Application Shell permanent appartient à SL-014.

## 5.2 Paramètres de route

Les identifiants de ressources utilisent des segments dynamiques explicites :

```text
/projects/[projectId]
/projects/[projectId]/campaigns/[campaignId]
```

Les noms des paramètres utilisent les noms canoniques du Domain Model.

## 5.3 Search Params

Les Search Params sont utilisés pour les états partageables et restaurables qui ne définissent pas une ressource distincte.

Ils peuvent notamment représenter :

* filtre ;
* tri ;
* pagination ;
* tab sélectionnée lorsque cette tab est navigable ;
* configuration légère d’une vue.

Un état purement éphémère ne doit pas automatiquement être placé dans l’URL.

---

# 6. Server Components et Client Components

Les Server Components sont utilisés par défaut lorsque le composant ne nécessite pas :

* d’interaction navigateur ;
* de Hook React client ;
* d’accès à une API navigateur ;
* de Client State ;
* de TanStack Query côté client.

La directive `"use client"` est ajoutée uniquement à la frontière qui nécessite réellement un Client Component.

Elle ne doit pas être placée systématiquement sur une page ou un layout entier.

## 6.1 Frontière client

Une frontière client doit rester aussi locale que possible.

Exemple :

```text
Server Page
└── ProjectHeader
    └── ArchiveProjectButton client
```

Le composant interactif peut être client sans transformer toute la page en Client Component.

## 6.2 Données métier

Même lorsqu’un Server Component charge des données, la source canonique reste l’API ASP.NET Core.

Le Frontend Next.js ne devient pas une seconde Application Layer.

---

# 7. Server State

TanStack Query gère le Server State côté client.

Le Server State représente les données dont la source de vérité se situe sur le Backend.

Exemples :

```text
Organization courante
Project
Campaign
Form Template
Results
SavedAnalysis
```

TanStack Query est responsable de :

* l’exécution des requêtes ;
* la mise en cache ;
* la déduplication ;
* la gestion de la fraîcheur ;
* l’invalidation ;
* les états de chargement ;
* les erreurs de requête ;
* les mutations.

## 7.1 Query Keys

Les Query Keys sont stables, structurées et liées aux ressources.

Exemples cibles :

```typescript
["projects"]
["projects", projectId]
["projects", projectId, "campaigns"]
["campaigns", campaignId]
```

Les Query Keys ne doivent pas dépendre d’objets recréés de manière instable.

Lorsque leur complexité augmente, elles sont centralisées dans une Factory propre à la Feature.

## 7.2 Invalidation

Une mutation invalide uniquement les Queries affectées.

Une invalidation globale ne doit pas remplacer l’identification des données réellement modifiées.

## 7.3 Optimistic Updates

Les Optimistic Updates ne sont utilisés que lorsque :

* la valeur UX est réelle ;
* le rollback est fiable ;
* le comportement métier est suffisamment simple ;
* l’API reste l’autorité finale.

Ils ne sont pas introduits par défaut.

---

# 8. Client State

Le Client State représente l’état propre à l’interface.

Exemples :

* Dialog ouvert ;
* champ temporaire ;
* élément sélectionné ;
* état de survol ;
* panneau replié ;
* étape locale d’un formulaire ;
* état non persisté d’un Workspace.

Le Client State reste local autant que possible.

Ordre de préférence :

```text
State local du composant
↓
State partagé par composition
↓
Context React ciblé
↓
Store global uniquement si un besoin démontré le justifie
```

TanStack Query ne doit pas être utilisé pour stocker du Client State.

---

# 9. Contrats API

Les types TypeScript représentent les contrats HTTP consommés par le Frontend.

Ils ne constituent pas le Domain Model.

Exemples :

```text
ProjectResponse
ProjectSummary
CreateProjectRequest
CreateProjectResponse
ProblemDetails
PagedResponse<ProjectSummary>
```

Les entités conceptuelles définies dans le Domain Model ne doivent pas être reproduites aveuglément comme modèles Client indépendants de l’API.

Une modification de contrat doit être reflétée explicitement dans :

* le type TypeScript ;
* le client API ;
* les composants consommateurs ;
* les tests associés.

La génération automatique de clients n’est pas introduite pendant SL-013.

Son ajout futur nécessite une décision dédiée.

---

# 10. Client API

Les appels HTTP sont centralisés dans une infrastructure ou une Feature clairement propriétaire.

Exemple cible :

```text
features/
└── projects/
    └── api/
        ├── get-project.ts
        ├── create-project.ts
        └── project-query-keys.ts
```

Une fonction API :

* construit la route ;
* sérialise les paramètres ;
* exécute la requête ;
* vérifie la réponse HTTP ;
* désérialise le contrat ;
* transforme les erreurs techniques en erreurs exploitables par l’interface.

Les composants React ne doivent pas répéter directement des appels `fetch` complexes.

## 10.1 Base URL

La base URL de l’API appartient à la configuration d’environnement.

Elle ne doit pas être écrite en dur dans les composants.

La validation de cette configuration appartient à SL-018.

## 10.2 Authentification

L’intégration Clerk appartient à sa Spec dédiée.

Les composants métier ne doivent pas manipuler directement des tokens.

La couche d’accès API reçoit les informations d’authentification par le mécanisme officiel retenu.

---

# 11. Composants

## 11.1 Composants UI

Les composants UI sont génériques et indépendants des domaines.

Exemples :

```text
Button
DataTable
EmptyState
ConfirmationDialog
```

## 11.2 Composants de Feature

Les composants de Feature expriment un concept ou un cas d’usage.

Exemples :

```text
ProjectCard
CampaignStatusBadge
ArchiveProjectDialog
FormTemplateEditor
```

Ils peuvent utiliser les primitives de `components/ui`.

## 11.3 Pages

Les pages composent les composants de Feature et les primitives de navigation.

Une page ne doit pas devenir une collection de logique réseau, transformation, validation et rendu dans un seul fichier.

## 11.4 Props

Les Props utilisent un type explicitement nommé lorsque leur structure n’est pas triviale.

Exemple :

```typescript
type ProjectCardProps = Readonly<{
  project: ProjectSummary;
  isSelected?: boolean;
}>;
```

Les Props sont considérées comme immuables.

---

# 12. Conventions TypeScript

Le mode strict de TypeScript reste actif.

Règles :

* éviter `any` ;
* préférer `unknown` lorsqu’un type n’est pas encore vérifié ;
* utiliser des Unions pour les états finis ;
* éviter les assertions de type non justifiées ;
* conserver les types près de leur propriétaire ;
* préférer les types explicites aux structures anonymes répétées ;
* ne pas dupliquer les valeurs canoniques sans raison.

Exemple :

```typescript
type RequestState =
  | { status: "idle" }
  | { status: "loading" }
  | { status: "success"; data: ProjectResponse }
  | { status: "error"; error: Error };
```

Les noms de concepts métier suivent la nomenclature canonique du Domain Model.

---

# 13. Nommage

## 13.1 Fichiers

Les composants React utilisent `PascalCase` ou la convention générée par l’outil lorsque celle-ci est déjà établie dans le dossier.

Les fichiers de route Next.js conservent les noms réservés en minuscules :

```text
page.tsx
layout.tsx
loading.tsx
error.tsx
```

Les fonctions techniques peuvent utiliser des noms en `kebab-case` :

```text
get-project.ts
create-project.ts
project-query-keys.ts
```

Une même catégorie de fichiers doit suivre une convention homogène.

## 13.2 Composants

```text
ProjectCard
CreateProjectDialog
CampaignResultsHeader
```

## 13.3 Hooks

Les Hooks commencent par `use` :

```text
useProject
useCreateProject
useCurrentOrganization
```

## 13.4 Fonctions API

Les fonctions portent le nom de l’opération :

```text
getProject
createProject
archiveProject
```

---

# 14. Styling

Tailwind CSS constitue l’outil principal de styling.

Les classes utilitaires sont écrites directement dans les composants lorsque leur portée reste locale et lisible.

Un composant réutilisable est créé lorsqu’un ensemble de styles et de comportements se répète.

Les fichiers CSS spécifiques sont réservés aux besoins qui ne sont pas raisonnablement exprimables avec les conventions existantes.

## 14.1 Design Tokens

Les couleurs, rayons, espacements et styles typographiques récurrents doivent utiliser des Design Tokens.

Les valeurs arbitraires répétées sont évitées.

shadcn/ui fournit une base technique de tokens, mais ne définit pas l’identité visuelle finale de SignalLab.

## 14.2 Variants

Les variants de composants réutilisables peuvent utiliser `class-variance-authority`.

Les variants doivent représenter une intention visuelle ou fonctionnelle explicite.

Exemples :

```text
default
secondary
destructive
ghost
outline
```

Un variant ne doit pas être ajouté pour contourner un composant mal défini.

---

# 15. Typographie

Geist est la police fonctionnelle de SignalLab.

Elle est utilisée pour :

* le texte courant ;
* la navigation ;
* les formulaires ;
* les tableaux ;
* les analyses ;
* les contrôles d’interface ;
* les contenus nécessitant une forte lisibilité.

Faune, dessinée par Alice Savoie, est réservée à l’identité visuelle forte.

Son usage futur peut notamment concerner :

* le wordmark ;
* certains grands titres ;
* les accroches éditoriales ;
* des espaces de présentation.

Faune ne doit pas remplacer Geist dans les interfaces denses.

Son intégration, ses fichiers, ses tokens et ses mentions de licence appartiennent à la Spec mettant en place l’identité visuelle correspondante.

---

# 16. Accessibilité

Les composants doivent conserver les comportements accessibles fournis par les primitives utilisées.

Règles générales :

* utiliser les éléments HTML sémantiques ;
* associer les labels aux contrôles ;
* préserver la navigation clavier ;
* rendre le focus visible ;
* fournir un nom accessible aux boutons iconiques ;
* ne pas transmettre une information uniquement par la couleur ;
* utiliser les Dialogs et Popovers selon leurs primitives accessibles ;
* tester les états disabled et loading.

Un `div` interactif ne doit pas remplacer un `button` ou un lien sans justification.

---

# 17. Gestion des erreurs

Les erreurs réseau et métier doivent produire un état d’interface explicite.

Une erreur ne doit pas être uniquement écrite dans la console.

Les états possibles incluent :

* erreur locale au champ ;
* erreur de formulaire ;
* message dans le contexte de la ressource ;
* page d’erreur ;
* notification globale lorsque cela est pertinent.

Les Problem Details retournés par l’API sont interprétés sans exposer d’information technique sensible.

Le texte affiché à l’utilisateur doit rester utile et compréhensible.

---

# 18. États de chargement

Une opération asynchrone doit fournir un feedback approprié.

Selon le contexte :

* Skeleton ;
* Spinner ;
* bouton loading ;
* état vide temporaire ;
* progression déterminée lorsqu’elle est disponible.

Les composants doivent éviter les changements visuels inutiles lorsque des données précédentes restent exploitables.

TanStack Query peut conserver des données précédentes lorsque cela améliore réellement la continuité de navigation.

---

# 19. Formulaires

Les formulaires distinguent :

* validation ergonomique côté Frontend ;
* validation technique côté API ;
* validation métier côté Backend.

Le Frontend peut détecter immédiatement :

* champ requis ;
* format évident ;
* longueur ;
* incohérence locale.

Il ne doit pas considérer cette validation comme définitive.

Les erreurs retournées par le Backend doivent pouvoir être réinjectées dans le formulaire ou affichées dans son contexte.

Aucune bibliothèque de formulaire supplémentaire n’est imposée par SL-013.

Son ajout doit répondre à un besoin réel.

---

# 20. Tests

Les composants sont testés selon la valeur de leur comportement.

Les tests Frontend utilisent principalement :

```text
Jest
React Testing Library
```

Les tests doivent privilégier les interactions et résultats observables.

Exemple :

```text
Render
↓
Interaction utilisateur
↓
Résultat visible ou appel attendu
```

Les tests ne doivent pas dépendre inutilement de la structure interne des composants.

La stratégie complète est définie dans :

```text
005 - TestingStrategy.md
```

---

# 21. Patterns exclus par défaut

Les éléments suivants ne sont pas utilisés sans besoin démontré :

* Redux ou store global équivalent ;
* TanStack Router ;
* React Router ;
* Vite comme framework d’application ;
* CSS-in-JS supplémentaire ;
* client GraphQL ;
* logique métier garantie uniquement dans React ;
* accès direct à PostgreSQL ;
* gestion manuelle des tokens dans les composants ;
* duplication locale non contrôlée du Server State ;
* appel API complexe répété directement dans plusieurs composants ;
* transformation d’une page entière en Client Component sans nécessité.

---

# 22. Checklist d’une nouvelle Feature

Avant validation d’une Feature Frontend :

* [ ] le domaine ou cas d’usage propriétaire est identifié ;
* [ ] la route reste dans `app` ;
* [ ] le code fonctionnel reste dans `features` lorsque nécessaire ;
* [ ] les composants UI génériques ne connaissent aucun domaine ;
* [ ] le Server State utilise TanStack Query ;
* [ ] le Client State reste local autant que possible ;
* [ ] aucune règle métier importante n’est garantie uniquement côté client ;
* [ ] les contrats correspondent à l’API ;
* [ ] les erreurs sont affichées ;
* [ ] les états de chargement sont gérés ;
* [ ] les composants interactifs sont accessibles ;
* [ ] les Client Components sont limités au besoin réel ;
* [ ] les tests pertinents sont ajoutés ;
* [ ] lint, typecheck, tests et build passent.

---

# 23. Références

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `003 - APIConventions.md`
* `005 - TestingStrategy.md`
* `05 - UXArchitecture.md`
* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `04 - ApplicationBlueprint.md`
* `M - Milestones.md`
