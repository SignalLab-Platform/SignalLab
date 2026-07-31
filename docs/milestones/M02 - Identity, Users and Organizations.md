# M02 — Identity, Users and Organizations

**Version :** 2.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 31/07/2026
**Milestone précédente :** M01 — Platform Foundation
**Milestone suivante :** M03 — Project Explorer

---

> Cette Milestone introduit les premiers domaines métier de SignalLab.
>
> Contrairement à M01, aucun élément d'architecture n'est créé.
>
> Tous les contextes viennent simplement s'intégrer dans l'Application Shell défini lors de la Milestone précédente.

---

# Objectif

Permettre à un utilisateur de :

- créer un compte ;
- se connecter ;
- gérer son profil ;
- créer une organisation ;
- rejoindre plusieurs organisations ;
- inviter des utilisateurs ;
- gérer les membres.

---

# Valeur produit

À la fin de cette Milestone, SignalLab devient utilisable comme plateforme collaborative.

Les fonctionnalités métier suivantes pourront ensuite simplement s'appuyer sur les Organizations existantes.

---

# Domaines concernés

- Identity
- Users
- Organizations
- Memberships
- Invitations

---

# Hors scope

Cette Milestone ne contient pas :

- Projects
- Builds
- Campaigns
- Participation
- Results
- Analyses
- Notifications
- Permissions avancées
- Billing

---

# Principes métier

- Un utilisateur possède un compte personnel.
- Un utilisateur peut appartenir à plusieurs organisations.
- Une organisation est totalement isolée des autres.
- Toutes les ressources métier appartiendront plus tard à une organisation.
- Les permissions reposent exclusivement sur les rôles.
- Les invitations sont nominatives.
- Une organisation possède toujours au moins un Owner.

---

# Intégration dans l'Application Shell

Cette Milestone enrichit uniquement le Shell créé dans M01.

Elle ajoute des contextes mais ne modifie jamais :

- Header
- Sidebar
- Navigation
- Layout

---

# Contextes introduits

## Personal Hub

Premier contexte affiché après connexion.

Il représente l'espace personnel de l'utilisateur.

Il permet notamment :

- consulter ses organisations ;
- consulter ses invitations ;
- accéder à son profil.

---

## Organization Explorer

Premier Explorer métier.

Il représente une organisation.

Il affichera progressivement toutes les ressources appartenant à cette organisation.

Pour cette Milestone il contient uniquement :

- membres
- invitations
- paramètres

---

# Scénario d'acceptation

Un nouvel utilisateur doit pouvoir :

Créer un compte

↓

Se connecter

↓

Accéder au Personal Hub

↓

Créer une Organisation

↓

Inviter un utilisateur existant

↓

Celui-ci accepte

↓

Les deux utilisateurs voient la même organisation

↓

Les rôles sont correctement appliqués

---

# Specs

---

## SL-023 — Implémenter l'authentification

### Description

Mettre en place le système d'authentification.

Cette Spec ne crée encore aucun domaine métier.

Elle permet uniquement d'identifier un utilisateur.

### Objectif produit

Permettre l'accès sécurisé à SignalLab.

### Objectif technique

Intégrer le fournisseur d'authentification retenu.

### Critères d'acceptation

- Inscription.
- Connexion.
- Déconnexion.
- Session persistée.

### Definition of Done

Authentification fonctionnelle.

---

## SL-024 — Créer le domaine User

### Description

Introduire l'entité User.

Le User représente une personne utilisant SignalLab.

Il existe indépendamment de toute organisation.

### Objectif produit

Disposer d'une identité unique.

### Critères d'acceptation

- User créé automatiquement.
- Profil stocké.
- Identifiant unique.

### Definition of Done

User opérationnel.

---

## SL-025 — Créer le Personal Hub

### Description

Créer le premier contexte métier de SignalLab.

Le Personal Hub devient le contexte par défaut après connexion.

Il est intégré au Shell existant.

### Il alimente notamment

Header

- informations utilisateur

Sidebar

- Home
- Organizations
- Invitations

Content

- contenu du Personal Hub

### Objectif produit

Fournir un point d'entrée personnel.

### Critères d'acceptation

Le Hub s'affiche correctement.

La navigation fonctionne.

Le Shell reste inchangé.

### Definition of Done

Personal Hub fonctionnel.

---

## SL-026 — Gérer le profil utilisateur

### Description

Permettre à un utilisateur de gérer son profil.

### Fonctionnalités

- nom
- avatar
- langue
- préférences

### Critères d'acceptation

Les modifications sont persistées.

### Definition of Done

Profil opérationnel.

---

## SL-027 — Créer le domaine Organization

### Description

Introduire le domaine Organization.

Une organisation représente un espace collaboratif isolé.

### Principes

Une organisation :

- possède ses membres ;
- possède ses ressources ;
- possède ses permissions.

### Critères d'acceptation

Une organisation peut être créée.

Son Owner est automatiquement créé.

### Definition of Done

Organization opérationnelle.

---

## SL-028 — Implémenter le sélecteur d'organisation

### Description

Ajouter le changement de contexte entre organisations.

Cette Spec n'introduit aucun nouveau layout.

Elle utilise exclusivement le système de navigation de M01.

### Le sélecteur apparaît dans

Header

et modifie le contexte actif.

### Critères d'acceptation

Le changement d'organisation :

- recharge le contexte ;
- conserve le Shell ;
- recharge uniquement le contenu.

### Definition of Done

Context Switch opérationnel.

---

## SL-029 — Créer l'Organization Explorer

### Description

Premier Explorer métier.

Il représente une organisation.

Toutes les futures fonctionnalités (Projects, Campaigns...) seront intégrées dans cet Explorer.

### Navigation locale

Pour cette Milestone :

- Members
- Invitations
- Settings

### Critères d'acceptation

Explorer affiché.

Navigation locale fonctionnelle.

### Definition of Done

Organization Explorer terminé.

---

## SL-030 — Implémenter les Memberships

### Description

Créer le lien entre User et Organization.

Introduction de :

OrganizationMembership

avec :

- Role
- CreatedAt

### Critères d'acceptation

Un utilisateur peut appartenir à plusieurs organisations.

Les permissions utilisent les rôles.

### Definition of Done

Memberships fonctionnels.

---

## SL-031 — Implémenter les Invitations

### Description

Créer le système d'invitations.

Les invitations sont nominatives.

Elles ciblent exclusivement un utilisateur SignalLab existant.

Workflow

Administrator

↓

Invitation créée

↓

Lien partagé

↓

Utilisateur connecté

↓

Acceptation ou refus

↓

Membership créé

### Critères d'acceptation

Une invitation peut être :

- créée ;
- acceptée ;
- refusée ;
- expirer.

Une invitation acceptée ne peut jamais être réutilisée.

### Definition of Done

Cycle de vie complet des invitations implémenté.

---

## SL-032 — Gérer les membres d'une organisation

### Description

Permettre aux administrateurs de consulter et gérer les membres d'une organisation.

Cette Spec enrichit la section **Members** de l'Organization Explorer.

### Fonctionnalités

- Liste des membres
- Recherche
- Consultation des rôles
- Modification des rôles
- Suppression d'un membre

### Règles métier

- Le dernier Owner ne peut jamais être supprimé.
- Un utilisateur peut quitter une organisation.
- Un Owner peut promouvoir un membre.
- Les permissions sont immédiatement mises à jour.

### Critères d'acceptation

Les membres sont correctement affichés.

Les rôles peuvent être modifiés.

Les règles métier sont respectées.

### Definition of Done

Gestion des membres terminée.

---

## SL-033 — Gérer les rôles

### Description

Implémenter le système de rôles de l'organisation.

Les rôles constituent l'unique mécanisme d'autorisation métier.

### Rôles MVP

- Owner
- Administrator
- Member

### Principes

Les permissions ne sont jamais codées directement.

Chaque fonctionnalité future vérifiera un rôle.

### Critères d'acceptation

Les rôles sont persistés.

Les contrôles d'accès fonctionnent.

### Definition of Done

Permissions opérationnelles.

---

## SL-034 — Paramètres d'organisation

### Description

Créer la première section **Settings** de l'Organization Explorer.

Cette section permet de gérer les informations générales d'une organisation.

### Fonctionnalités

- Nom
- Description
- Avatar
- Suppression (future)
- Informations générales

### Critères d'acceptation

Les paramètres sont modifiables.

Les changements sont persistés.

### Definition of Done

Paramètres disponibles.

---

## SL-035 — Sécuriser les accès

### Description

Mettre en place l'autorisation sur les domaines Identity et Organization.

Cette Spec introduit la notion de protection des Contextes.

### Vérifications

- Utilisateur connecté
- Organisation accessible
- Membership valide
- Permission suffisante

### Principes

Toutes les futures fonctionnalités utiliseront ce système.

Aucune Feature métier ne doit implémenter sa propre logique d'autorisation.

### Critères d'acceptation

Les accès non autorisés sont refusés.

Les routes protégées sont sécurisées.

### Definition of Done

Autorisation fonctionnelle.

---

## SL-036 — Finaliser les domaines Identity et Organization

### Description

Stabiliser toute la Milestone.

Cette Spec comprend :

- nettoyage ;
- documentation ;
- refactoring ;
- corrections ;
- revue complète.

### Objectif produit

Disposer d'une base collaborative robuste avant l'arrivée des Projects.

### Objectif technique

Garantir une base stable pour les prochaines Milestones.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

La documentation est à jour.

Les tests sont verts.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M02, SignalLab permet à un utilisateur de :

- créer un compte ;
- s'authentifier ;
- gérer son profil ;
- créer une organisation ;
- appartenir à plusieurs organisations ;
- inviter des utilisateurs existants ;
- accepter ou refuser des invitations ;
- gérer les membres ;
- attribuer des rôles.

L'Application Shell défini dans M01 reste totalement inchangé.

Les domaines **Identity** et **Organization** viennent simplement enregistrer leurs contextes dans cette architecture.

Cette Milestone constitue la première implémentation métier de SignalLab.

Les Milestones suivantes pourront désormais enrichir l'Organization Explorer avec de nouveaux Explorers spécialisés :

- Projects
- Campaigns
- Builds
- Participants
- Results
- MAP
- Analyses

sans jamais remettre en cause la structure globale de l'application.