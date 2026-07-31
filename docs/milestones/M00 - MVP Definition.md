# M00 — MVP Definition

**Version :** 1.0  
**Statut :** 🟡 Draft  
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
- responsive design ;
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
| M06       | Participation Flow                |
| M07       | Results Review                    |
| M08       | Analysis Workspace                |
| M09       | Motivation Player Profile         |
| M10       | Deployment and Operations         |
| M11       | Documentation and Portfolio       |

---

# Specs

---

## SL-000 — Geler le périmètre du MVP

### Description

Établir officiellement le périmètre fonctionnel du MVP.

### Critères d'acceptation

- Le périmètre du MVP est validé.
- Les fonctionnalités hors scope sont identifiées.
- Les Milestones du projet sont définies.

---

## SL-001 — Définir le glossaire métier

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

### Description

Définir les responsabilités des différents types d'utilisateurs du MVP.

- Participant
- Organization Member
- Organization Administrator
- Organization Owner

---

## SL-003 — Définir les états métier principaux

### Description

Définir les états de vie des principales entités.

Notamment :

- Campaign
- Participation
- Build

---

## SL-004 — Définir les règles d'immutabilité

### Description

Identifier quelles données peuvent évoluer et lesquelles deviennent définitivement immuables après certaines actions métier.

---

## SL-005 — Définir les règles MAP

### Description

Formaliser les règles concernant :

- création ;
- partage ;
- utilisation dans les analyses ;
- confidentialité.

---

## SL-006 — Définir la stratégie Build

### Description

Formaliser le rôle du Build dans SignalLab.

Définir ce qui appartient au MVP et ce qui relève de la vision produit.

---

## SL-007 — Définir le périmètre des analyses

### Description

Identifier précisément les capacités analytiques couvertes par le MVP.

---

## SL-008 — Valider officiellement le MVP

### Description

La validation de cette Spec marque le début du développement logiciel.

À partir de cette étape, toutes les Milestones suivantes peuvent être implémentées.