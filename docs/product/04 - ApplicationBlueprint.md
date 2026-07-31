# Application Blueprint

## Objectif

L'**Application Blueprint** présente la vision globale de l'architecture fonctionnelle de SignalLab.

Contrairement aux spécifications fonctionnelles ou techniques, ce document ne décrit pas le comportement détaillé des fonctionnalités. Il expose la manière dont l'application est organisée afin de permettre à tout membre de l'équipe de comprendre rapidement son fonctionnement général.

Il constitue la référence visuelle de l'application et sert de point d'entrée avant la lecture des autres documents.

---

# 1. Architecture générale de l'application

L'interface de SignalLab est organisée autour de trois zones permanentes.

┌──────────────────────────────────────────────────────────────────────────────┐
│ SignalLab                                     🔔        🌐        👤         │
├──────────────────────┬───────────────────────────────────────────────────────┤
│                      │                                                       │
│ Home                 │                                                       │
│                      │         Context Navigation                            │
│ Organization ▼       │                                                       │
│ Ubisoft              │         Local Navigation                              │
│                      │                                                       │
│ Project ▼            │         Search                                        │
│ Rainbow Six          │                                                       │
│                      │         Primary Actions                               │
│ Campaign ▼           │                                                       │
│ Accessibility Test   │         Workspace                                     │
│                      │                                                       │
│──────────────────────│                                                       │
│ My Participations    │                                                       │
│ Sleep Study #42      │                                                       │
└──────────────────────┴───────────────────────────────────────────────────────┘

L'application repose sur trois régions ayant chacune une responsabilité bien définie.

| Région      | Responsabilité                            |
|-------------|-------------------------------------------|
| **Header**  | Contrôles globaux de l'application        |
| **Sidebar** | Navigation entre les différents contextes |
| **Content** | Espace de travail du contexte courant     |

Cette structure est identique dans toute l'application.

---

# 2. Hiérarchie des contextes

SignalLab est organisé autour d'une hiérarchie de contextes de travail.

Home
│
├── MAP Profile
├── Organization
│     │
│     ├── Project
│     │      │
│     │      ├── Campaign
│     │      └── Saved Analysis *(Project resource; opens overlay)*
│     │
│     ├── Measure
│     └── Form Template
│
└── Participation

Les éléments Organization, Project, Campaign et Participation représentent des contextes de navigation.

`Saved Analysis` est une ressource du Project : elle apparaît dans son Explorer et ouvre l'Analysis Workspace en surcouche sans devenir un niveau de contexte.

Chaque véritable niveau de contexte apporte un périmètre métier plus spécifique que le précédent.

Entrer dans un contexte signifie changer entièrement d'espace de travail tout en conservant, lorsque cela est possible, l'état de navigation précédent.

---

# 3. Navigation entre les contextes

Le **Sidebar** permet de naviguer progressivement dans cette hiérarchie.

Home

Organization ▼
Ubisoft

Project ▼
Rainbow Six

Campaign ▼
Accessibility Test

──────────────────────

My Participations
Sleep Study #42

Le parcours le plus courant est le suivant.

Home
 │
 ▼
Organization
 │
 ▼
Project
 │
 ▼
Campaign

Les **Participations** constituent un parcours indépendant.

Elles sont accessibles directement depuis **Home** sans passer par la hiérarchie Organization → Project → Campaign.

---

# 4. Les différents modèles de navigation

Tous les contextes n'ont pas le même rôle.

SignalLab utilise volontairement plusieurs modèles de navigation selon la nature de l'espace de travail.

## Explorer

Les contextes **Organization** et **Project** utilisent un modèle **Explorer**.

Search

All | Projects | Measures | Form Templates

Primary Actions

Explorer

ou

Search

All | Campaigns | Builds | Analyses | Boards

Primary Actions

Explorer

L'objectif d'un **Explorer** est d'organiser et de parcourir une collection de ressources.

---

## Workspace

Le contexte **Campaign** utilise un modèle **Workspace**.

Campaign

Overview | Configuration | Form | Recruitment | Participants | Results | Settings

────────────────────────────────────────────────────────────────

Workspace

Un **Workspace** permet de travailler sur une ressource unique en changeant simplement de point de vue.

---

## Document

Une **Participation** est présentée sous la forme d'un **Document**.

Overview

Consent

Activities

Progress

Support

L'utilisateur progresse naturellement dans un document unique plutôt que de naviguer entre plusieurs pages.

---

## Analysis Workspace

L'**Analysis Workspace** est une surcouche analytique globale.

Il ne constitue pas un niveau supplémentaire dans la hiérarchie des contextes et n'est pas une vue locale ordinaire d'une Campaign.

Il peut être ouvert depuis plusieurs contextes :

Project | Campaign | Results | Saved Analysis

↓

Floating Analytics Action

↓

Analysis Workspace Overlay

Le contexte d'ouverture initialise le **Analysis Scope** :

- depuis une Campaign ou ses Results, la Campaign courante est présélectionnée ;
- depuis un Project, le Scope est initialisé dans ce Project ;
- depuis une Saved Analysis, sa configuration est restaurée.

Le Scope reste ensuite modifiable tant que toutes les Campaigns sélectionnées appartiennent au même Project.

Fermer la surcouche restaure immédiatement le contexte sous-jacent et son état de navigation.

Une Analysis temporaire ne constitue pas une ressource persistante.

Lorsqu'elle est sauvegardée, elle produit une **Saved Analysis** appartenant au Project.

Les Saved Analyses sont consultables depuis la tab **Analyses** du Project Explorer et rouvrent la même surcouche analytique.

---

## Synthèse

| Contexte      | Modèle        |
|---------------|---------------|
| Home          | Personal Hub  |
| Organization  | Explorer      |
| Project       | Explorer      |
| Campaign      | Workspace     |
| Participation | Document      |
| Analysis      | Overlay Workspace |

Chaque modèle répond à un besoin différent tout en restant cohérent avec le reste de l'application.

---

# 5. Structure d'un Explorer

Tous les **Explorer** suivent exactement la même organisation.

Search

↓

Tabs

↓

Primary Actions

↓

Content

Exemple :

Search...

All | Campaigns | Builds | Analyses | Boards

+ New Campaign ▼

──────────────────────────────────────

▼ Campaigns

Campaign A

Campaign B

Campaign C

Chaque **Explorer** offre systématiquement :

- une **Search** contextuelle ;
- des **Tabs** permettant de filtrer les ressources ;
- des **Primary Actions** pour créer de nouvelles ressources ;
- une zone **Content** présentant les résultats.

La structure reste identique même lorsqu'aucune ressource n'existe.

---

# 6. Les deux types de navigation

SignalLab distingue deux mécanismes de navigation totalement indépendants.

## Context Navigation

La **Context Navigation** consiste à changer d'espace de travail.

Sidebar

↓

Change Context

Exemples :

- Home → Organization
- Organization → Project
- Project → Campaign

Changer de contexte remplace entièrement le **Workspace** affiché.

---

## Local Navigation

La **Local Navigation** consiste à changer de vue au sein d'un même contexte.

Tabs

↓

Change View

Exemples :

- Campaigns
- Builds
- Analyses
- Boards

Le contexte courant reste inchangé.

---

# 7. Les deux systèmes de recherche

SignalLab propose deux recherches complémentaires.

## Global Search

Global Search

↓

Locate Resource

↓

Reconstruct Context

La **Global Search** recherche dans l'ensemble de SignalLab.

Sélectionner un résultat reconstruit automatiquement le contexte nécessaire pour accéder à cette ressource.

---

## Context Search

Context Search

↓

Filter Current Explorer

La **Context Search** agit uniquement sur les ressources du contexte courant.

Elle ne change jamais le contexte de navigation.

---

# 8. Results et Analysis

SignalLab distingue deux responsabilités.

## Results

Les **Results** appartiennent au Campaign Workspace.

Ils permettent de consulter fidèlement les Campaign Participations, Submissions et Responses d'une seule Campaign.

Ils privilégient la lecture individuelle et la traçabilité historique.

## Analysis

L'**Analysis Workspace** permet d'agréger, filtrer, comparer et visualiser plusieurs Campaigns du même Project.

Il utilise les Results comme données sources sans en devenir propriétaire.

Results
↓
preuves individuelles

Analysis
↓
projections, comparaisons et visualisations recalculables

---

# 9. MAP Profile

Le **MAP Profile** appartient au contexte personnel du User.

Il est accessible depuis Home et reste indépendant des Organizations et Campaigns.

Lorsqu'il est partagé, il peut être résolu dynamiquement par l'Analysis Workspace afin de segmenter les données de recherche.

Il ne devient jamais une ressource du Project ou de la Campaign.

---

# 10. Modèle mental de SignalLab

Le schéma suivant résume la philosophie générale de l'application.

SignalLab
│
├── Header
│
├── Sidebar
│      │
│      ├── Home
│      ├── Organization
│      ├── Project
│      ├── Campaign
│      └── Participations
│
└── Content
       │
       ├── Context Navigation
       ├── Local Navigation
       ├── Search
       ├── Primary Actions
       ├── Workspace
       └── Analysis Overlay

SignalLab n'est pas conçu comme une succession de pages indépendantes.

L'utilisateur sélectionne d'abord **où** il souhaite travailler en choisissant un contexte, puis **comment** il souhaite visualiser ce contexte grâce à la navigation locale.

La séparation entre **Context Navigation** et **Local Navigation** constitue le principe fondamental de toute l'architecture fonctionnelle de SignalLab.