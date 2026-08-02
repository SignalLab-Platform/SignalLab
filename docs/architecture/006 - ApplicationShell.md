# Application Shell

**Version :** 1.0
**Statut :** Implémenté
**Spec propriétaire :** SL-014 — Construire l’Application Shell

---

# 1. Objectif

L’Application Shell définit la structure permanente de l’interface SignalLab.

Il fournit les régions racines dans lesquelles toutes les futures fonctionnalités seront affichées, sans introduire de domaine métier ni d’architecture de navigation.

Le Shell est composé de trois régions permanentes :

* `Header`
* `Sidebar`
* `Content`

Les fonctionnalités futures remplissent ces régions mais ne modifient jamais leur responsabilité structurelle.

---

# 2. Structure générale

```text
ApplicationViewport
├── Desktop Application
│   └── AppShell
│       ├── Header
│       └── Shell Body
│           ├── Sidebar
│           └── Content
│
└── Desktop Required
```

Sur un environnement desktop pris en charge, `ApplicationViewport` affiche l’`AppShell`.

Sur un viewport trop étroit ou un appareil uniquement tactile, l’`AppShell` est masqué et la page `DesktopRequired` est affichée.

---

# 3. Persistance entre les routes

Les routes applicatives sont placées dans le Route Group Next.js :

```text
src/app/(application)/
```

Le layout de ce Route Group instancie l’`ApplicationViewport`, qui instancie lui-même l’`AppShell`.

```text
RootLayout
└── ApplicationLayout
    └── ApplicationViewport
        └── AppShell
            └── Route Content
```

Les changements de route internes au Route Group remplacent uniquement les `children` affichés dans le `Content`.

Le `Header`, la `Sidebar` et la structure du Shell restent portés par le layout parent et ne sont pas recréés comme du contenu propre à chaque page.

Le Root Layout reste responsable uniquement :

* des métadonnées globales ;
* de la police globale ;
* des Providers applicatifs ;
* des scripts d’initialisation précédant l’hydratation.

---

# 4. Header

Le `Header` représente la région globale de SignalLab.

Il reste visible au-dessus de la `Sidebar` et du `Content`.

Il contient uniquement des éléments globaux, actuellement :

* l’identité SignalLab ;
* le contrôle Light/Dark.

Il ne contient aucune information liée à une `Organization`, un `Project`, une `Campaign` ou une autre ressource métier courante.

Les futurs contrôles globaux, notamment les paramètres utilisateur, les notifications et le profil, pourront y être ajoutés sans modifier sa responsabilité.

---

# 5. Sidebar

La `Sidebar` est réservée à la future Context Navigation.

SL-014 ne construit encore aucune navigation métier. Elle affiche donc uniquement un état vide indiquant qu’aucun contexte n’est sélectionné.

La Sidebar possède deux états :

```text
expanded
collapsed
```

Largeurs actuelles :

```text
expanded  → 18rem
collapsed → 4rem
```

Son état est conservé dans le navigateur avec la clé :

```text
signallab.shell.sidebar
```

Valeurs autorisées :

```text
expanded
collapsed
```

L’état est également appliqué sur l’élément racine :

```html
<html data-sidebar-state="expanded">
```

ou :

```html
<html data-sidebar-state="collapsed">
```

La largeur de la première colonne du Shell dépend de la variable CSS :

```css
--application-sidebar-width
```

Le changement d’état redimensionne donc réellement la Sidebar et rend l’espace libéré au `Content`.

La Sidebar possède son propre défilement vertical lorsque son contenu dépasse la hauteur disponible.

---

# 6. Content

Le `Content` représente la région principale dans laquelle Next.js affiche la page de la route courante.

Il est indépendant de la Sidebar et ne contrôle jamais le Header.

Il possède :

* sa propre largeur disponible ;
* son propre défilement vertical ;
* une hauteur contrainte par le Shell ;
* une protection contre les débordements horizontaux de la grille.

Les futurs modèles `Explorer`, `Workspace` et `Document` seront introduits par SL-015 et les Specs fonctionnelles correspondantes.

SL-014 ne définit aucun de leurs comportements.

---

# 7. Défilement et dimensions

Le Shell occupe la hauteur dynamique complète du viewport :

```css
height: 100dvh;
```

Le document principal ne défile pas :

```css
body
{
  overflow: hidden;
}
```

Les régions responsables du défilement sont :

```text
Sidebar → défilement contextuel indépendant
Content → défilement de la page courante indépendant
```

Le défilement de l’une ne déplace jamais l’autre ni le Header.

Le corps du Shell utilise une grille à deux colonnes :

```text
Sidebar | Content
```

La colonne `Content` utilise :

```css
minmax(0, 1fr)
```

afin de pouvoir se réduire correctement sans provoquer de débordement causé par son contenu interne.

---

# 8. Apparence

SignalLab supporte deux modes d’apparence :

```text
light
dark
```

Le mode par défaut est :

```text
light
```

La préférence est conservée dans le navigateur avec la clé :

```text
signallab.appearance.mode
```

Valeurs autorisées :

```text
light
dark
```

Le mode est appliqué à l’élément racine avec :

```html
<html data-appearance-mode="light">
```

ou :

```html
<html class="dark" data-appearance-mode="dark">
```

Le mode `system` n’est pas supporté.

SignalLab ne suit pas automatiquement la préférence d’apparence du système d’exploitation.

Les composants utilisent exclusivement les tokens sémantiques définis dans `globals.css`. Ils ne doivent pas contenir directement les couleurs Light ou Dark.

Les tokens couvrent actuellement notamment :

* surfaces générales ;
* texte ;
* bordures ;
* surfaces du Shell ;
* Sidebar ;
* états `success`, `warning` et `error` ;
* alias `destructive`.

Les tokens d’identité propres aux objets métier seront ajoutés seulement lorsque ces objets seront effectivement introduits.

---

# 9. Initialisation avant hydratation

Le serveur ne peut pas lire `localStorage`.

Sans initialisation précoce, le document pourrait être affiché brièvement avec les valeurs par défaut avant d’appliquer les préférences sauvegardées.

Le Root Layout exécute donc deux scripts avec la stratégie Next.js :

```text
beforeInteractive
```

Scripts concernés :

* initialisation de l’apparence ;
* initialisation de l’état de la Sidebar.

Ces scripts appliquent les attributs et classes nécessaires sur `<html>` avant l’hydratation de React.

L’apparence utilise ensuite un External Store compatible avec `useSyncExternalStore` afin de garantir :

* un snapshot serveur déterministe ;
* une hydratation sans divergence ;
* la synchronisation des composants React après un changement ;
* la persistance dans `localStorage`.

Les structures conditionnelles visibles avant l’hydratation sont rendues de manière stable. Les variantes Light/Dark sont présentes dans le même arbre React et leur visibilité est contrôlée par CSS.

---

# 10. Desktop Required

SignalLab est conçu exclusivement pour une utilisation desktop.

L’application desktop est affichée lorsque :

* le viewport atteint au moins `40rem` ;
* l’appareil n’est pas limité à un pointeur tactile imprécis sans capacité de survol.

La protection repose sur CSS et non sur une détection JavaScript.

Le document contient simultanément :

* la branche Desktop Application ;
* la branche Desktop Required.

Les media queries contrôlent leur visibilité sans modifier l’arbre rendu par React.

Seuil actuel :

```css
@media (min-width: 40rem)
```

Détection des appareils uniquement tactiles :

```css
@media (any-hover: none) and (any-pointer: coarse)
```

Sur un environnement non pris en charge :

* le Shell est masqué ;
* aucune adaptation mobile du produit n’est réalisée ;
* une page informative demande d’utiliser un ordinateur avec une souris ou un trackpad.

---

# 11. Organisation des fichiers

```text
src/
├── app/
│   ├── (application)/
│   │   ├── layout.tsx
│   │   ├── page.tsx
│   │   └── page.test.tsx
│   ├── globals.css
│   └── layout.tsx
│
├── components/
│   ├── appearance/
│   │   ├── appearance-toggle.tsx
│   │   └── appearance-toggle.test.tsx
│   │
│   └── shell/
│       ├── app-content.tsx
│       ├── app-header.tsx
│       ├── app-shell.tsx
│       ├── app-shell.test.tsx
│       ├── app-sidebar.tsx
│       ├── app-sidebar.test.tsx
│       ├── application-viewport.tsx
│       ├── application-viewport.test.tsx
│       └── desktop-required.tsx
│
├── lib/
│   ├── appearance.ts
│   └── sidebar.ts
│
└── providers/
    ├── app-providers.tsx
    ├── appearance-provider.tsx
    └── query-provider.tsx
```

---

# 12. Responsabilités des composants

## `ApplicationViewport`

Sélectionne par CSS entre :

* l’application desktop ;
* la page Desktop Required.

Il ne contient aucune logique de navigation ni de domaine métier.

## `AppShell`

Définit les dimensions permanentes du Header et du corps de l’application.

Il compose la Sidebar et le Content sans connaître le contenu de la route.

## `AppHeader`

Affiche uniquement les contrôles globaux.

## `AppSidebar`

Affiche la future région de Context Navigation et gère uniquement son état visuel expanded/collapsed.

## `AppContent`

Héberge les `children` du layout applicatif et fournit leur région de défilement.

## `AppearanceProvider`

Expose le mode d’apparence aux composants clients et synchronise React avec l’état appliqué au document.

---

# 13. Tests

Les tests frontend couvrent actuellement :

* la présence des régions permanentes du Shell ;
* la présence du Header, de la Sidebar et du Content ;
* le rendu du contenu de route ;
* le mode Light par défaut ;
* le passage au mode Dark ;
* la persistance de l’apparence ;
* la restauration d’une apparence appliquée ;
* l’état expanded par défaut de la Sidebar ;
* le passage à l’état collapsed ;
* la persistance de la Sidebar ;
* la restauration d’un état collapsed ;
* la présence des branches Desktop Application et Desktop Required.

JSDOM ne calcule pas les media queries comme un navigateur complet.

Le choix réel entre les deux branches de l’`ApplicationViewport` est donc validé manuellement dans un navigateur jusqu’à l’introduction éventuelle de tests End-to-End avec un navigateur réel.

Les commandes de validation frontend sont :

```powershell
npm.cmd --prefix apps/web run lint
npm.cmd --prefix apps/web run typecheck
npm.cmd --prefix apps/web test
npm.cmd --prefix apps/web run build
```

---

# 14. Hors scope

SL-014 n’implémente pas :

* la Context Navigation ;
* la Local Navigation ;
* le Navigation State ;
* la reconstruction d’un contexte depuis une URL ;
* les modèles `Explorer`, `Workspace` et `Document` ;
* les contextes `Organization`, `Project` ou `Campaign` ;
* l’authentification ;
* les contrôles de permissions ;
* une interface mobile ;
* une adaptation fonctionnelle aux interactions tactiles.

Ces responsabilités appartiennent aux Specs futures, principalement SL-015 pour l’architecture de navigation.

---

# 15. Invariants

Les règles suivantes doivent rester vraies pendant toute l’évolution de SignalLab :

1. Le Header reste global.
2. La Sidebar reste contextuelle.
3. Le Content reste la région de travail de la route courante.
4. Une fonctionnalité métier ne remplace jamais la structure du Shell.
5. Le Header, la Sidebar et le Content ne sont pas recréés dans chaque page.
6. Les scrolls de la Sidebar et du Content restent indépendants.
7. La Sidebar conserve son état entre les sessions.
8. L’apparence conserve son état entre les sessions.
9. Les composants utilisent les tokens sémantiques et non des couleurs de thème directes.
10. SignalLab ne fournit pas d’interface mobile ou tactile.
11. SL-014 ne porte aucune logique de navigation métier.
