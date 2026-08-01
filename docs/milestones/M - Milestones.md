# M00 — MVP Definition

**Version :** 1.0  
**Statut :** ✅ Validée
**Dernière mise à jour :** 31/07/2026  
**Milestone suivante :** M01 — Platform Foundation

---

> **Décision de conception**
>
> Cette Milestone constitue le contrat fonctionnel officiel du MVP de SignalLab.
>
> Toute fonctionnalité développée doit appartenir explicitement au périmètre défini dans ce document.
> Toute nouvelle idée ou évolution devra être ajoutée à une future Milestone et ne devra pas être implémentée directement.

---

# Objectif

Définir précisément le périmètre du **Minimum Viable Product (MVP)** de SignalLab.

Cette Milestone ne produit aucune fonctionnalité logicielle. Elle établit les règles fonctionnelles, les limites du produit et les objectifs du MVP afin de garantir une vision claire et stable pendant tout le développement.

Une fois validée, cette Milestone devient la référence officielle du projet.

---

# Valeur produit

Le MVP doit permettre à une équipe de recherche de concevoir, conduire et analyser une étude utilisateur complète autour d'un produit interactif.

SignalLab doit permettre de :

- créer des Organizations de recherche ;
- gérer des Projects ;
- référencer précisément les Builds étudiés ;
- créer des Campaigns ;
- construire des questionnaires ;
- collecter les réponses des participants ;
- analyser les résultats ;
- segmenter ces analyses grâce au profil MAP des participants.

Le MVP doit être suffisamment complet pour être utilisé dans le cadre d'une étude réelle, notamment pour le projet **Descent**.

---

# Résultat attendu

À la fin du MVP, le cycle complet suivant doit être couvert :

```text
Organization
    ↓
Project
    ↓
Build
    ↓
Measure
    ↓
Campaign
    ↓
Campaign Form
    ↓
Participation
    ↓
Submission
    ↓
Results
    ↓
Analysis
```

Toutes ces étapes doivent pouvoir être réalisées sans outil externe, à l'exception de l'hébergement ou de la distribution des Builds.

---

# Philosophie produit

## Desktop First

SignalLab est conçu exclusivement pour une utilisation sur ordinateur.

L'interface peut s'adapter aux différentes dimensions de fenêtres et d'écrans desktop tant que la structure et les responsabilités du Header, de la Sidebar et du Content restent inchangées.

Les appareils mobiles et les interfaces tactiles ne sont pas pris en charge.

Tout utilisateur accédant au site depuis un appareil mobile doit être redirigé vers une page informative indiquant que SignalLab nécessite un navigateur desktop.

---

## SignalLab est un outil de recherche

SignalLab n'est pas une plateforme de diffusion de jeux.

Il ne constitue ni une bibliothèque de Builds, ni une plateforme communautaire.

Toutes les fonctionnalités sont organisées autour de la réalisation d'études utilisateurs.

---

## Les données vivent dans SignalLab

Le MVP ne prévoit aucun système d'export de données.

Les analyses sont consultées directement dans SignalLab.

Les exports pourront être étudiés dans une future version du produit.

---

## Les Campaigns représentent une étude

Une Campaign représente une étude réelle.

Une Campaign n'est jamais un modèle réutilisable.

La réutilisation des questionnaires appartient aux Form Templates.

Le MVP ne permet pas de dupliquer une Campaign.

Une éventuelle duplication future produira toujours une nouvelle Campaign entièrement indépendante, sans synchronisation avec la Campaign d'origine.

---

## Le profil MAP est vivant

Chaque User possède un unique profil MAP.

Les analyses utilisent toujours le profil MAP actuel du participant.

Les Participations ne stockent jamais de snapshot MAP.

Le partage du profil MAP peut être activé ou désactivé librement par son propriétaire.

La désactivation du partage retire immédiatement le profil MAP des analyses futures tout en conservant les réponses déjà soumises.

---

## Les Builds représentent une version du produit

Un Build représente l'identité d'une version précise du produit étudié.

Le MVP référence uniquement :

- son identité ;
- sa plateforme ;
- sa provenance technique.

Le MVP n'héberge aucun Build.

Le MVP ne distribue aucun Build.

L'hébergement et l'exécution des Builds web directement dans SignalLab font partie de la vision produit mais sont explicitement exclus du MVP.

---

# Fonctionnalités incluses

Le MVP comprend notamment :

- authentification ;
- Organizations ;
- Projects ;
- Builds ;
- Measures ;
- Campaigns ;
- Form Templates ;
- Campaign Forms ;
- Participation ;
- Submission ;
- Results ;
- MAP Profile ;
- filtres MAP ;
- analyses comparatives.

---

# Fonctionnalités explicitement exclues

Le MVP n'inclut pas :

- Scheduled Campaigns ;
- duplication de Campaign ;
- hébergement de Builds ;
- exécution de Builds web ;
- génération automatique de rapports PDF ;
- export CSV (hors produit) ;
- export Excel (hors produit) ;
- API publique ;
- notifications ;
- système de commentaires ;
- chat (hors produit) ;
- collaboration temps réel ;
- application mobile (hors produit) ;
- support des navigateurs mobiles ;
- responsive mobile et adaptations tactiles ;
- gestion avancée des permissions ;
- intégrations GitHub ;
- intégrations Steam ;
- intégrations itch.io.

---

# User Journeys couverts

Le MVP couvre les parcours suivants.

## Participant

- créer un compte ;
- répondre à une Campaign ;
- gérer son profil personnel ;
- créer son profil MAP ;
- partager ou masquer son profil MAP ;
- consulter son historique de participations.

Un Participant n'est pas obligé d'appartenir à une Organization.

---

## Researcher

- créer une Organization ;
- gérer ses Projects ;
- créer des Builds ;
- créer des Measures ;
- créer des Campaigns ;
- construire un questionnaire ;
- ouvrir une Campaign ;
- consulter les résultats ;
- réaliser des analyses.

---

## Administrator d'Organization

- gérer les membres ;
- gérer les ressources appartenant à l'Organization.

---

# Principes métier

## Une Campaign active possède exactement un Build

Une Campaign en préparation peut temporairement ne référencer aucun Build.

Elle ne peut être activée qu'après avoir été associée à exactement un Build appartenant à son Project.

Une fois la Campaign activée, cette référence devient immuable.

---

## Une Participation appartient à une seule Campaign

Une Participation ne peut jamais être partagée entre plusieurs Campaigns.

---

## Une Submission est immuable

Une fois soumise, une Submission ne peut plus être modifiée.

---

## Les réponses sont historiques

Les réponses représentent un instant précis dans le temps.

Elles ne doivent jamais être modifiées automatiquement.

---

## Le profil MAP est actuel

À l'inverse des réponses, le profil MAP est considéré comme vivant.

Les analyses utilisent toujours sa valeur actuelle.

---

# Vision post-MVP

Les évolutions suivantes sont envisagées après le MVP :

- duplication de Campaign ;
- hébergement et stockage d'artefacts de Build ;
- distribution et exécution intégrée des Builds ;
- hébergement de Builds web ;
- runner intégré comparable à itch.io ;
- exécution des Builds uniquement depuis les pages Campaign et Participation ;
- artefacts multiples par Build ;
- exports ;
- intégrations externes ;
- Scheduled Campaigns ;
- API publique.

Ces éléments peuvent influencer les frontières du modèle ou les abstractions techniques uniquement lorsque cela ne nécessite aucune fonctionnalité, infrastructure ou complexité significative supplémentaire dans le MVP.

Aucun service inutilisé ne doit être implémenté uniquement pour préparer une évolution future.

---

# Roadmap du MVP

| Milestone | Objectif                          |
|-----------|-----------------------------------|
| M00       | MVP Definition                    |
| M01       | Platform Foundation               |
| M02       | Identity, Users and Organizations |
| M03       | Project Explorer                  |
| M04       | Campaign Workspace                |
| M05       | Build Management                  |
| M06       | Measures, Forms and Response Model|
| M07       | Participation Flow                |
| M08       | Results Review                    |
| M09       | Analysis Workspace                |
| M10       | MAP Profile                       |
| M11       | Deployment and Operations         |
| M12       | Documentation and Portfolio       |

---

# Specs

---

## SL-000 — Geler le périmètre du MVP

**Statut :** ✅ Validée

### Description

Établir officiellement le périmètre fonctionnel du MVP.

### Critères d'acceptation

- Le périmètre du MVP est validé.
- Les fonctionnalités hors scope sont identifiées.
- Les Milestones du projet sont définies.

---

## SL-001 — Définir le glossaire métier

**Statut :** ✅ Validée

### Description

Définir le vocabulaire métier officiel de SignalLab.

Exemples :

- Organization
- Project
- Build
- Campaign
- Participation
- Submission
- Measure
- Form Template
- MAP Profile

Le glossaire servira de référence pour le code, la documentation et l'interface utilisateur.

---

## SL-002 — Définir les rôles utilisateurs

**Statut :** ✅ Validée

### Description

Définir les responsabilités des différents types d'utilisateurs du MVP.

- Participant
- Organization Member
- Organization Administrator
- Organization Owner

---

## SL-003 — Définir les états métier principaux

**Statut :** ✅ Validée

### Description

Définir les états de vie des principales entités.

Notamment :

- Campaign
- Participation
- Build

---

## SL-004 — Définir les règles d'immutabilité

**Statut :** ✅ Validée

### Description

Identifier quelles données peuvent évoluer et lesquelles deviennent définitivement immuables après certaines actions métier.

---

## SL-005 — Définir les règles MAP

**Statut :** ✅ Validée

### Description

Formaliser les règles concernant :

- création ;
- partage ;
- utilisation dans les analyses ;
- confidentialité.

---

## SL-006 — Définir la stratégie Build

**Statut :** ✅ Validée

### Description

Formaliser le rôle du Build dans SignalLab.

Définir ce qui appartient au MVP et ce qui relève de la vision produit.

---

## SL-007 — Définir le périmètre des analyses

**Statut :** ✅ Validée

### Description

Identifier précisément les capacités analytiques couvertes par le MVP.

---

## SL-008 — Valider officiellement le MVP

**Statut :** ✅ Validée

### Description

La validation de cette Spec marque le début du développement logiciel.

À partir de cette étape, toutes les Milestones suivantes peuvent être implémentées.

---

# M01 — Platform Foundation

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone suivante :** M02

---

# Objectif

Construire les fondations techniques du produit sans introduire de domaine métier.

La Milestone établit le Modular Monolith, l’Application Shell, la navigation, la persistance, les environnements et la chaîne de livraison.

# Valeur produit

Un développeur peut cloner, lancer, tester et déployer une première application vide dont l’architecture est suffisamment stable pour accueillir tous les domaines du MVP.

# Domaines concernés

- Architecture technique
- Application Shell
- Navigation
- Persistence
- Infrastructure

# Hors scope

- Authentification
- Organizations
- Projects
- Campaigns
- Builds
- Forms
- Participation
- Results
- Analytics
- MAP
- SignalR et collaboration temps réel

# Principes

- Software Architecture est la référence de stack.
- Frontend canonique : React, Next.js App Router, TypeScript, TanStack Query, Tailwind CSS et shadcn/ui.
- Backend canonique : ASP.NET Core, Clean Architecture, Entity Framework Core et PostgreSQL.
- Le Shell permanent contient Header, Sidebar et Content.
- SignalR reste une capacité technique post-MVP et n’est pas installé sans besoin fonctionnel.
- Aucune logique métier n’est introduite.

# Scénario d'acceptation

Cloner le dépôt
↓
Lancer Docker Compose
↓
Démarrer Next.js, ASP.NET Core et PostgreSQL
↓
Ouvrir le Shell desktop
↓
Naviguer entre des routes de démonstration
↓
Exécuter CI et premier déploiement

# Specs

---

## SL-010 — Initialiser le dépôt

**Statut :** ✅ Validée

### Description

Créer le monorepository officiel et ses conventions.

### Critères d'acceptation

- Arborescence Backend, Frontend, tests et documentation créée.
- README, licence et .gitignore présents.
- Commandes de développement documentées.

### Definition of Done

- Initialiser le dépôt validé par les tests et la documentation.

---

## SL-011 — Initialiser le Backend

### Description

Créer la solution ASP.NET Core selon Clean Architecture.

### Critères d'acceptation

- Les projets Presentation, Application, Domain et Infrastructure compilent.
- Une API de santé et OpenAPI sont disponibles.
- Aucune dépendance technologique ne remonte vers Domain.

### Definition of Done

- Initialiser le Backend validé par les tests et la documentation.

---

## SL-012 — Initialiser le Frontend

### Description

Créer l’application React avec Next.js App Router.

### Critères d'acceptation

- Next.js, TypeScript, Tailwind et shadcn/ui sont configurés.
- TanStack Query est initialisé pour le Server State.
- Le routing dépend de Next.js ; Vite et TanStack Router ne sont pas utilisés.

### Definition of Done

- Initialiser le Frontend validé par les tests et la documentation.

---

## SL-013 — Figer l’architecture de la solution

### Description

Documenter le Modular Monolith, les dépendances, DTO, API, persistence, tests et ADR.

### Critères d'acceptation

- Les responsabilités de chaque couche sont explicites.
- Les conventions de Vertical Slice et de nommage sont documentées.
- Les décisions de stack disposent d’ADR.

### Definition of Done

- Figer l’architecture de la solution validé par les tests et la documentation.

---

## SL-014 — Construire l’Application Shell

### Description

Implémenter le Header, la Sidebar et le Content permanents.

### Critères d'acceptation

- Le Shell n’est pas reconstruit lors d’un changement de route interne.
- Le Header ne contient que des contrôles globaux.
- La Sidebar porte la navigation contextuelle.
- Un viewport mobile affiche la page desktop requis.

### Definition of Done

- Construire l’Application Shell validé par les tests et la documentation.

---

## SL-015 — Implémenter l’architecture de navigation

### Description

Construire Context Navigation, Local Navigation, restauration d’URL et modèles Explorer, Workspace et Document.

### Critères d'acceptation

- Une URL reconstruit le contexte complet.
- Les changements locaux ne changent pas le contexte.
- L’état de navigation est persistable par ressource.
- L’infrastructure autorise les surcouches fonctionnelles futures, dont Analysis Workspace.

### Definition of Done

- Implémenter l’architecture de navigation validé par les tests et la documentation.

---

## SL-016 — Configurer PostgreSQL

### Description

Installer et configurer PostgreSQL pour les environnements locaux et Docker.

### Critères d'acceptation

- Connexion locale et Docker fonctionnelle.
- Chaîne de connexion externalisée.
- Base unique de vérité persistante.

### Definition of Done

- Configurer PostgreSQL validé par les tests et la documentation.

---

## SL-017 — Configurer Entity Framework Core

### Description

Définir DbContext, conventions Fluent API et migrations.

### Critères d'acceptation

- Une migration vide peut être générée et appliquée.
- Une configuration distincte est prévue par entité.
- Les conventions de contraintes et index sont documentées.

### Definition of Done

- Configurer Entity Framework Core validé par les tests et la documentation.

---

## SL-018 — Configurer les environnements

### Description

Définir Development, Staging et Production.

### Critères d'acceptation

- Aucun secret n’est commité.
- Les variables sont validées au démarrage.
- Les erreurs de configuration sont explicites.

### Definition of Done

- Configurer les environnements validé par les tests et la documentation.

---

## SL-019 — Configurer Docker

### Description

Créer les images et Docker Compose nécessaires au développement.

### Critères d'acceptation

- Frontend, Backend et PostgreSQL démarrent ensemble.
- Les volumes et réseaux sont documentés.
- Le démarrage est reproductible.

### Definition of Done

- Configurer Docker validé par les tests et la documentation.

---

## SL-020 — Mettre en place la CI/CD

### Description

Créer GitHub Actions pour build, lint et tests.

### Critères d'acceptation

- Chaque Pull Request exécute les contrôles.
- Une erreur bloque la fusion.
- Frontend et Backend sont construits séparément.

### Definition of Done

- Mettre en place la CI/CD validé par les tests et la documentation.

---

## SL-021 — Réaliser le premier déploiement

### Description

Déployer le Shell et l’API de santé.

### Critères d'acceptation

- Frontend publiquement accessible.
- Backend répond en HTTPS.
- Configuration de staging validée.

### Definition of Done

- Réaliser le premier déploiement validé par les tests et la documentation.

---

## SL-022 — Finaliser la documentation développeur

### Description

Documenter installation, architecture, workflow Git et conventions.

### Critères d'acceptation

- Un nouveau développeur peut lancer le projet sans assistance.
- Les ADR et commandes sont à jour.
- Les limites MVP et capacités post-MVP sont distinguées.

### Definition of Done

- Finaliser la documentation développeur validé par les tests et la documentation.

---

# Fin de Milestone

SignalLab dispose d’une plateforme vide mais déployable. Toutes les Milestones suivantes ajoutent des domaines sans remettre en cause le Shell ni la stack.

---

# M02 — Identity, Users and Organizations

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M01
**Milestone suivante :** M03

---

# Objectif

Permettre à un User de s’authentifier, gérer son profil, créer une Organization, inviter des Users existants et administrer ses membres par Permissions.

# Valeur produit

SignalLab devient un espace collaboratif multi-tenant dont l’isolation, la propriété et les rôles sont garantis avant l’arrivée des ressources de recherche.

# Domaines concernés

- Identity
- User
- Organization
- OrganizationMember
- OrganizationInvitation
- Role
- Permission Catalogue

# Hors scope

- Projects
- Participation aux Campaigns
- Notifications push
- Roles personnalisés
- Plusieurs Roles par membre
- OrganizationActivityEntry

# Principes

- Clerk assure l’authentification ; le User SignalLab reste l’identité métier.
- Une Organization possède toujours au moins un Owner.
- Chaque OrganizationMember possède exactement un Role système actif.
- Les cas d’usage vérifient des Permissions et jamais directement le nom du Role.
- Une OrganizationInvitation précède le Membership et ne donne aucun accès.
- Un OrganizationMember possède le cycle Active ⇄ Removed et conserve son identité lors d'une réactivation.
- Le sélecteur d’Organization appartient à la Sidebar.

# Scénario d'acceptation

Créer un compte
↓
Créer une Organization
↓
Devenir Owner
↓
Inviter un User existant avec un Role
↓
Le destinataire accepte
↓
OrganizationMember créé atomiquement
↓
Les Permissions sont appliquées

# Specs

---

## SL-023 — Implémenter l’authentification

### Description

Intégrer Clerk avec Next.js et valider ses JWT dans ASP.NET Core.

### Critères d'acceptation

- Inscription, connexion, déconnexion et récupération de session fonctionnent.
- Le Backend résout l’identité externe sans stocker de PasswordHash.
- Les routes protégées refusent les tokens invalides.

### Definition of Done

- Implémenter l’authentification validé par les tests et la documentation.

---

## SL-024 — Créer le domaine User

### Description

Créer le User métier global et sa liaison technique au fournisseur d’identité.

### Critères d'acceptation

- Email unique SignalLab.
- Profil créé à la première authentification valide.
- Aucun credential n’est persisté dans le Domain.

### Definition of Done

- Créer le domaine User validé par les tests et la documentation.

---

## SL-025 — Créer le Personal Hub

### Description

Créer le contexte personnel affichant Organizations, invitations reçues et accès au profil.

### Critères d'acceptation

- Le Hub est le contexte après connexion.
- Seules les ressources accessibles sont affichées.
- Le Shell reste inchangé.

### Definition of Done

- Créer le Personal Hub validé par les tests et la documentation.

---

## SL-026 — Gérer le profil utilisateur

### Description

Permettre la modification des informations personnelles et préférences globales.

### Critères d'acceptation

- Prénom, nom, avatar, langue et fuseau sont persistés.
- Les changements n’altèrent aucun snapshot historique.

### Definition of Done

- Gérer le profil utilisateur validé par les tests et la documentation.

---

## SL-027 — Créer le domaine Organization

### Description

Créer Organization et son premier Owner dans une transaction.

### Critères d'acceptation

- Une Organization peut être créée par un User authentifié avec un Name et un Country obligatoires.
- Website reste facultatif.
- Un OrganizationMember Owner est créé atomiquement.
- L’isolation par Organization est garantie.

### Definition of Done

- Créer le domaine Organization validé par les tests et la documentation.

---

## SL-028 — Implémenter le sélecteur d’Organization

### Description

Ajouter la sélection du contexte Organization dans la Sidebar.

### Critères d'acceptation

- Le changement met à jour Sidebar et Content.
- Le Header reste global.
- Une URL directe restaure l’Organization active.

### Definition of Done

- Implémenter le sélecteur d’Organization validé par les tests et la documentation.

---

## SL-029 — Créer l’Organization Explorer

### Description

Créer l’Explorer avec Members, Invitations et Settings pour cette Milestone.

### Critères d'acceptation

- Recherche et états vides respectent UX Architecture.
- La navigation locale possède ses URLs.
- Les futures ressources peuvent s’y intégrer.

### Definition of Done

- Créer l’Organization Explorer validé par les tests et la documentation.

---

## SL-030 — Implémenter OrganizationMember

### Description

Matérialiser l’appartenance User–Organization et son Role actif.

### Critères d'acceptation

- Une seule appartenance historique par paire User–Organization.
- Cycle Active ⇄ Removed et RemovedAt implémentés.
- JoinedAt, identité et rattachements restent immuables.
- Le retrait supprime l'accès sans effacer l'historique créé.
- Une réactivation conserve le même OrganizationMemberId.

### Definition of Done

- Implémenter OrganizationMember validé par les tests et la documentation.

---

## SL-031 — Implémenter OrganizationInvitation

### Description

Implémenter le cycle Pending, Accepted, Declined, Cancelled et Expired.

### Critères d'acceptation

- Invitation nominative vers un User existant.
- Une seule invitation Pending par paire Organization–User.
- Acceptation atomique avec création d'un OrganizationMember ou réactivation du Membership Removed existant.
- Le Role porté par l'invitation devient le Role actif du Membership créé ou réactivé.
- Une invitation terminale est inutilisable.
- Les invitations restent consultables sans système de notifications MVP.

### Definition of Done

- Implémenter OrganizationInvitation validé par les tests et la documentation.

---

## SL-032 — Gérer les membres

### Description

Lister, rechercher, retirer et changer le Role des OrganizationMembers.

### Critères d'acceptation

- Le dernier Owner ne peut être retiré, rétrogradé ou quitter.
- Un Administrator ne gère pas les Owners.
- Le retrait passe le Membership à Removed et renseigne RemovedAt.
- Un Membership Removed ne retrouve l'accès que par acceptation d'une nouvelle OrganizationInvitation.
- Les Permissions changent immédiatement.

### Definition of Done

- Gérer les membres validé par les tests et la documentation.

---

## SL-033 — Implémenter Roles et Permission Catalogue

### Description

Créer Member, Administrator et Owner comme ensembles système de Permissions.

### Critères d'acceptation

- Exactement un Role actif par membre.
- Les cas d’usage vérifient la Permission requise.
- Les Organizations ne modifient pas le catalogue MVP.

### Definition of Done

- Implémenter Roles et Permission Catalogue validé par les tests et la documentation.

---

## SL-034 — Gérer les paramètres d’Organization

### Description

Créer la section Settings pour nom, pays, description, logo et site web.

### Critères d'acceptation

- Seuls les membres autorisés modifient les données.
- Name et Country restent obligatoires.
- Website est facultatif et validé comme URL lorsqu'il est renseigné.
- Les changements sont autosauvegardés.
- La suppression d’Organization reste hors scope.

### Definition of Done

- Gérer les paramètres d’Organization validé par les tests et la documentation.

---

## SL-035 — Sécuriser l’isolation et les accès

### Description

Centraliser l’autorisation et la résolution du tenant.

### Critères d'acceptation

- Un identifiant connu ne contourne jamais l’isolation.
- Toutes les mutations vérifient Membership et Permission.
- Les erreurs 401, 403 et 404 sont cohérentes.

### Definition of Done

- Sécuriser l’isolation et les accès validé par les tests et la documentation.

---

## SL-036 — Finaliser Identity et Organizations

### Description

Stabiliser les domaines, UX, tests et documentation.

### Critères d'acceptation

- Cycles d’invitation couverts par tests.
- Invariants du dernier Owner couverts.
- Aucune nomenclature OrganizationMembership ou OrganizationRole ne subsiste.

### Definition of Done

- Finaliser Identity et Organizations validé par les tests et la documentation.

---

# Fin de Milestone

Un User peut collaborer dans plusieurs Organizations avec une identité globale et des Permissions déterministes. Les futures ressources héritent de cette frontière.

---

# M03 — Project Explorer

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M02
**Milestone suivante :** M04

---

# Objectif

Introduire Project comme frontière opérationnelle des Campaigns, Builds et SavedAnalysis.

# Valeur produit

Une Organization peut organiser plusieurs ensembles d’études indépendants et naviguer entre eux sans permissions propres au Project dans le MVP.

# Domaines concernés

- Project
- ProjectStatus
- Project Explorer

# Hors scope

- ProjectMember
- ProjectRole
- Label
- ProjectActivityEntry
- Restrictions d’accès propres au Project

# Principes

- Un Project appartient à une Organization et ne peut être déplacé.
- Tous les OrganizationMembers autorisés accèdent aux Projects selon leurs Permissions d’Organization.
- Project suit Active ⇄ Archived.
- Un Project Archived ne reçoit plus de nouvelles ressources.
- Le contexte Project est porté par Sidebar et Content, jamais par le Header global.

# Scénario d'acceptation

Ouvrir une Organization
↓
Créer un Project
↓
Ouvrir son Explorer
↓
Rechercher et trier
↓
Archiver
↓
Restaurer

# Specs

---

## SL-037 — Créer le domaine Project

### Description

Créer Project, ProjectStatus et ownership Organization.

### Critères d'acceptation

- Project créé Active.
- OrganizationId et créateur immuables.
- Aucun ProjectMember ou ProjectRole MVP.

### Definition of Done

- Créer le domaine Project validé par les tests et la documentation.

---

## SL-038 — Construire le Project Explorer

### Description

Créer l’Explorer standard du Project.

### Critères d'acceptation

- Le Workspace Header local présente le Project.
- Les tabs initiales Overview, Campaigns et Builds existent.
- Analyses sera ajoutée en M09 et Boards restera future.

### Definition of Done

- Construire le Project Explorer validé par les tests et la documentation.

---

## SL-039 — Enregistrer le contexte Project

### Description

Intégrer Project à Context Navigation et aux URLs profondes.

### Critères d'acceptation

- Sidebar reflète Organization puis Project.
- Le changement conserve le Shell.
- Les états de chaque Project sont indépendants.

### Definition of Done

- Enregistrer le contexte Project validé par les tests et la documentation.

---

## SL-040 — Créer un Project

### Description

Créer un Project depuis l’Organization courante.

### Critères d'acceptation

- Nom obligatoire et description facultative.
- Le Project apparaît immédiatement.
- Aucune unicité de nom non définie par le Domain n’est imposée.

### Definition of Done

- Créer un Project validé par les tests et la documentation.

---

## SL-041 — Consulter un Project

### Description

Afficher ses métadonnées et futurs points d’entrée.

### Critères d'acceptation

- Nom, description, statut et dates visibles.
- Campaigns et Builds disposent d’états vides.

### Definition of Done

- Consulter un Project validé par les tests et la documentation.

---

## SL-042 — Modifier un Project

### Description

Modifier nom et description d’un Project Active.

### Critères d'acceptation

- Autosave et feedback de synchronisation.
- Un Project Archived est en lecture seule.

### Definition of Done

- Modifier un Project validé par les tests et la documentation.

---

## SL-043 — Organiser les Projects

### Description

Ajouter recherche, tris et filtre Archived.

### Critères d'acceptation

- Recherche instantanée.
- Tri alphabétique et par dates.
- État d’Explorer restauré.

### Definition of Done

- Organiser les Projects validé par les tests et la documentation.

---

## SL-044 — Archiver et restaurer un Project

### Description

Implémenter Active ⇄ Archived.

### Critères d'acceptation

- Archivage non destructif.
- Aucune nouvelle ressource dans un Project Archived.
- Restauration sans mutation des ressources existantes.

### Definition of Done

- Archiver et restaurer un Project validé par les tests et la documentation.

---

## SL-045 — Appliquer les Permissions Project

### Description

Utiliser exclusivement les Permissions héritées de l’Organization.

### Critères d'acceptation

- Aucune table de Membership Project.
- Les actions sont refusées sans Permission.

### Definition of Done

- Appliquer les Permissions Project validé par les tests et la documentation.

---

## SL-046 — Finaliser Project

### Description

Stabiliser domaine, navigation, UX et tests.

### Critères d'acceptation

- Isolation et archivage testés.
- Concepts ProjectMember, ProjectRole, Label et Activity absents du MVP implémenté.

### Definition of Done

- Finaliser Project validé par les tests et la documentation.

---

# Fin de Milestone

Le Project devient la frontière de travail de toutes les études et peut accueillir les Builds, Campaigns puis SavedAnalysis.

---

# M04 — Campaign Workspace

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M03
**Milestone suivante :** M05

---

# Objectif

Créer le domaine Campaign et son Workspace cible, sans encore implémenter Build, Form, recrutement ou collecte.

# Valeur produit

Une équipe peut créer et organiser ses études Draft dans un espace stable que les Milestones suivantes enrichissent sans ajouter de nouveau niveau de contexte.

# Domaines concernés

- Campaign
- CampaignStatus
- Campaign Workspace
- CampaignContext limité

# Hors scope

- Build Management
- Formulaires
- Activation réellement satisfaisable
- Recruitment
- Participation
- Results
- Analysis Workspace

# Principes

- Campaign appartient à un Project.
- Le Workspace local cible Overview, Configuration, Form, Recruitment, Participants, Results et Settings.
- Analysis n’est pas une tab locale : il sera une surcouche globale context-aware en M09.
- Campaign est créée Draft.
- Archived est terminal dans le MVP.
- Completed résulte d’une action explicite de fin de collecte, jamais d’une date seule.

# Scénario d'acceptation

Créer une Campaign Draft
↓
Ouvrir son Workspace
↓
Modifier ses métadonnées et son contexte
↓
Voir les prérequis futurs
↓
Archiver un Draft abandonné

# Specs

---

## SL-047 — Créer le domaine Campaign

### Description

Créer Campaign, CampaignStatus et rattachement au Project.

### Critères d'acceptation

- Statuts Draft, Active, Completed et Archived définis.
- Transitions non documentées refusées.
- CreatedByOrganizationMemberId utilisé.

### Definition of Done

- Créer le domaine Campaign validé par les tests et la documentation.

---

## SL-048 — Construire le Campaign Workspace

### Description

Créer le Workspace et sa navigation locale cible.

### Critères d'acceptation

- Tabs cibles enregistrées, avec états non disponibles explicites selon les Milestones.
- Analysis n’est pas une tab.
- Workspace Header reste constant entre les vues.

### Definition of Done

- Construire le Campaign Workspace validé par les tests et la documentation.

---

## SL-049 — Enregistrer le contexte Campaign

### Description

Intégrer Campaign comme enfant du contexte Project.

### Critères d'acceptation

- URL profonde restaure Organization, Project et Campaign.
- Sidebar et Content reflètent le contexte.

### Definition of Done

- Enregistrer le contexte Campaign validé par les tests et la documentation.

---

## SL-050 — Créer une Campaign

### Description

Créer une Campaign Draft avec nom et description.

### Critères d'acceptation

- Aucun Build ni Form requis à la création.
- Workspace ouvert après création.
- Préparation incomplète clairement affichée.

### Definition of Done

- Créer une Campaign validé par les tests et la documentation.

---

## SL-051 — Consulter l’Overview

### Description

Afficher métadonnées, statut et état de préparation.

### Critères d'acceptation

- Project parent, dates et prérequis visibles.
- Les futures données sont projetées sans duplication.

### Definition of Done

- Consulter l’Overview validé par les tests et la documentation.

---

## SL-052 — Modifier les métadonnées

### Description

Modifier nom et description internes selon le statut.

### Critères d'acceptation

- Draft modifiable.
- Après activation, seules les métadonnées internes sans incidence historique restent modifiables.

### Definition of Done

- Modifier les métadonnées validé par les tests et la documentation.

---

## SL-053 — Configurer le CampaignContext

### Description

Créer la version MVP du contexte d’étude.

### Critères d'acceptation

- ExperienceType, ParticipationMode, Environment, Platform, ExpectedDuration, Instructions et AdditionalContext disponibles.
- Modifiable en Draft.
- Participant-facing et analytique figés à l’activation.

### Definition of Done

- Configurer le CampaignContext validé par les tests et la documentation.

---

## SL-054 — Organiser les Campaigns

### Description

Ajouter recherche, tri et filtres de statut dans le Project Explorer.

### Critères d'acceptation

- Campaigns retrouvables par nom et dates.
- Archived séparées de l’activité courante.

### Definition of Done

- Organiser les Campaigns validé par les tests et la documentation.

---

## SL-055 — Implémenter le cycle de vie de base

### Description

Implémenter Draft → Archived et le contrat des transitions futures.

### Critères d'acceptation

- Draft peut être archivée sans suppression.
- Archived est terminal dans le MVP.
- Les commandes Active, Completed et archivage depuis Completed disposent de contrats mais sont finalisées lorsque leurs prérequis existent.

### Definition of Done

- Implémenter le cycle de vie de base validé par les tests et la documentation.

---

## SL-056 — Finaliser Campaign Workspace

### Description

Stabiliser le Workspace et ses points d’extension.

### Critères d'acceptation

- Aucune restauration de Campaign Archived.
- Aucune tab Analysis locale.
- Tests de contexte et d’immutabilité préparés.

### Definition of Done

- Finaliser Campaign Workspace validé par les tests et la documentation.

---

# Fin de Milestone

Le Campaign Workspace est stable. Les Milestones M05–M08 remplissent ses vues Configuration, Form, Recruitment, Participants et Results.

---

# M05 — Build Management

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M04
**Milestone suivante :** M06

---

# Objectif

Implémenter Build comme référence métier d’une version testable et permettre son association à une Campaign Draft.

# Valeur produit

Chaque étude peut identifier précisément la version testée sans transformer SignalLab en stockage, distribution ou système de compilation.

# Domaines concernés

- Build
- BuildStatus
- BuildProvenanceType
- Association Campaign–Build

# Hors scope

- Upload
- Stockage
- Téléchargement géré
- Exécution
- Git
- Repository
- Branche
- Commit
- Intégrations de distribution

# Principes

- Build appartient à un Project.
- Identité : Project, Version, Platform, Variant, ProvenanceType et ProvenanceValue.
- External est une URL ; Manual est une description libre.
- L’identité est immuable et toute correction produit un nouveau Build.
- Seules Name, Description, Tags et Notes sont modifiables sur un Build Active.
- Build suit Active ⇄ Archived.
- Campaign Draft référence zéro ou un Build ; la référence est figée à l’activation.

# Scénario d'acceptation

Créer un Build External ou Manual
↓
Le retrouver dans le Project
↓
L’associer à une Campaign Draft
↓
Changer l’association en Draft
↓
Activer plus tard la Campaign
↓
Conserver la référence historique

# Specs

---

## SL-057 — Créer le domaine Build

### Description

Créer Build, BuildStatus et BuildProvenanceType.

### Critères d'acceptation

- Propriétés conformes au Domain Model.
- Contrainte d’identité unique dans le Project.
- Aucune propriété Git.

### Definition of Done

- Créer le domaine Build validé par les tests et la documentation.

---

## SL-058 — Construire la bibliothèque de Builds

### Description

Créer la tab Builds du Project et les composants de consultation.

### Critères d'acceptation

- Liste, détail et états vides.
- Même projection réutilisée dans la configuration Campaign.

### Definition of Done

- Construire la bibliothèque de Builds validé par les tests et la documentation.

---

## SL-059 — Enregistrer un Build

### Description

Créer le workflow External ou Manual.

### Critères d'acceptation

- Version et Platform obligatoires.
- Variant facultative.
- External valide une URL ; Manual exige un texte non vide.
- Organization reste responsable de l’accès externe.

### Definition of Done

- Enregistrer un Build validé par les tests et la documentation.

---

## SL-060 — Consulter un Build

### Description

Afficher identité, provenance, métadonnées, statut et utilisation.

### Critères d'acceptation

- Campaigns référentes visibles.
- Aucun faux bouton de téléchargement ou lancement.

### Definition of Done

- Consulter un Build validé par les tests et la documentation.

---

## SL-061 — Modifier les métadonnées descriptives

### Description

Modifier Name, Description, Tags et Notes sur un Build Active.

### Critères d'acceptation

- Identité et provenance restent immuables.
- Archived est en lecture seule.

### Definition of Done

- Modifier les métadonnées descriptives validé par les tests et la documentation.

---

## SL-062 — Organiser les Builds

### Description

Ajouter recherche, tris et filtre Archived.

### Critères d'acceptation

- Recherche sur nom, version, plateforme, variante et provenance.
- État d’Explorer persisté.

### Definition of Done

- Organiser les Builds validé par les tests et la documentation.

---

## SL-063 — Associer un Build à une Campaign Draft

### Description

Sélectionner un Build Active du même Project.

### Critères d'acceptation

- Zéro ou un Build en Draft.
- Aucune copie du Build.
- Build d’un autre Project refusé.

### Definition of Done

- Associer un Build à une Campaign Draft validé par les tests et la documentation.

---

## SL-064 — Changer ou retirer le Build en Draft

### Description

Mettre à jour la référence avant activation.

### Critères d'acceptation

- Changement atomique.
- Interdit après activation.
- Build Archived empêche l’activation.

### Definition of Done

- Changer ou retirer le Build en Draft validé par les tests et la documentation.

---

## SL-065 — Archiver et restaurer un Build

### Description

Implémenter Active ⇄ Archived.

### Critères d'acceptation

- Archived non sélectionnable pour une nouvelle Campaign.
- Références historiques restent visibles.
- Aucune suppression physique MVP.

### Definition of Done

- Archiver et restaurer un Build validé par les tests et la documentation.

---

## SL-066 — Finaliser Build Management

### Description

Stabiliser domaine, UX, permissions et tests.

### Critères d'acceptation

- Aucune connaissance de Git ou d’artefact.
- Invariants d’identité et d’association couverts.

### Definition of Done

- Finaliser Build Management validé par les tests et la documentation.

---

# Fin de Milestone

Le Project possède une bibliothèque de références de versions testables et les Campaigns Draft peuvent sélectionner leur Build exact.

---

# M06 — Measures, Forms and Analytical Bindings

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M05
**Milestone suivante :** M07

---

# Objectif

Implémenter le vocabulaire analytique, le catalogue de QuestionTypes, les FormTemplates versionnés et leur copie indépendante vers CampaignForm.

# Valeur produit

Les équipes peuvent préparer des questionnaires réutilisables et définir explicitement comment chaque Question contribue à une ou plusieurs Measures.

# Domaines concernés

- Measure
- MeasureOutcomeDirection
- FormTemplate
- FormTemplateVersion
- Section
- Question
- QuestionType
- QuestionMeasureBinding
- CampaignForm

# Hors scope

- Submission
- Response persistées
- Participation
- Analytics calculés
- IA ou détection sémantique
- Types de Questions hors catalogue MVP

# Principes

- Measures appartiennent à l’Organization.
- Question et Measure sont reliées many-to-many par bindings explicites.
- Scored/Contextual, Aligned/Inverted et Weight sont configurés manuellement.
- Aucune Measure, direction ou pondération n’est déduite du texte.
- FormTemplateVersion est immuable.
- CampaignForm est une copie indépendante et ses bindings historiques sont figés à l’activation.

# Scénario d'acceptation

Créer une Measure
↓
Créer un FormTemplate
↓
Ajouter Sections et Questions MVP
↓
Configurer plusieurs bindings pondérés
↓
Publier une version
↓
Générer le CampaignForm
↓
Adapter en Draft

# Specs

---

## SL-067 — Créer le domaine Measure

### Description

Créer Measure, OutcomeDirection et cycle Active ⇄ Archived.

### Critères d'acceptation

- Key canonique et concept sémantique stables.
- OutcomeDirection modifiable uniquement avant usage historique.
- Archived non utilisable pour de nouveaux bindings.

### Definition of Done

- Créer le domaine Measure validé par les tests et la documentation.

---

## SL-068 — Construire la gestion des Measures

### Description

Créer l’Explorer Organization des Measures.

### Critères d'acceptation

- Créer, consulter, modifier rédactionnellement, archiver et restaurer.
- Usage dans Templates et CampaignForms visible.

### Definition of Done

- Construire la gestion des Measures validé par les tests et la documentation.

---

## SL-069 — Implémenter FormTemplate et QuestionTypes

### Description

Créer FormTemplate, Section, Question et le catalogue MVP.

### Critères d'acceptation

- Scale, Number, Boolean, SingleChoice, MultipleChoice, ShortText et LongText disponibles.
- Configurations et validations déterministes.
- Famille analytique dérivée de la structure.

### Definition of Done

- Implémenter FormTemplate et QuestionTypes validé par les tests et la documentation.

---

## SL-070 — Construire le FormTemplate Workspace

### Description

Créer l’éditeur structuré des Sections et Questions.

### Critères d'acceptation

- Réorganisation, édition et autosave en Active.
- Aucun constructeur générique hors catalogue.
- État des modifications non publiées visible.

### Definition of Done

- Construire le FormTemplate Workspace validé par les tests et la documentation.

---

## SL-071 — Publier et versionner un FormTemplate

### Description

Créer une FormTemplateVersion immuable à chaque publication.

### Critères d'acceptation

- Le FormTemplate vivant ne porte aucun numéro de version.
- VersionNumber, auteur et date sont portés uniquement par chaque FormTemplateVersion publiée.
- Contenu complet et bindings copiés.
- Versions précédentes consultables.

### Definition of Done

- Publier et versionner un FormTemplate validé par les tests et la documentation.

---

## SL-072 — Configurer QuestionMeasureBinding

### Description

Permettre zéro, une ou plusieurs Measures par Question.

### Critères d'acceptation

- Paire Question–Measure unique.
- Scored exige famille scalaire, direction et Weight > 0.
- Contextual ne contribue à aucun score.
- Poids par défaut 1.0 et indépendants entre Measures.

### Definition of Done

- Configurer QuestionMeasureBinding validé par les tests et la documentation.

---

## SL-073 — Générer le CampaignForm

### Description

Copier une FormTemplateVersion dans une Campaign Draft.

### Critères d'acceptation

- Sections, Questions, configurations et bindings copiés avec provenance.
- Aucune synchronisation ultérieure.
- Un seul CampaignForm par Campaign.

### Definition of Done

- Générer le CampaignForm validé par les tests et la documentation.

---

## SL-074 — Adapter le CampaignForm Draft

### Description

Modifier la copie sans altérer Template ou Version.

### Critères d'acceptation

- Bindings Campaign propres, ajoutables et modifiables en Draft.
- Mesures de la même Organization uniquement.

### Definition of Done

- Adapter le CampaignForm Draft validé par les tests et la documentation.

---

## SL-075 — Figer le CampaignForm à l’activation

### Description

Finaliser les validations structurelles et le gel historique.

### Critères d'acceptation

- FrozenAt renseigné.
- Questions, options, validation, ordre et CampaignFormQuestionMeasureBindings immuables.
- Activation refusée pour type, configuration ou binding invalide.

### Definition of Done

- Figer le CampaignForm à l’activation validé par les tests et la documentation.

---

## SL-076 — Définir les contrats de valeur et familles analytiques

### Description

Formaliser les représentations de Response attendues par chaque QuestionType.

### Critères d'acceptation

- Scalar, Ordinal, Binary, Categorical, MultiCategorical, Textual et NonScorable déterministes.
- Number Scored exige bornes finies.
- Types média, date et fichier hors MVP.

### Definition of Done

- Définir les contrats de valeur et familles analytiques validé par les tests et la documentation.

---

## SL-077 — Finaliser Measures et Forms

### Description

Stabiliser versioning, copie, immutabilité et permissions.

### Critères d'acceptation

- Aucun MeasureId direct dans Question.
- Tests de copie et de gel complets.
- Le CampaignForm prêt participe aux prérequis d’activation.

### Definition of Done

- Finaliser Measures et Forms validé par les tests et la documentation.

---

# Fin de Milestone

SignalLab dispose d’un modèle de questionnaire historique et analytiquement riche. La collecte concrète des Submissions et Responses arrive en M07.

---

# M07 — Participation Flow

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M06
**Milestone suivante :** M08

---

# Objectif

Permettre à un User authentifié de rejoindre une Campaign Active via un lien, accepter les conditions, remplir le CampaignForm et soumettre une unique Submission.

# Valeur produit

Le premier cycle de collecte réel est complet et conserve des snapshots interprétables sans dépendre de données personnelles futures.

# Domaines concernés

- ParticipantProfile
- CampaignParticipation
- CampaignAccessLink
- CampaignConsentDefinition
- DemographicSnapshot
- ConsentSnapshot
- Submission
- Response

# Hors scope

- CampaignInvitation
- Application
- Announcement
- Recruitment avant activation
- Plusieurs Submissions
- Reprise multi-appareils avancée

# Principes

- CampaignParticipation existe avant Submission.
- Une seule participation par Participant et Campaign.
- Lien Active/Disabled avec expiration facultative et sans quota MVP.
- CampaignParticipation créée après acceptation des conditions.
- Accepted, Started, Completed, Abandoned et Expired sont explicites.
- Submission reste Draft pendant la saisie puis Submitted ou Abandoned.
- Build et CampaignForm doivent être figés avant ouverture.

# Scénario d'acceptation

Ouvrir un CampaignAccessLink valide
↓
S’authentifier
↓
Créer ou compléter ParticipantProfile
↓
Consulter contexte et consentement
↓
Accepter et créer CampaignParticipation
↓
Commencer
↓
Répondre avec sauvegarde Draft
↓
Soumettre atomiquement
↓
Participation Completed

# Specs

---

## SL-078 — Créer ParticipantProfile et CampaignParticipation

### Description

Implémenter le profil Participant et la relation à une Campaign.

### Critères d'acceptation

- Un User possède au maximum un ParticipantProfile.
- Une seule CampaignParticipation par Campaign en MVP.
- Rattachements et snapshots immuables.

### Definition of Done

- Créer ParticipantProfile et CampaignParticipation validé par les tests et la documentation.

---

## SL-079 — Implémenter CampaignAccessLink

### Description

Créer, désactiver, réactiver et expirer les liens.

### Critères d'acceptation

- Cycle Active ⇄ Disabled.
- ExpiresAt facultatif vérifié à l’accès.
- Aucun MaximumUses.
- Lien utilisable uniquement pour Campaign Active.

### Definition of Done

- Implémenter CampaignAccessLink validé par les tests et la documentation.

---

## SL-080 — Construire le Landing Flow

### Description

Présenter Campaign, Project, Build et CampaignContext avant acceptation.

### Critères d'acceptation

- Lien ne crée aucune Participation à l’ouverture.
- Retour après authentification vers le lien initial.
- Build External affiché comme lien si prévu ; Manual comme instruction de localisation.

### Definition of Done

- Construire le Landing Flow validé par les tests et la documentation.

---

## SL-081 — Configurer consentement et participation

### Description

Implémenter CampaignParticipationSettings et CampaignConsentDefinition.

### Critères d'acceptation

- MaxSubmissions = 1 et AccessMode = AccessLink.
- RequireExplicitConsent est l'unique source de vérité sur l'obligation de consentement.
- Une CampaignConsentDefinition complète est obligatoire à l'activation uniquement lorsque RequireExplicitConsent = true.
- Aucune définition n'est requise lorsque RequireExplicitConsent = false.
- Définition et paramètres participant-facing figés à l’activation.

### Definition of Done

- Configurer consentement et participation validé par les tests et la documentation.

---

## SL-082 — Créer les snapshots à l’acceptation

### Description

Créer DemographicSnapshot et, lorsque requis, ConsentSnapshot atomiquement avec la Participation.

### Critères d'acceptation

- Snapshot démographique issu du ParticipantProfile courant.
- MAP explicitement exclu du snapshot.
- ConsentSnapshot reprend la définition réellement acceptée lorsque RequireExplicitConsent = true.
- Aucun ConsentSnapshot n'est créé lorsque RequireExplicitConsent = false.

### Definition of Done

- Créer les snapshots à l’acceptation validé par les tests et la documentation.

---

## SL-083 — Gérer le cycle de Participation

### Description

Implémenter Accepted, Started, Completed, Abandoned et Expired.

### Critères d'acceptation

- Transitions non définies refusées.
- Fermeture du navigateur ≠ abandon.
- Fin de Campaign expire les Participations inachevées selon règle métier.

### Definition of Done

- Gérer le cycle de Participation validé par les tests et la documentation.

---

## SL-084 — Créer Submission et rendre le CampaignForm

### Description

Créer l’unique Submission Draft au commencement et afficher les Questions historiques.

### Critères d'acceptation

- Une seule Submission par Participation.
- QuestionTypes rendus correctement.
- CampaignForm figé utilisé comme source.

### Definition of Done

- Créer Submission et rendre le CampaignForm validé par les tests et la documentation.

---

## SL-085 — Enregistrer les Responses

### Description

Autosauvegarder les valeurs validées tant que Submission est Draft.

### Critères d'acceptation

- Une Response par CampaignFormQuestion.
- Valeurs conformes au type historique.
- Échec de sauvegarde explicite sans perdre les valeurs locales non confirmées.

### Definition of Done

- Enregistrer les Responses validé par les tests et la documentation.

---

## SL-086 — Soumettre atomiquement

### Description

Valider les Questions Required puis soumettre Submission et compléter Participation.

### Critères d'acceptation

- SubmissionSubmitted verrouille Submission et Responses.
- Participation passe à Completed dans la même transaction.
- Données disponibles pour Results et Analytics.

### Definition of Done

- Soumettre atomiquement validé par les tests et la documentation.

---

## SL-087 — Finaliser l’activation de Campaign

### Description

Rendre Draft → Active réellement utilisable avec tous les prérequis MVP.

### Critères d'acceptation

- Project Active, Build Active du même Project, CampaignForm valide, Context, settings et consentement valides.
- Activation fige Build, Form et informations participant-facing.
- Active → Completed est une action explicite de fin de collecte.
- Completed → Archived disponible ; Archived terminal.

### Definition of Done

- Finaliser l’activation de Campaign validé par les tests et la documentation.

---

## SL-088 — Finaliser Participation Flow

### Description

Sécuriser les accès, tests, abandon et expiration.

### Critères d'acceptation

- Un Participant ne voit que ses Participations.
- Une Campaign non Active ne reçoit rien.
- Transactions et immutabilité couvertes.

### Definition of Done

- Finaliser Participation Flow validé par les tests et la documentation.

---

# Fin de Milestone

Une étude réelle peut être activée, rejoindre des Participants et produire des Submissions historiques immuables.

---

# M08 — Results Review

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M07
**Milestone suivante :** M09

---

# Objectif

Permettre aux membres autorisés de consulter fidèlement CampaignParticipations, Submissions et Responses sans créer d’agrégat analytique.

# Valeur produit

Les chercheurs peuvent auditer chaque donnée collectée, reconstruire son contexte historique et préparer l’exploration analytique.

# Domaines concernés

- Results
- Participation Document interne
- Submission Review
- Response Review

# Hors scope

- Agrégations multi-Campaigns
- SavedAnalysis
- Exports
- Modification des données historiques

# Principes

- Results est limité à une Campaign.
- Seules les données historiques persistées sont affichées.
- Une Question peut posséder plusieurs bindings historiques.
- Results ne devient jamais une source de vérité distincte.
- Identité et snapshots respectent les Permissions.

# Scénario d'acceptation

Ouvrir Results d’une Campaign
↓
Filtrer les Participations
↓
Ouvrir une Participation
↓
Consulter Submission et Responses
↓
Voir Question, bindings, Build, contexte et snapshots historiques

# Specs

---

## SL-089 — Introduire Results

### Description

Créer la projection read-only des données d’une Campaign.

### Critères d'acceptation

- Active, Completed et Archived consultables.
- Aucune donnée calculée persistée.

### Definition of Done

- Introduire Results validé par les tests et la documentation.

---

## SL-090 — Créer la navigation Results

### Description

Ajouter la tab Results au Campaign Workspace.

### Critères d'acceptation

- Liste des CampaignParticipations avec recherche et filtres opérationnels.
- États Accepted, Started, Completed, Abandoned et Expired distingués.

### Definition of Done

- Créer la navigation Results validé par les tests et la documentation.

---

## SL-091 — Construire le Participation Document interne

### Description

Afficher chronologie, snapshots et accès à la Submission.

### Critères d'acceptation

- AcceptedAt, StartedAt, CompletedAt, AbandonedAt ou ExpiredAt correctement distingués.
- DemographicSnapshot et ConsentSnapshot protégés par Permissions.

### Definition of Done

- Construire le Participation Document interne validé par les tests et la documentation.

---

## SL-092 — Consulter la Submission

### Description

Afficher statut, durée, dates et complétude.

### Critères d'acceptation

- Draft/Abandoned visibles opérationnellement mais non confondus avec Submitted.
- Immutabilité explicitée.

### Definition of Done

- Consulter la Submission validé par les tests et la documentation.

---

## SL-093 — Consulter les Responses historiques

### Description

Afficher valeur, CampaignFormQuestion et tous ses bindings copiés.

### Critères d'acceptation

- QuestionType et configuration historiques visibles.
- Measures, modes, directions et poids exacts accessibles.
- Verbatims lisibles et recherchables dans la Campaign.

### Definition of Done

- Consulter les Responses historiques validé par les tests et la documentation.

---

## SL-094 — Finaliser Results Review

### Description

Sécuriser performances, navigation et tests.

### Critères d'acceptation

- Aucune mutation depuis Results.
- Drill-down traçable.
- Point d’ouverture contextuelle vers Analysis Workspace préparé.

### Definition of Done

- Finaliser Results Review validé par les tests et la documentation.

---

# Fin de Milestone

Les preuves individuelles sont consultables et traçables. M09 peut calculer des projections sans perdre l’accès aux sources.

---

# M09 — Analysis Workspace

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M08
**Milestone suivante :** M10

---

# Objectif

Implémenter le moteur analytique déterministe et l’Analysis Workspace en surcouche context-aware, avec sauvegarde de configurations au niveau Project.

# Valeur produit

SignalLab délivre sa valeur principale : analyser Questions et Measures dans le temps, comparer Campaigns et Builds, relier quantitatif et qualitatif et conserver une traçabilité méthodologique complète.

# Domaines concernés

- Analytics
- Analysis
- AnalysisScope
- AnalysisGroup
- SavedAnalysis
- AnalyticalEngineVersion
- Visualisations MVP

# Hors scope

- ResearchBoard
- Notes
- IA/NLP
- Dashboards libres
- Exports
- Régression
- Pondération de population
- Snapshots de résultats

# Principes

- L’Analysis Workspace est une surcouche, pas une Context Navigation ni une tab de Campaign.
- Ouverture depuis Project, Campaign ou Results préremplit un Scope modifiable.
- SavedAnalysis appartient au Project et est listée dans sa tab Analyses.
- Seules les Submissions Submitted contribuent.
- Score de Measure calculé par Submission avant agrégation.
- Aucune valeur manquante imputée.
- Aucun résultat analytique persisté.
- Chaque formule est déterministe et versionnée.

# Scénario d'acceptation

Depuis Campaign Results
↓
Ouvrir le bouton Analytics flottant
↓
Scope prérempli avec la Campaign
↓
Ajouter d’autres Campaigns du Project
↓
Configurer groupes, filtres, Questions et Measures
↓
Explorer statistiques, graphiques et verbatims
↓
Sauvegarder
↓
Retrouver la SavedAnalysis dans Project > Analyses

# Specs

---

## SL-095 — Créer les contrats Analytics

### Description

Implémenter AnalysisScope, AnalysisGroup, SavedAnalysis et AnalyticalEngineVersion.

### Critères d'acceptation

- Scope = un Project + une ou plusieurs Campaigns du Project.
- SavedAnalysis ne stocke que configuration.
- Groups et filtres sérialisables et validés.

### Definition of Done

- Créer les contrats Analytics validé par les tests et la documentation.

---

## SL-096 — Intégrer les Analyses au Project

### Description

Ajouter la tab Analyses et le bouton Analytics flottant aux contextes compatibles.

### Critères d'acceptation

- SavedAnalysis listées dans Project Explorer.
- Bouton disponible depuis Project, Campaign et Results selon Permissions.
- Aucun nouvel échelon de contexte.

### Definition of Done

- Intégrer les Analyses au Project validé par les tests et la documentation.

---

## SL-097 — Ouvrir une Analysis context-aware

### Description

Créer une Analysis temporaire dans une surcouche.

### Critères d'acceptation

- Project initialise toutes ou aucune Campaign selon action.
- Campaign et Results initialisent la Campaign courante.
- Scope reste modifiable dans le même Project.
- Fermeture restaure le contexte sous-jacent.

### Definition of Done

- Ouvrir une Analysis context-aware validé par les tests et la documentation.

---

## SL-098 — Construire l’Analysis Workspace

### Description

Créer les régions Subjects, Scope, Groups, Filters, Visualization et Sources.

### Critères d'acceptation

- État temporaire conservé pendant la surcouche.
- Calcul annulable et feedback de progression.
- Aucune formule configurable par l’utilisateur.

### Definition of Done

- Construire l’Analysis Workspace validé par les tests et la documentation.

---

## SL-099 — Configurer Scope, groupes et filtres

### Description

Implémenter les populations comparables du MVP.

### Critères d'acceptation

- Filtres Campaign, Build, date, Response, MeasureScore, DemographicSnapshot et couverture.
- Build résolu via Campaign.
- Filtre Participant individuel absent de l’analyse principale.
- MAP branché en M10.

### Definition of Done

- Configurer Scope, groupes et filtres validé par les tests et la documentation.

---

## SL-100 — Analyser les Questions et preuves qualitatives

### Description

Implémenter les projections par famille analytique.

### Critères d'acceptation

- Scalar/Ordinal : moyenne, médiane, quartiles, écart-type, distribution et percentiles.
- Categorical/Binary : effectifs et proportions.
- MultiCategorical : taux de sélection et nombre moyen d’options.
- Textual : liste, recherche, filtres et accès source.

### Definition of Done

- Analyser les Questions et preuves qualitatives validé par les tests et la documentation.

---

## SL-101 — Calculer les scores de Measure

### Description

Implémenter normalisation, direction, poids, couverture et diagnostic composite.

### Critères d'acceptation

- Normalisation 0–100 structurelle.
- Aligned/Inverted appliqué depuis binding historique.
- Weighted mean par Submission, puis agrégation population.
- MeasureScore et OutcomeScore distincts.
- ExpectedWeight, AnsweredWeight et WeightCoverage exposés.
- Cohérence composite diagnostiquée sans retirer automatiquement une Question.

### Definition of Done

- Calculer les scores de Measure validé par les tests et la documentation.

---

## SL-102 — Comparer et calculer les statistiques avancées

### Description

Implémenter comparaisons descriptives déterministes.

### Critères d'acceptation

- Différences de moyenne, médiane et proportion.
- Spearman entre Measures avec paires valides.
- Hedges g entre groupes quantitatifs.
- Intervalles d’incertitude 95 % versionnés.
- Corrélation explicitement non causale.

### Definition of Done

- Comparer et calculer les statistiques avancées validé par les tests et la documentation.

---

## SL-103 — Implémenter les douze familles visuelles

### Description

Fournir les visualisations opinionated du Domain Model.

### Critères d'acceptation

- Vue détaillée Measure : score, histogramme, box plot, cumulative.
- Comparaison Campaigns/Builds : barres, dumbbell, slope, évolution.
- Profil multi-Measures et heatmaps.
- Contributions/poids, preuves qualitatives et couverture.
- Scatter, matrice Spearman, forest plot.
- Graphe Question–Measure.
- Emplacements MAP désactivés jusqu’à M10.
- Le moteur ne propose que des vues compatibles.

### Definition of Done

- Implémenter les douze familles visuelles validé par les tests et la documentation.

---

## SL-104 — Sauvegarder, recalculer et tracer

### Description

Implémenter SavedAnalysis et drill-down vers les sources.

### Critères d'acceptation

- Sauvegarde Scope, sujets, groupes, filtres, métriques et visualisations.
- Réouverture recalcule avec les données actuelles.
- CalculatedAt et version moteur affichés.
- Drill-down jusqu’aux bindings, Responses, Submissions et Participations.

### Definition of Done

- Sauvegarder, recalculer et tracer validé par les tests et la documentation.

---

## SL-105 — Sécuriser et finaliser Analysis Workspace

### Description

Valider permissions, confidentialité, performance et exactitude.

### Critères d'acceptation

- ViewAnalytics/CreateSavedAnalysis appliquées.
- Identités individuelles protégées.
- Budgets de Scope et annulation définis.
- Jeux de données de référence testent toutes les formules et visualisations.
- ResearchBoard absent du MVP.

### Definition of Done

- Sécuriser et finaliser Analysis Workspace validé par les tests et la documentation.

---

# Fin de Milestone

Le MVP possède un moteur analytique professionnel, longitudinal et traçable. Le futur Research Workspace pourra organiser ces preuves sans modifier leur calcul.

---

# M10 — MAP Profile

**Version :** 3.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 01/08/2026
**Milestone précédente :** M09
**Milestone suivante :** M11

---

# Objectif

Implémenter l’instrument MAP versionné, le profil motivationnel vivant du User et sa segmentation confidentielle dans les Analyses.

# Valeur produit

SignalLab peut expliquer comment les expériences diffèrent selon neuf dimensions motivationnelles sans typologie exclusive ni snapshot dans les études.

# Domaines concernés

- MAPModelVersion
- MAPAssessment
- MAPProfile
- MAPProfileSharing
- Segmentation MAP

# Hors scope

- Norme francophone validée
- Z-scores sans référence versionnée
- Types de joueur
- Accès aux réponses brutes par les Organizations
- Snapshot MAP

# Principes

- 34 items, neuf dimensions et échelle 1–7 selon MAP-V1-FR-SL.
- Profil privé par défaut.
- Dernier assessment Submitted remplace atomiquement le profil actuel.
- Le choix de partage est conservé lors d’un recalcul.
- Analyses utilisent le profil courant partagé au moment du calcul.
- Absent, privé et incompatible sont indistinguables pour l’Organization.
- Un segment n’agrège qu’une MAPModelVersion compatible.

# Scénario d'acceptation

Ouvrir son profil MAP
↓
Compléter 34 items
↓
Soumettre atomiquement
↓
Calculer neuf scores
↓
Choisir SharedForResearch
↓
Ouvrir une SavedAnalysis
↓
Segmenter par dimension MAP courante

# Specs

---

## SL-106 — Créer le domaine MAP

### Description

Créer MAPModelVersion, MAPAssessment, MAPProfile et Sharing.

### Critères d'acceptation

- Ownership User.
- Aucune réponse brute accessible aux Organizations.
- Cycles Draft, Submitted et Abandoned.

### Definition of Done

- Créer le domaine MAP validé par les tests et la documentation.

---

## SL-107 — Publier MAP-V1-FR-SL

### Description

Intégrer les 34 items, neuf dimensions et règles de calcul.

### Critères d'acceptation

- Version publiée immuable.
- Une version active pour nouveaux assessments.
- Aucun item inversé.
- Aucune migration automatique.

### Definition of Done

- Publier MAP-V1-FR-SL validé par les tests et la documentation.

---

## SL-108 — Construire le MAP Assessment

### Description

Créer le Document personnel de réponse.

### Critères d'acceptation

- 34 réponses obligatoires 1–7.
- Autosave en Draft.
- Reprise de la même version.

### Definition of Done

- Construire le MAP Assessment validé par les tests et la documentation.

---

## SL-109 — Calculer atomiquement le profil

### Description

Valider, calculer neuf moyennes et remplacer le profil courant.

### Critères d'acceptation

- Précision persistée au moins quatre décimales.
- Aucun arrondi intermédiaire.
- Échec laisse Assessment Draft et ancien profil intact.

### Definition of Done

- Calculer atomiquement le profil validé par les tests et la documentation.

---

## SL-110 — Consulter et partager le profil

### Description

Créer la vue personnelle et le contrôle Private/SharedForResearch.

### Critères d'acceptation

- Private par défaut.
- Scores 1–7 sans seuils faible/moyen/élevé.
- Pas de score global ni type de joueur.
- Partage conservé après nouvel assessment.

### Definition of Done

- Consulter et partager le profil validé par les tests et la documentation.

---

## SL-111 — Segmenter les Analyses avec MAP

### Description

Activer les capacités MAP prévues par M09.

### Critères d'acceptation

- Filtres inférieur, supérieur et intervalle par dimension.
- Groupes MAP comparables.
- Heatmap, distributions, scatter dimension–Measure et forest plot.
- Population utilisable et indisponible affichées sans motif individuel.

### Definition of Done

- Segmenter les Analyses avec MAP validé par les tests et la documentation.

---

## SL-112 — Protéger la confidentialité MAP

### Description

Appliquer résolution dynamique et indistinction.

### Critères d'acceptation

- Désactivation retire immédiatement le profil des recalculs.
- Aucun score copié dans Participation, Submission, Response ou SavedAnalysis.
- Une seule MAPModelVersion par segmentation.

### Definition of Done

- Protéger la confidentialité MAP validé par les tests et la documentation.

---

## SL-113 — Finaliser MAP Profile

### Description

Stabiliser questionnaire, calculs, UX et tests.

### Critères d'acceptation

- Conformité avec 07 - MAP Form.
- Tests des 34 items et neuf formules.
- Standardisation z-score explicitement non activée sans référence versionnée.

### Definition of Done

- Finaliser MAP Profile validé par les tests et la documentation.

---

# Fin de Milestone

Le cycle MVP complet est fonctionnel : collecte, Results, analyses longitudinales et segmentation motivationnelle confidentielle.

---

# M11 — Deployment and Operations

**Version :** 2.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M10 — MAP Profile  
**Milestone suivante :** M12 — Documentation and Portfolio

---

> Cette Milestone prépare la version fonctionnelle complète de SignalLab à une utilisation réelle.
>
> Elle valide son déploiement, son exploitation, sa sécurité opérationnelle et sa capacité de restauration.
>
> Elle n'introduit aucune nouvelle fonctionnalité métier.

---

# Objectif

Permettre à SignalLab d'être :

- déployé ;
- configuré ;
- exploité ;
- supervisé ;
- sauvegardé ;
- restauré ;
- mis à jour de manière reproductible.

---

# Valeur produit

À la fin de cette Milestone, SignalLab peut être utilisé dans un environnement de production par des Organizations et Participants réels.

---

# Hors scope

Cette Milestone ne contient pas :

- nouvelles fonctionnalités métier ;
- évolution du Domain Model ;
- nouvelles interfaces produit ;
- hébergement de Builds ;
- stockage d'artefacts binaires ;
- distribution ou exécution de Builds.

---

# Principes

- Les environnements sont reproductibles.
- Les secrets sont externalisés.
- PostgreSQL reste la source de vérité persistée.
- Les métadonnées des Builds sont stockées comme les autres données métier.
- Aucun artefact de Build n'est pris en charge dans le MVP.
- Les déploiements sont automatisés et réversibles.
- Les sauvegardes et restaurations sont testées.
- L'observabilité ne doit jamais exposer de données sensibles.

---

# Scénario d'acceptation

Une version validée est fusionnée.

↓

La CI exécute les contrôles.

↓

L'application est construite.

↓

La version est déployée en production.

↓

Les migrations sont appliquées de manière contrôlée.

↓

Le Frontend et le Backend sont accessibles.

↓

Les données existantes sont conservées.

↓

Une restauration testée permet de récupérer la plateforme en cas d'incident.

---

# Specs

---

## SL-114 — Préparer les environnements finaux

### Description

Finaliser les configurations Development, Staging et Production pour l'application complète.

Cette Spec prolonge les conventions introduites dans M01 sans les redéfinir.

### Objectif produit

Garantir un comportement cohérent jusqu'à la production.

### Objectif technique

Valider les configurations, variables d'environnement, URLs et dépendances de chaque environnement.

### Critères d'acceptation

Les trois environnements sont définis et documentés.

Aucun secret n'est présent dans le repository.

Les configurations nécessaires sont injectées correctement.

### Definition of Done

Les environnements finaux sont opérationnels.

---

## SL-115 — Déployer l'infrastructure de production

### Description

Déployer les composants nécessaires au fonctionnement du MVP :

- Frontend ;
- API ASP.NET Core ;
- PostgreSQL ;
- services techniques strictement nécessaires ;
- terminaison HTTPS.

### Objectif produit

Rendre la plateforme complète accessible.

### Objectif technique

Créer et documenter l'infrastructure de production.

### Critères d'acceptation

Tous les composants requis sont déployés.

Le Frontend communique avec l'API.

L'API communique avec PostgreSQL.

Les communications publiques utilisent HTTPS.

### Definition of Done

L'infrastructure de production est opérationnelle.

---

## SL-116 — Valider la persistance des métadonnées des Builds

### Description

Valider la création, la conservation et la récupération en production des métadonnées décrivant les Builds.

Aucun fichier ou artefact binaire n'est stocké par SignalLab dans le MVP.

### Objectif produit

Garantir que les références des Builds restent exploitables dans les Campaigns déployées.

### Objectif technique

Vérifier leur persistance PostgreSQL, leur exposition API et leur conservation entre les déploiements.

### Critères d'acceptation

Les métadonnées d'un Build peuvent être créées et récupérées en production.

Les Campaigns conservent une référence valide vers leur Build.

Les sauvegardes incluent ces métadonnées.

Aucun stockage binaire n'est provisionné pour les Builds.

### Definition of Done

La persistance des métadonnées des Builds est validée.

---

## SL-117 — Sécuriser l'exploitation

### Description

Configurer les mécanismes opérationnels nécessaires à la confidentialité, l'intégrité et la disponibilité de la plateforme.

### Cette Spec comprend notamment

- gestion des secrets ;
- HTTPS et certificats ;
- configuration sécurisée ;
- sauvegardes PostgreSQL ;
- politique de restauration ;
- limitation des accès administratifs ;
- mises à jour des dépendances critiques.

### Objectif produit

Protéger les données des Organizations et des Participants.

### Critères d'acceptation

Les secrets sont externalisés.

Les sauvegardes sont automatisées.

Une restauration est documentée.

Les accès administratifs sont limités.

### Definition of Done

L'exploitation respecte les exigences de sécurité du MVP.

---

## SL-118 — Superviser la plateforme

### Description

Mettre en place l'observabilité nécessaire à l'exploitation de SignalLab.

### Cette Spec comprend notamment

- logs structurés ;
- corrélation ;
- traces ;
- métriques essentielles ;
- health checks ;
- alertes techniques critiques.

### Objectif produit

Détecter et diagnostiquer rapidement les incidents.

### Objectif technique

Configurer l'observabilité prévue par l'architecture sans introduire de plateforme disproportionnée.

### Critères d'acceptation

L'état du Frontend, de l'API et de PostgreSQL est observable.

Les erreurs critiques peuvent être retrouvées grâce à un identifiant de corrélation.

Aucune donnée sensible n'est enregistrée dans les logs.

### Definition of Done

La supervision est opérationnelle.

---

## SL-119 — Automatiser les déploiements finaux

### Description

Étendre la CI/CD de M01 afin de déployer l'application complète de manière contrôlée.

### Cette Spec comprend notamment

- build Backend ;
- build Frontend ;
- tests ;
- lint ;
- migrations contrôlées ;
- déploiement Staging ;
- déploiement Production ;
- stratégie de retour arrière.

### Objectif produit

Réduire les risques liés aux mises en production.

### Critères d'acceptation

Une version validée peut être déployée automatiquement.

Les contrôles bloquants s'exécutent avant le déploiement.

Un échec ne laisse pas la plateforme dans un état incohérent.

Une procédure de retour arrière est disponible.

### Definition of Done

La chaîne de déploiement finale est opérationnelle.

---

## SL-120 — Tester sauvegarde, restauration et mise à jour

### Description

Valider les opérations critiques permettant de maintenir la plateforme dans le temps.

### Objectif produit

Garantir que les données peuvent être conservées et récupérées après un incident ou une mise à jour.

### Objectif technique

Tester sur un environnement représentatif :

- installation complète ;
- migration de version ;
- sauvegarde ;
- restauration ;
- retour arrière compatible.

### Critères d'acceptation

Une installation complète réussit.

Une mise à jour conserve les données.

Une sauvegarde peut être restaurée.

Le comportement après restauration est vérifié.

### Definition of Done

Les procédures opérationnelles critiques sont validées.

---

## SL-121 — Valider le MVP en production

### Description

Exécuter le scénario fonctionnel principal du MVP dans l'environnement de production.

### Parcours validé

Organization

↓

Project

↓

Build metadata

↓

Measure

↓

Form Template

↓

Campaign

↓

Campaign Form

↓

Participation

↓

Submission

↓

Results

↓

Analysis

↓

MAP filtering

### Objectif produit

Garantir que la chaîne complète fonctionne dans les conditions réelles de déploiement.

### Critères d'acceptation

Le parcours principal est exécutable de bout en bout.

Le Frontend et le Backend sont accessibles.

Les données persistent correctement.

Les contrôles d'autorisation restent effectifs.

### Definition of Done

Le MVP est validé en production.

---

## SL-122 — Finaliser Deployment and Operations

### Description

Stabiliser l'ensemble de la Milestone.

Cette Spec comprend :

- corrections ;
- revue de sécurité ;
- revue des coûts et ressources ;
- documentation opérationnelle ;
- validation finale ;
- suppression des éléments inutilisés.

### Objectif produit

Disposer d'une plateforme réellement exploitable.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Les procédures sont documentées.

Les tests de production sont concluants.

Aucune infrastructure de Build hors scope n'est déployée.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M11, SignalLab peut être :

- déployé ;
- exploité ;
- supervisé ;
- sauvegardé ;
- restauré ;
- mis à jour de manière sécurisée.

Le parcours fonctionnel complet du MVP est validé en production sans stockage ni hébergement d'artefacts de Build.

---

# M12 — Documentation and Portfolio

**Version :** 2.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M11 — Deployment and Operations

---

> Cette Milestone finalise SignalLab en tant que produit, projet technique et réalisation de portfolio.
>
> Elle rassemble les éléments nécessaires à sa compréhension, son installation, son exploitation, sa démonstration et sa maintenance.

---

# Objectif

Produire une documentation complète ainsi que les supports permettant de présenter SignalLab dans un contexte académique, professionnel ou technique.

---

# Valeur produit

À la fin de cette Milestone, SignalLab peut être compris et utilisé sans dépendre exclusivement de la connaissance de son développeur initial.

Le projet peut également être présenté clairement dans un portfolio.

---

# Hors scope

Cette Milestone ne contient pas :

- nouvelles fonctionnalités métier ;
- évolution du Domain Model ;
- nouvelle architecture ;
- refonte visuelle ;
- fonctionnalités post-MVP.

---

# Principes

- La documentation décrit l'état réellement livré.
- Les documents produit et techniques conservent des responsabilités distinctes.
- Les fonctionnalités post-MVP sont clairement séparées du MVP.
- Les instructions sont reproductibles.
- Aucun secret ou donnée personnelle n'est publié.

---

# Specs

---

## SL-123 — Rédiger le README principal

### Description

Créer le README principal du repository.

### Contenu

- présentation de SignalLab ;
- problème résolu ;
- périmètre du MVP ;
- principales fonctionnalités ;
- technologies ;
- architecture générale ;
- prérequis ;
- installation locale ;
- lancement ;
- tests ;
- liens vers la documentation détaillée.

### Objectif produit

Présenter rapidement le projet à tout nouvel utilisateur ou contributeur.

### Critères d'acceptation

Le README correspond à l'application réellement livrée.

Les instructions d'installation sont vérifiées depuis un environnement propre.

### Definition of Done

Le README principal est complet et publié.

---

## SL-124 — Finaliser la documentation d'architecture

### Description

Mettre à jour l'ensemble de la documentation d'architecture afin qu'elle corresponde à l'implémentation finale.

### Cette Spec couvre notamment

- architecture Backend ;
- architecture Frontend ;
- Domain Model ;
- navigation ;
- persistance ;
- authentification et autorisation ;
- Data Lifecycle ;
- infrastructure ;
- ADR ;
- diagrammes principaux.

### Objectif produit

Permettre de comprendre les responsabilités et les décisions structurantes de SignalLab.

### Critères d'acceptation

Les diagrammes sont à jour.

Les divergences entre documents sont supprimées ou explicitement justifiées.

Les ADR importantes sont présentes.

### Definition of Done

La documentation d'architecture est complète.

---

## SL-125 — Documenter les APIs

### Description

Finaliser la documentation des endpoints et contrats du Backend.

### Cette Spec comprend notamment

- documentation OpenAPI ;
- authentification requise ;
- paramètres ;
- DTO ;
- réponses ;
- Problem Details ;
- exemples utiles ;
- conventions de pagination et filtrage.

### Objectif produit

Faciliter la maintenance et l'intégration du Frontend.

### Critères d'acceptation

Toutes les APIs du MVP sont documentées.

Les contrats exposés correspondent au comportement réel.

Aucune entité Domain n'est présentée comme contrat public.

### Definition of Done

La documentation API est finalisée.

---

## SL-126 — Rédiger le guide utilisateur

### Description

Créer un guide d'utilisation destiné aux Participants et aux membres des Organizations.

### Parcours couverts

- compte et profil ;
- Organizations ;
- Projects ;
- Builds ;
- Measures ;
- Form Templates ;
- Campaigns ;
- Participation ;
- Results ;
- Analysis ;
- MAP Profile.

### Objectif produit

Permettre une prise en main autonome des fonctionnalités principales.

### Critères d'acceptation

Les parcours essentiels sont documentés.

Les différences entre rôles sont explicites.

Les limitations du MVP sont indiquées.

### Definition of Done

Le guide utilisateur est disponible.

---

## SL-127 — Rédiger le guide d'exploitation

### Description

Documenter l'installation, le déploiement et la maintenance de SignalLab.

### Cette Spec comprend notamment

- environnements ;
- variables de configuration ;
- secrets ;
- déploiement ;
- migrations ;
- sauvegardes ;
- restaurations ;
- supervision ;
- diagnostic ;
- retour arrière.

### Objectif produit

Permettre de maintenir la plateforme sans dépendre d'informations implicites.

### Critères d'acceptation

Les procédures ont été exécutées au moins une fois.

Les responsabilités opérationnelles sont claires.

Aucun secret réel n'est documenté.

### Definition of Done

Le guide d'exploitation est complet.

---

## SL-128 — Préparer le Portfolio

### Description

Créer les supports permettant de présenter SignalLab comme réalisation produit et technique.

### Contenu

- contexte ;
- problème ;
- vision produit ;
- démarche de conception ;
- Domain Model ;
- architecture ;
- choix techniques ;
- parcours principal ;
- fonctionnalités livrées ;
- captures et démonstration ;
- difficultés rencontrées ;
- arbitrages MVP ;
- perspectives post-MVP.

### Objectif produit

Mettre en valeur la cohérence entre recherche utilisateur, conception produit et implémentation technique.

### Critères d'acceptation

Le portfolio distingue clairement MVP et vision long terme.

Les éléments visuels correspondent à la version finale.

Les choix structurants sont expliqués sans exposer d'informations sensibles.

### Definition of Done

Le portfolio est finalisé.

---

## SL-129 — Nettoyer et préparer le repository

### Description

Préparer le repository pour sa conservation ou sa diffusion.

### Cette Spec comprend notamment

- suppression des fichiers temporaires ;
- vérification du `.gitignore` ;
- suppression des secrets et exemples sensibles ;
- harmonisation de l'arborescence ;
- vérification des licences ;
- vérification des scripts ;
- archivage des documents obsolètes lorsque nécessaire.

### Objectif produit

Garantir un projet propre, compréhensible et maintenable.

### Critères d'acceptation

L'arborescence est cohérente.

Aucun secret n'est versionné.

Les commandes documentées fonctionnent.

Les conventions sont respectées.

### Definition of Done

Le repository est prêt à être partagé.

---

## SL-130 — Finaliser SignalLab

### Description

Clôturer officiellement le développement du MVP.

Cette Spec comprend :

- revue fonctionnelle globale ;
- revue des critères du MVP ;
- validation de la documentation ;
- validation du déploiement ;
- validation du repository ;
- validation du portfolio ;
- liste explicite des limitations et évolutions post-MVP.

### Objectif produit

Livrer une version finale cohérente, démontrable et documentée de SignalLab.

### Critères d'acceptation

Toutes les Milestones précédentes sont validées.

Le cycle complet du MVP fonctionne.

La documentation correspond à la version déployée.

Les limitations connues sont documentées.

Le portfolio est prêt.

### Definition of Done

SignalLab MVP est officiellement terminé.

---

# Fin de Milestone

À la fin de M12, SignalLab dispose :

- d'un MVP fonctionnel déployé ;
- d'une documentation produit et technique complète ;
- d'un guide utilisateur ;
- d'un guide d'exploitation ;
- d'un repository propre ;
- d'un portfolio finalisé ;
- d'une séparation explicite entre le MVP livré et la vision long terme.

Le projet SignalLab est officiellement finalisé dans son périmètre MVP.
