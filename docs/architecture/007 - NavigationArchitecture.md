# Navigation Architecture

**Projet :** SignalLab
**Spec :** SL-015 — Implémenter l’architecture de navigation
**Statut :** Validée
**Dernière mise à jour :** 04/08/2026

---

# 1. Objectif

Ce document décrit l’architecture de navigation frontend de SignalLab.

Cette architecture permet aux futures Milestones d’enregistrer leurs ressources et leurs vues sans modifier l’Application Shell ni reconstruire les mécanismes de navigation.

SL-015 ne contient aucune logique métier réelle.

Les Organizations, Projects, Campaigns et Participations présents dans l’application sont uniquement des ressources de démonstration permettant de valider les contrats de navigation.

---

# 2. Principes

L’architecture respecte les principes suivants :

* l’Application Shell reste permanent ;
* le Header conserve uniquement des responsabilités globales ;
* la Sidebar porte la Context Navigation ;
* le Content porte la Local Navigation et le modèle de présentation courant ;
* une URL décrit entièrement la navigation visible ;
* un changement de vue locale ne change jamais le contexte ;
* l’état d’un contexte est indépendant de celui des autres contextes ;
* le navigateur conserve son rôle canonique pour Back, Forward et les URLs profondes ;
* les Functional Overlays ne deviennent jamais un niveau de contexte supplémentaire.

---

# 3. Régions permanentes

L’Application Shell conserve trois régions :

```text
Application Shell
├── Header
├── Sidebar
└── Content
```

Le Content possède deux couches internes :

```text
Content
├── application-content-scrollport
│   └── présentation courante
└── application-content-overlay-root
    ├── Floating Analytics Action
    └── Functional Overlay éventuelle
```

`application-content-scrollport` est l’unique propriétaire du défilement principal du Content.

La couche Overlay est positionnée relativement au Content. Elle ne recouvre jamais le Header ni la Sidebar.

---

# 4. Context Navigation

La Context Navigation ouvre une nouvelle ressource métier.

Elle est principalement exposée dans la Sidebar, mais peut également être déclenchée par :

* un Explorer ;
* une URL ;
* l’historique du navigateur ;
* un futur flux de création ;
* une future Global Search.

Les contextes actuellement reconnus sont :

```text
Home
Organization
Project
Campaign
Participation
```

Chaque contexte possède une clé stable :

```text
home
organization:{organizationId}
project:{projectId}
campaign:{campaignId}
participation:{participationId}
```

Ces clés identifient les états de navigation persistés.

## Hiérarchie de recherche

La hiérarchie professionnelle démontrée est :

```text
Organization
└── Project
    └── Campaign
```

Une Participation appartient à un parcours personnel indépendant et n’est pas présentée comme un enfant navigable de cette hiérarchie dans la Sidebar.

## États visuels

La Sidebar distingue :

* le contexte courant ;
* ses ancêtres ;
* les contextes disponibles.

Le contexte courant utilise `aria-current="location"`.

Les ancêtres restent identifiables sans être présentés comme sélectionnés.

---

# 5. Local Navigation

La Local Navigation change la vue interne d’une ressource sans modifier son contexte.

Elle est utilisée par :

* Organization Explorer ;
* Project Explorer ;
* Campaign Workspace.

Chaque vue locale possède une URL canonique.

Exemples :

```text
/organizations/{organizationId}/projects
/organizations/{organizationId}/projects/{projectId}/campaigns
/organizations/{organizationId}/projects/{projectId}/campaigns/{campaignId}/results
```

Changer de tab :

* conserve la ressource courante ;
* ne change pas la hiérarchie de Sidebar ;
* ne reconstruit pas le Shell ;
* enregistre la nouvelle vue comme dernière vue du contexte.

La Participation utilise une Table of Contents et non une barre de tabs de Workspace.

---

# 6. Modèles de présentation

Les noms Explorer, Workspace et Document décrivent des modèles techniques.

Ils ne sont pas affichés comme titres principaux dans l’interface.

L’identité visible utilise toujours :

```text
Type de ressource / Nom de la ressource
```

Exemples :

```text
Organization / Demo Organization
Project / Demo Project
Campaign / Demo Campaign
Participation / Demo Participation
```

## 6.1 Personal Home

Home utilise le modèle Personal Home.

Il ne possède aucune Local Navigation dans SL-015.

## 6.2 Explorer

Organization et Project utilisent le modèle Explorer.

La barre contextuelle compacte contient :

```text
Resource Identity
Local Navigation
Context Search
Primary Action
```

L’Explorer conserve sa structure même lorsqu’aucune ressource ne correspond à la vue ou à la recherche courante.

La recherche est indépendante pour chaque ressource Explorer.

## 6.3 Workspace

Campaign utilise le modèle Workspace.

La barre contextuelle compacte contient :

```text
Campaign Identity
Local Navigation
Resource Status
```

Les vues démontrées sont :

```text
Overview
Configuration
Form
Recruitment
Participants
Results
Settings
```

Ces vues sont structurelles. Leur contenu métier appartient aux futures Milestones.

## 6.4 Document

Participation utilise le modèle Document.

La barre contextuelle compacte contient :

```text
Participation Identity
Table of Contents
Document Status
```

Les sections sont affichées dans un flux continu :

```text
Overview
Consent
Activities
Progress
Support
```

La Table of Contents positionne le Document sur une section sans changer de ressource.

---

# 7. Routage

Le routage repose sur Next.js App Router.

Les routes de démonstration sont :

```text
/home

/organizations/{organizationId}/{view}

/organizations/{organizationId}/projects/{projectId}/{view}

/organizations/{organizationId}/projects/{projectId}/campaigns/{campaignId}/{view}

/participations/{participationId}/{section}
```

La route `/` redirige vers `/home`.

## Résolution

`resolveNavigationLocation` transforme un pathname en emplacement de navigation typé.

La résolution :

* valide les segments ;
* valide les vues locales ;
* reconstruit la hiérarchie ;
* produit une URL canonique ;
* refuse une route non reconnue.

Les composants d’interface ne parsèment pas eux-mêmes les routes métier.

Les fonctions de construction centralisées produisent les URLs utilisées par la Sidebar, les tabs et les ressources de démonstration.

---

# 8. Navigation State

L’état de navigation est stocké dans :

```text
signallab.navigation.state.v1
```

Le format est versionné.

Chaque ressource peut conserver :

```text
lastLocalView
explorerSearchQuery
scrollPositions
```

Exemple :

```json
{
  "version": 1,
  "resources": {
    "organization:demo-organization": {
      "lastLocalView": "projects",
      "explorerSearchQuery": "research",
      "scrollPositions": {
        "projects": 240
      }
    },
    "campaign:demo-campaign": {
      "lastLocalView": "results",
      "scrollPositions": {
        "results": 420,
        "settings": 80
      }
    }
  }
}
```

## Priorité de l’URL

L’URL reste toujours prioritaire sur l’état mémorisé.

Une URL directe vers `Results` ouvre `Results`, même si la dernière vue mémorisée était `Settings`.

La vue ouverte par cette URL devient ensuite la nouvelle dernière vue du contexte.

## Validation des valeurs persistées

Une vue mémorisée est restaurée uniquement lorsqu’elle appartient encore au catalogue des vues autorisées pour la ressource.

Une valeur ancienne, invalide ou corrompue est remplacée par la vue par défaut.

---

# 9. Restauration du défilement

Le scroll est enregistré sur :

```text
#application-content-scrollport
```

Il n’est pas enregistré sur `window`.

Pour les Explorers et Workspaces, la position est identifiée par :

```text
contextKey + localView
```

Exemple :

```text
campaign:demo-campaign + results
campaign:demo-campaign + settings
```

Chaque vue conserve ainsi sa propre position.

Pour une Participation, toutes les sections utilisent :

```text
participation:{participationId} + document
```

La Participation constitue un Document continu et ne possède donc pas une position indépendante par section.

## Priorités de restauration

L’ordre de priorité est :

1. hash explicite dans l’URL ;
2. position persistée ;
3. début du Content.

Les liens internes utilisent `scroll={false}` afin de laisser SignalLab gérer la restauration du Content sans concurrence avec le scroll automatique de Next.js.

---

# 10. Analysis Functional Overlay

SL-015 introduit l’infrastructure visible de l’Analysis Workspace sans implémenter les capacités Analytics de M09.

L’Overlay est disponible dans les contextes :

```text
Project
Campaign
Campaign Results
```

Elle n’est pas disponible dans :

```text
Home
Organization
Participation
```

## Floating Analytics Action

Le bouton Analytics est rendu dans la couche Overlay du Content.

Il reste ancré dans le coin inférieur droit du Content.

Son ouverture ajoute :

```text
?overlay=analysis
```

à l’URL courante.

## Fenêtre Overlay

La fenêtre ouverte :

* reste contenue dans le Content ;
* conserve le Header et la Sidebar visibles ;
* laisse percevoir le contexte sous-jacent ;
* empêche l’interaction avec ce contexte ;
* possède son propre défilement ;
* conserve son ancrage inférieur droit commun avec le bouton Analytics.

La structure démontrée contient :

```text
Subjects
Scope
Groups
Filters
Visualization
Sources
```

Ces régions sont uniquement des placeholders structurels.

## Fermeture

L’Overlay peut être fermée par :

* le bouton Close ;
* la touche Escape ;
* un clic sur le backdrop ;
* Back dans l’historique du navigateur.

Forward peut la rouvrir lorsque l’entrée existe encore dans l’historique.

Une ouverture provenant directement d’une URL retire uniquement le query parameter de l’Overlay lors de la fermeture.

## Accessibilité

Lorsque l’Overlay est ouverte :

* le contenu sous-jacent devient `inert` ;
* le focus est placé dans la fenêtre ;
* le focus reste contenu dans le dialogue ;
* Escape déclenche la fermeture ;
* le focus revient au bouton Analytics après fermeture ;
* le dialogue expose ses relations ARIA.

---

# 11. Frontières fonctionnelles

SL-015 ne met pas en œuvre :

* AnalysisScope réel ;
* calcul Analytics ;
* Sources analytiques ;
* SavedAnalysis ;
* permissions `ViewAnalytics` ;
* autosave métier ;
* confirmation de fermeture avec modifications ;
* résultats statistiques ;
* visualisations ;
* ressources persistées ;
* authentification ;
* chargement de ressources depuis l’API.

Ces responsabilités appartiennent aux futures Milestones, principalement M02 à M09.

L’Analysis Overlay de SL-015 valide uniquement que l’Application Shell et la navigation peuvent accueillir cette expérience sans créer un nouveau niveau de contexte.

---

# 12. Organisation du code

Les responsabilités principales sont réparties ainsi :

```text
src/navigation
├── navigation-types.ts
├── navigation-routes.ts
├── navigation-resolver.ts
├── navigation-state.ts
├── navigation-view-definitions.ts
└── demo-navigation-data.ts

src/components/navigation
├── context-navigation.tsx
├── local-navigation.tsx
├── navigation-presentation.tsx
├── navigation-route-page.tsx
├── navigation-state-recorder.tsx
├── navigation-scroll-restoration.tsx
├── presentation-context-bar.tsx
├── explorer-presentation.tsx
├── workspace-presentation.tsx
├── document-presentation.tsx
├── document-table-of-contents.tsx
└── personal-home-presentation.tsx

src/components/analysis
└── analysis-overlay-controller.tsx

src/components/shell
├── app-content.tsx
├── app-sidebar.tsx
└── app-shell.tsx
```

Les tests sont colocalisés avec les composants ou modules correspondants.

---

# 13. Extension par les futures Milestones

Une future ressource doit s’intégrer en :

1. définissant ses types de navigation ;
2. ajoutant ses constructeurs de route ;
3. étendant le resolver ;
4. enregistrant son contexte dans la Sidebar ou son Explorer parent ;
5. choisissant un modèle Explorer, Workspace ou Document ;
6. fournissant ses vues locales ;
7. conservant les responsabilités du Shell existant.

Une future fonctionnalité ne doit pas :

* parser librement les URLs dans ses composants métier ;
* recréer une Sidebar locale ;
* transformer une vue locale en nouveau contexte ;
* modifier le Header pour afficher l’identité d’une ressource ;
* contourner le Navigation State avec un second stockage concurrent ;
* afficher une Functional Overlay hors de la couche prévue par le Content.

---

# 14. Validation

SL-015 est validée par :

* ESLint ;
* TypeScript ;
* tests Jest ;
* build de production Next.js ;
* vérification manuelle des routes profondes ;
* vérification manuelle de la restauration des vues ;
* vérification manuelle de la recherche persistée ;
* vérification manuelle du scroll ;
* vérification manuelle de Back et Forward ;
* vérification manuelle de l’Analysis Overlay ;
* vérification manuelle du comportement Desktop Required.

La navigation constitue désormais une fondation stable pour les Milestones métier suivantes.
