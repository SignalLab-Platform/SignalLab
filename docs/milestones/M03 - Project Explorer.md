# M03 — Project Explorer

**Version :** 2.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M02 — Identity, Users and Organizations  
**Milestone suivante :** M04 — Campaign Workspace

---

> Cette Milestone introduit le premier véritable domaine métier de SignalLab.
>
> Elle permet aux organisations de créer et gérer des Projects.
>
> Le Project devient le point d'entrée principal de toutes les futures ressources métier de la plateforme.

---

# Objectif

Permettre à une organisation de :

- créer plusieurs Projects ;
- organiser ses travaux ;
- naviguer entre ses Projects ;
- préparer les futures Campaigns ;
- centraliser les Builds associés.

---

# Valeur produit

À la fin de cette Milestone, une organisation dispose d'un véritable espace de travail.

Les Projects deviennent les conteneurs logiques de toutes les futures études.

---

# Domaines concernés

- Projects

---

# Hors scope

Cette Milestone ne contient pas :

- Campaigns
- Builds
- Measures
- Participants
- Results
- MAP
- Analysis
- Hosting
- GitHub Integration

---

# Principes métier

- Un Project appartient exactement à une Organization.
- Une Organization possède plusieurs Projects.
- Les permissions héritent de l'Organization.
- Les ressources futures appartiendront toujours à un Project.
- Un Project peut être archivé.
- Un Project n'est jamais partagé entre Organizations.

---

# Intégration dans l'Application Shell

Cette Milestone ajoute un nouveau contexte métier.

```
Organization Explorer

↓

Project Explorer
```

Le Shell reste inchangé.

---

# Navigation

## Context Navigation

```
Organization

↓

Project
```

---

## Local Navigation

Le Project Explorer introduit les premières sections du Project Workspace. D'autres sections seront ajoutées progressivement au fil des Milestones.

- Overview
- Campaigns
- Builds

Campaigns et Builds sont volontairement vides.

Elles serviront de point d'ancrage aux prochaines Milestones.

---

# Scénario d'acceptation

Un utilisateur doit pouvoir :

Connexion

↓

Choisir une Organization

↓

Créer un Project

↓

L'ouvrir

↓

Le retrouver dans la liste

↓

Le modifier

↓

L'archiver

↓

Le restaurer

---

# Specs

---

## SL-037 — Créer le domaine Project

### Description

Introduire l'entité Project.

Le Project représente un espace de travail appartenant à une Organization.

Toutes les futures ressources métier seront rattachées à un Project.

### Objectif produit

Disposer d'un premier conteneur métier.

### Objectif technique

Créer le domaine Project.

### Principes impliqués

- Ownership par Organization.
- Isolation.
- Nom unique dans une Organization.
- Archivage.

### Critères d'acceptation

Le domaine est créé.

Les migrations sont générées.

Le Repository fonctionne.

### Definition of Done

Le domaine Project est opérationnel.

---

## SL-038 — Construire le Project Explorer

### Description

Créer le premier Explorer métier de SignalLab.

Le Project Explorer devient le point d'entrée de toutes les futures fonctionnalités du Project.

Il respecte intégralement les conventions définies dans M01.

### Cette Spec alimente

Header

- Project courant

Sidebar

- Projects
- Overview
- Campaigns
- Builds

Content

- Vue principale du Project

### Objectif produit

Offrir un espace de travail cohérent.

### Objectif technique

Créer un Explorer réutilisable.

### Principes impliqués

- Le Shell n'est jamais modifié.
- Explorer standardisé.
- Navigation locale.

### Critères d'acceptation

Le Project Explorer s'affiche correctement.

Le changement de Project recharge uniquement le contenu.

### Definition of Done

Project Explorer fonctionnel.

---

## SL-039 — Enregistrer le contexte Project

### Description

Déclarer officiellement le contexte Project dans l'architecture de navigation.

Cette Spec permet au moteur de navigation de connaître les Projects.

### Objectif produit

Naviguer naturellement entre Organizations et Projects.

### Objectif technique

Étendre le Context Navigation.

### Critères d'acceptation

Le changement de Project :

- met à jour le contexte actif ;
- recharge l'Explorer ;
- conserve le Shell.

### Definition of Done

Navigation opérationnelle.

---

## SL-040 — Créer un Project

### Description

Permettre aux utilisateurs autorisés de créer un nouveau Project.

### Informations

- Nom
- Description

### Règles métier

Le nom est unique dans une Organization.

### Critères d'acceptation

Le Project apparaît immédiatement.

Le contexte devient actif.

### Definition of Done

Création fonctionnelle.

---

## SL-041 — Consulter un Project

### Description

Afficher les informations générales d'un Project.

Cette Spec introduit la vue Overview.

### Informations

- Nom
- Description
- Date de création
- Dernière modification

Les sections Campaigns et Builds restent vides.

### Critères d'acceptation

Les informations sont affichées.

La navigation locale fonctionne.

### Definition of Done

Overview disponible.

---

## SL-042 — Modifier un Project

### Description

Permettre de modifier les informations générales.

### Fonctionnalités

- Renommer
- Modifier la description

### Critères d'acceptation

Les changements sont persistés.

Le Project Explorer est mis à jour.

### Definition of Done

Modification fonctionnelle.

---

## SL-043 — Organiser les Projects

### Description

Créer les outils de navigation dans la liste des Projects.

### Fonctionnalités

- Recherche
- Tri alphabétique
- Tri par date
- Filtre Archived

### Objectif produit

Retrouver rapidement un Project.

### Critères d'acceptation

Les listes sont filtrables.

Les tris fonctionnent.

### Definition of Done

Organisation des Projects disponible.

---

## SL-044 — Archiver un Project

### Description

Permettre d'archiver un Project.

L'archivage masque le Project sans supprimer ses données.

### Principes

Toutes les futures ressources restent conservées.

Le Project peut être restauré.

### Critères d'acceptation

Archivage.

Restauration.

Filtrage.

### Definition of Done

Archivage opérationnel.

---

## SL-045 — Permissions Project

### Description

Étendre le système de permissions.

Le Project hérite entièrement des rôles de l'Organization.

Cette Spec prépare les futures permissions spécifiques.

### Principes

Aucune permission propre au Project n'existe encore.

Le système est extensible.

### Critères d'acceptation

Les contrôles d'accès fonctionnent.

Les héritages sont corrects.

### Definition of Done

Permissions validées.

---

## SL-046 — Finaliser le domaine Project

### Description

Stabiliser toute la Milestone.

Cette Spec comprend :

- corrections ;
- refactoring ;
- documentation ;
- tests ;
- revue finale.

### Objectif produit

Disposer d'une base solide avant l'arrivée des Campaigns.

### Critères d'acceptation

Toutes les Specs sont validées.

Les tests sont verts.

La documentation est à jour.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M03, une Organization peut :

- créer plusieurs Projects ;
- naviguer entre eux ;
- consulter leurs informations ;
- les modifier ;
- les organiser ;
- les archiver.

Le **Project Explorer** devient le premier Explorer métier complet de SignalLab.

Les sections **Campaigns** et **Builds** sont désormais présentes dans la navigation mais encore vides.

Les Milestones suivantes viendront enrichir progressivement cet Explorer sans remettre en cause son architecture.