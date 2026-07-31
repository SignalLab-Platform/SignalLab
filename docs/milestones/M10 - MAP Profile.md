# M10 — MAP Profile

**Version :** 2.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M09 — Analysis Workspace  
**Milestone suivante :** M11 — Deployment and Operations

---

> Cette Milestone introduit le profil MAP de SignalLab.
>
> Chaque User peut créer et maintenir un unique profil MAP actuel.
>
> Le profil est vivant : les Analyses utilisent toujours sa valeur actuelle et les Participations ne conservent jamais de snapshot MAP.
>
> Le propriétaire contrôle librement son partage avec les Organizations réalisant les analyses.

---

# Objectif

Permettre à un User de :

- réaliser un assessment MAP ;
- obtenir son profil MAP ;
- consulter son profil actuel ;
- le mettre à jour en réalisant un nouvel assessment ;
- activer ou désactiver son partage ;
- enrichir les Analyses lorsqu'il autorise ce partage.

---

# Valeur produit

À la fin de cette Milestone, les chercheurs peuvent segmenter les Results selon les motivations actuelles des Participants, sans dupliquer ces informations dans les Campaigns ou les Participations.

---

# Domaines concernés

- MAPModelVersion
- MAPAssessment
- MAPProfile
- ParticipantProfile
- Analytics

---

# Hors scope

Cette Milestone ne contient pas :

- modèle MAP personnalisable par Organization ;
- plusieurs profils MAP actifs par User ;
- snapshot MAP dans les Participations ;
- recommandations personnalisées ;
- matching automatique ;
- segmentation automatique par intelligence artificielle ;
- export avancé des profils ;
- comparaison historique des anciens profils dans les Analyses.

---

# Principes métier

- SignalLab définit et maintient le modèle MAP.
- Les Organizations ne peuvent pas modifier sa définition.
- Chaque User possède au maximum un profil MAP actuel.
- Un profil MAP est calculé à partir d'un assessment explicite du User.
- Chaque calcul référence la version du modèle utilisée.
- Un nouvel assessment remplace le profil MAP actuel du User.
- Les Analyses utilisent toujours le profil MAP actuel au moment du calcul.
- Une CampaignParticipation, Submission ou Response ne stocke jamais de snapshot MAP.
- Le partage est contrôlé par le propriétaire du profil.
- La désactivation du partage retire immédiatement le profil des futurs calculs analytiques.
- Les Responses déjà soumises restent conservées et inchangées.

---

# Intégration dans l'Application

Le profil MAP est géré depuis l'espace personnel du User.

```text
Personal Hub

└── MAP Profile
    ├── Assessment
    ├── Current Profile
    └── Sharing
```

L'Analysis Workspace est enrichi par des filtres MAP lorsque les profils concernés sont partagés.

---

# Scénario d'acceptation

Un User ouvre son MAP Profile.

↓

Il réalise l'assessment actif.

↓

SignalLab calcule son profil actuel.

↓

Le User active le partage.

↓

Une Analysis incluant ses Responses peut utiliser son profil MAP actuel.

↓

Le User désactive le partage.

↓

Les nouveaux calculs analytiques n'utilisent plus son profil, tandis que ses Responses restent inchangées.

---

# Specs

---

## SL-104 — Introduire le domaine MAP

### Description

Introduire les entités nécessaires au profil MAP :

- MAPModelVersion ;
- MAPAssessment ;
- MAPProfile.

### Objectif produit

Disposer d'un modèle motivationnel commun à l'ensemble de SignalLab.

### Objectif technique

Créer les entités, relations, invariants et configurations de persistance.

### Critères d'acceptation

Le domaine MAP est créé.

Un User ne peut posséder qu'un seul profil actuel.

Chaque profil référence la version du modèle utilisée pour son calcul.

### Definition of Done

Le domaine MAP est opérationnel.

---

## SL-105 — Gérer les versions du modèle MAP

### Description

Permettre à SignalLab de maintenir les versions successives de la définition MAP.

Une version publiée est immuable.

Une seule version est active pour les nouveaux assessments à un instant donné.

### Objectif produit

Garantir que chaque profil reste interprétable selon le modèle utilisé.

### Objectif technique

Créer le versionnement et la sélection de la version active.

### Critères d'acceptation

Une version peut être publiée.

Une version publiée ne peut pas être modifiée.

Les assessments utilisent la version active au moment de leur création.

### Definition of Done

Le versionnement MAP est opérationnel.

---

## SL-106 — Construire le MAP Assessment

### Description

Créer le parcours permettant au User de compléter l'assessment MAP depuis son Personal Hub.

Le MAP Assessment est distinct des Campaign Forms et n'appartient à aucune Organization.

### Objectif produit

Permettre au User de créer ou mettre à jour son profil motivationnel.

### Objectif technique

Construire le formulaire, la validation et la soumission d'un assessment.

### Critères d'acceptation

Le User peut ouvrir et compléter l'assessment actif.

Les réponses sont validées.

Un assessment soumis ne peut pas être attribué à un autre User.

### Definition of Done

Le MAP Assessment est opérationnel.

---

## SL-107 — Calculer et mettre à jour le profil MAP

### Description

Calculer de manière déterministe le MAPProfile à partir d'un MAPAssessment soumis.

Lorsqu'un User réalise un nouvel assessment valide, le résultat devient son profil MAP actuel.

Les analyses futures utilisent ce nouveau profil.

### Objectif produit

Maintenir une représentation actuelle des motivations du User.

### Objectif technique

Implémenter le moteur de calcul et la mise à jour atomique du profil courant.

### Critères d'acceptation

Le même assessment et la même version produisent le même résultat.

Le profil courant est créé ou remplacé atomiquement.

Aucune Participation n'est modifiée lors de cette mise à jour.

### Definition of Done

Le calcul et la mise à jour du profil MAP sont opérationnels.

---

## SL-108 — Consulter et gérer le partage du profil MAP

### Description

Créer la section MAP Profile du Personal Hub.

Elle permet de consulter le profil actuel et de contrôler son partage.

### Fonctionnalités

- visualisation du profil ;
- version du modèle ;
- date du dernier assessment ;
- nouvel assessment ;
- activation du partage ;
- désactivation du partage.

### Objectif produit

Donner au User la maîtrise de son profil et de sa confidentialité.

### Critères d'acceptation

Le profil courant est lisible.

Le User peut modifier son choix de partage à tout moment.

Le changement est persisté immédiatement.

### Definition of Done

La consultation et le partage sont opérationnels.

---

## SL-109 — Exploiter MAP dans les Analyses

### Description

Enrichir l'Analysis Workspace avec les dimensions et filtres MAP.

Seuls les profils actuellement partagés sont utilisés.

Les profils sont résolus au moment de chaque calcul et ne sont jamais copiés dans la SavedAnalysis ou les données de Participation.

### Fonctionnalités MVP

- filtrer selon les dimensions MAP ;
- créer des segments MAP ;
- comparer des segments ;
- afficher la population incluse et les profils indisponibles.

### Objectif produit

Comprendre comment les motivations actuelles des Participants influencent les Results.

### Critères d'acceptation

Les filtres MAP sont disponibles dans une Analysis.

La désactivation du partage retire le profil des recalculs suivants.

Les Responses du Participant restent présentes dans les analyses non segmentées par MAP selon les règles d'accès applicables.

Aucun snapshot MAP n'est persisté.

### Definition of Done

MAP est exploitable dans les Analyses.

---

## SL-110 — Sécuriser les données MAP

### Description

Appliquer les règles de confidentialité et d'autorisation au domaine MAP.

### Principes

- le User contrôle son propre profil ;
- les Organizations ne consultent que les profils partagés et nécessaires à leurs analyses ;
- aucune donnée MAP n'est exposée hors d'un contexte autorisé ;
- le choix de partage est respecté côté Backend.

### Critères d'acceptation

Un User ne peut modifier que son propre profil et son propre partage.

Un profil masqué n'est pas retourné aux analyses.

Les accès non autorisés sont refusés.

### Definition of Done

La sécurité MAP est validée.

---

## SL-111 — Finaliser MAP Profile

### Description

Stabiliser l'ensemble de la Milestone MAP Profile.

Cette Spec comprend :

- corrections ;
- validation du calcul ;
- tests de confidentialité ;
- tests d'intégration avec Analysis ;
- harmonisation UX ;
- documentation ;
- revue complète.

### Objectif produit

Disposer d'un profil MAP fiable, vivant et contrôlé par son propriétaire.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Les analyses utilisent toujours le profil actuel et partagé.

Aucun snapshot MAP n'est stocké dans les Participations.

Les tests sont verts.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M10, SignalLab permet à chaque User de :

- réaliser un assessment MAP ;
- obtenir un profil actuel ;
- le mettre à jour ;
- le consulter ;
- contrôler son partage.

Les chercheurs peuvent utiliser les profils actuellement partagés pour filtrer et comparer leurs Analyses, sans modifier ni dupliquer les données historiques des Campaigns.