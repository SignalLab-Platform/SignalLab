# M04 — Campaign Workspace

**Version :** 2.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M03 — Project Explorer  
**Milestone suivante :** M05 — Build Management

---

> Cette Milestone introduit le cœur fonctionnel de SignalLab.
>
> Elle permet de créer, organiser et administrer les Campaigns d'un Project.
>
> La Campaign devient le principal espace de travail des équipes UX.

---

# Objectif

Permettre à une équipe de recherche de créer plusieurs Campaigns au sein d'un Project.

Chaque Campaign constitue un espace de travail dédié permettant de préparer une étude utilisateur.

Cette Milestone introduit uniquement la structure de ce Workspace.

Les fonctionnalités liées aux Builds, à la Participation, aux Results et aux Analyses seront implémentées dans les Milestones suivantes.

---

# Valeur produit

À la fin de cette Milestone, une équipe peut :

- créer plusieurs Campaigns ;
- les organiser ;
- naviguer entre elles ;
- préparer leurs informations générales ;
- disposer d'un Workspace prêt à accueillir les prochaines fonctionnalités.

---

# Domaines concernés

- Campaigns

---

# Hors scope

Cette Milestone ne contient pas :

- Build Management
- Participation
- Results
- Analysis
- Scheduling
- Notifications
- Hosting

---

# Principes métier

- Une Campaign appartient à un Project.
- Un Project peut contenir plusieurs Campaigns.
- Les permissions héritent du Project.
- Une Campaign est indépendante des autres Campaigns.
- Les futures ressources métier seront rattachées à une Campaign.

---

# Intégration dans l'Application Shell

Cette Milestone introduit un nouveau contexte.

```

Organization

↓

Project

↓

Campaign Workspace

```

Le Shell défini dans M01 reste inchangé.

---

# Navigation

## Context Navigation

```

Organization

↓

Project

↓

Campaign

```

---

## Navigation locale

Le Campaign Workspace introduit les sections suivantes :

- Overview
- Build
- Participants
- Results
- Analysis
- Settings

Seules **Overview** et **Settings** sont réellement implémentées dans cette Milestone.

Les autres sections constituent les points d'entrée des prochaines Milestones.

---

# Scénario d'acceptation

Un utilisateur doit pouvoir :

Connexion

↓

Choisir une Organization

↓

Choisir un Project

↓

Créer une Campaign

↓

Ouvrir son Workspace

↓

Modifier ses informations

↓

Naviguer entre plusieurs Campaigns

---

# Specs

---

## SL-047 — Créer le domaine Campaign

### Description

Introduire le domaine métier Campaign.

Une Campaign représente une étude appartenant à un Project.

Elle devient le futur point de rattachement des Builds, des Participants, des Results et des Analyses.

### Objectif produit

Permettre aux équipes de structurer leurs études.

### Objectif technique

Créer l'entité Campaign ainsi que son infrastructure de persistance.

### Principes impliqués

- Ownership par Project.
- Isolation.
- Identifiant unique.
- Héritage des permissions.

### Critères d'acceptation

- Le domaine Campaign est créé.
- Les migrations sont générées.
- Les accès au Repository fonctionnent.
- Les APIs CRUD sont disponibles.

### Definition of Done

Le domaine Campaign est opérationnel.

---

## SL-048 — Construire le Campaign Workspace

### Description

Créer le Workspace principal des Campaigns.

Ce Workspace respecte les conventions définies dans l'Application Blueprint.

Il constitue le point d'entrée de toutes les futures fonctionnalités liées à une Campaign.

### Cette Spec alimente

Header

- Campaign active

Sidebar

- Overview
- Build
- Participants
- Results
- Analysis
- Settings

Content

- Workspace de la Campaign

### Objectif produit

Offrir un espace de travail cohérent pour chaque étude.

### Objectif technique

Créer un Workspace standardisé réutilisable.

### Principes impliqués

- Le Shell n'est jamais modifié.
- Les Workspaces suivent une structure commune.
- La navigation locale est indépendante du contenu métier.

### Critères d'acceptation

Le Workspace est correctement affiché.

Le changement de Campaign recharge uniquement son contenu.

La navigation locale fonctionne.

### Definition of Done

Campaign Workspace opérationnel.

---

## SL-049 — Enregistrer le contexte Campaign

### Description

Déclarer officiellement le contexte Campaign auprès du moteur de navigation.

Cette Spec permet au système de navigation de gérer les Campaigns comme un nouveau niveau de contexte.

### Objectif produit

Naviguer naturellement entre Projects et Campaigns.

### Objectif technique

Étendre la Context Navigation.

### Principes impliqués

- Le contexte actif est unique.
- Le Shell reste persistant.
- Les changements de contexte ne reconstruisent jamais l'application.

### Critères d'acceptation

Le changement de Campaign :

- met à jour le contexte actif ;
- recharge le Workspace ;
- conserve le Shell.

### Definition of Done

Le contexte Campaign est intégré.

---

## SL-050 — Créer une Campaign

### Description

Permettre aux utilisateurs autorisés de créer une nouvelle Campaign.

Une Campaign est automatiquement rattachée au Project actuellement actif.

### Informations

- Nom
- Description

Les autres informations seront configurées ultérieurement dans le Workspace.

### Objectif produit

Permettre la création d'une nouvelle étude.

### Objectif technique

Créer le workflow complet de création.

### Principes impliqués

- Création depuis le contexte Project.
- Héritage des permissions.
- Validation des données.

### Critères d'acceptation

Une Campaign peut être créée.

Elle apparaît immédiatement dans la liste.

Son Workspace devient le contexte actif.

### Definition of Done

Création de Campaign fonctionnelle.

## SL-051 — Consulter une Campaign

### Description

Permettre de consulter les informations générales d'une Campaign.

Cette Spec introduit la vue **Overview** du Campaign Workspace.

L'Overview constitue la page d'accueil de toutes les Campaigns.

Elle centralise les informations essentielles de l'étude et deviendra progressivement un tableau de bord à mesure que les prochaines Milestones ajouteront de nouvelles fonctionnalités.

### Cette Spec affiche notamment

- Nom
- Description
- Projet parent
- Date de création
- Dernière modification

Les futures informations (Build, Participants, Results, Analysis...) ne sont pas encore implémentées.

### Objectif produit

Offrir un point d'entrée clair pour chaque Campaign.

### Objectif technique

Créer la première vue métier du Campaign Workspace.

### Principes impliqués

- Une Overview unique par Campaign.
- Informations générales uniquement.
- Les futures sections enrichiront progressivement cette vue.

### Critères d'acceptation

Les informations générales sont affichées.

La navigation locale fonctionne.

Le changement de Campaign recharge correctement l'Overview.

### Definition of Done

Overview disponible.

---

## SL-052 — Modifier une Campaign

### Description

Permettre de modifier les informations générales d'une Campaign.

Cette Spec concerne uniquement les métadonnées de la Campaign.

Elle ne modifie aucun comportement métier.

### Informations modifiables

- Nom
- Description

Les autres propriétés seront introduites dans les Milestones suivantes.

### Objectif produit

Permettre de maintenir les informations de l'étude à jour.

### Objectif technique

Créer le workflow complet de modification.

### Principes impliqués

- Validation des données.
- Mise à jour immédiate.
- Rafraîchissement du Workspace.

### Critères d'acceptation

Les modifications sont persistées.

Les changements sont immédiatement visibles.

Les validations empêchent les données invalides.

### Definition of Done

Modification fonctionnelle.

---

## SL-053 — Organiser les Campaigns

### Description

Créer les outils permettant de retrouver rapidement une Campaign au sein d'un Project.

Cette Spec enrichit le Project Explorer.

### Fonctionnalités

- Recherche
- Tri alphabétique
- Tri par date de création
- Tri par dernière modification

Les filtres avancés seront ajoutés ultérieurement.

### Objectif produit

Faciliter la navigation entre de nombreuses Campaigns.

### Objectif technique

Standardiser les composants de recherche et de tri.

### Principes impliqués

- Recherche instantanée.
- Tri côté client lorsque possible.
- Comportement cohérent avec les autres Explorers.

### Critères d'acceptation

Les Campaigns peuvent être recherchées.

Les tris fonctionnent correctement.

La navigation reste fluide.

### Definition of Done

Organisation des Campaigns disponible.

---

## SL-054 — Archiver une Campaign

### Description

Permettre d'archiver une Campaign.

Une Campaign archivée reste conservée dans le système mais n'apparaît plus dans les listes principales.

Toutes ses données sont conservées.

Les futures ressources associées (Builds, Participants, Results...) restent également conservées.

### Objectif produit

Nettoyer les espaces de travail sans perdre l'historique.

### Objectif technique

Mettre en place un archivage logique.

### Principes impliqués

- Archivage non destructif.
- Restauration possible.
- Conservation complète des données.

### Critères d'acceptation

Une Campaign peut être archivée.

Une Campaign archivée peut être restaurée.

Les listes distinguent les Campaigns actives et archivées.

### Definition of Done

Archivage opérationnel.

## SL-055 — Configurer les paramètres d'une Campaign

### Description

Créer la section **Settings** du Campaign Workspace.

Cette section regroupe toutes les informations de configuration générale d'une Campaign.

Elle constitue le point d'entrée des futurs paramètres introduits par les prochaines Milestones.

### Cette Spec affiche notamment

Informations générales

- Nom
- Description

Gestion

- Archivage

Informations système

- Date de création
- Dernière modification
- Identifiant

Les futurs paramètres (Build, Participation, Notifications...) seront ajoutés progressivement dans cette même section.

### Objectif produit

Centraliser la configuration d'une Campaign.

### Objectif technique

Créer une section Settings extensible sans remettre en cause l'organisation du Workspace.

### Principes impliqués

- Une seule section Settings.
- Toutes les configurations d'une Campaign y sont regroupées.
- Les futures fonctionnalités enrichissent cette section sans créer de nouveaux écrans.

### Critères d'acceptation

La section Settings est accessible depuis la navigation locale.

Les informations sont correctement affichées.

Les modifications disponibles sont persistées.

### Definition of Done

Settings opérationnel.

---

## SL-056 — Finaliser le Campaign Workspace

### Description

Stabiliser l'ensemble du Campaign Workspace.

Cette Spec clôt la première itération fonctionnelle des Campaigns.

Elle comprend notamment :

- corrections ;
- refactoring ;
- harmonisation UX ;
- documentation ;
- revue complète ;
- tests.

### Objectif produit

Disposer d'un Workspace stable avant l'arrivée des Builds.

### Objectif technique

Garantir une base robuste permettant aux prochaines Milestones d'ajouter de nouvelles sections sans modifier l'architecture existante.

### Principes impliqués

- Le Workspace est désormais figé.
- Les futures Milestones enrichissent le Workspace sans modifier sa structure.
- La documentation reste la référence officielle.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Les tests sont verts.

La documentation est à jour.

Le Workspace respecte les conventions définies dans l'Application Blueprint.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M04, une équipe peut :

- créer plusieurs Campaigns ;
- naviguer entre elles ;
- consulter leurs informations générales ;
- modifier leurs métadonnées ;
- organiser et rechercher leurs Campaigns ;
- les archiver ;
- configurer leurs paramètres généraux.

Le **Campaign Workspace** constitue désormais le principal espace de travail de SignalLab.

Les sections **Build**, **Participants**, **Results** et **Analysis** sont présentes dans la navigation locale mais restent volontairement non implémentées. Elles seront progressivement enrichies par les Milestones suivantes sans remettre en cause l'architecture du Workspace.