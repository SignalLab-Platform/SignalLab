# M05 — Build Management

**Version :** 2.0
**Statut :** 🟡 Draft
**Dernière mise à jour :** 31/07/2026
**Milestone précédente :** M04 — Campaign Workspace
**Milestone suivante :** M06 — Measures, Forms and Response Model

---

> Cette Milestone introduit la gestion des Builds.
>
> Elle enrichit la section **Build** du Campaign Workspace.
>
> Les Builds représentent les exécutables utilisés pendant une Campaign.

---

# Objectif

Permettre aux équipes de gérer les Builds d'un Project.

Les chercheurs peuvent :

- enregistrer plusieurs Builds ;
- consulter leurs informations ;
- sélectionner le Build utilisé par une Campaign.

Cette Milestone ne traite pas encore l'hébergement des Builds.

---

# Valeur produit

À la fin de cette Milestone, une équipe peut préparer complètement une Campaign en lui associant un Build.

SignalLab devient capable de référencer plusieurs versions d'une application.

---

# Domaines concernés

- Builds

---

# Hors scope

Cette Milestone ne contient pas :

- Hosting
- Upload automatique GitHub
- Browser Builds
- Cloud Storage
- Téléchargement
- Exécution distante

---

# Principes métier

- Un Build appartient à un Project.
- Un Project peut posséder plusieurs Builds.
- Une Campaign référence un Build.
- Plusieurs Campaigns peuvent utiliser le même Build.
- Les Builds sont immuables.
- Les métadonnées d'un Build peuvent être enrichies.

---

# Intégration dans l'Application Shell

Cette Milestone n'introduit aucun nouveau contexte.

Elle enrichit uniquement :

Campaign Workspace

↓

Build

---

# Navigation

La section **Build** du Campaign Workspace devient fonctionnelle.

Le Project Explorer continue également de proposer une vue globale des Builds du Project.

---

# Scénario d'acceptation

Créer un Build

↓

Le retrouver dans le Project

↓

Ouvrir une Campaign

↓

Associer le Build

↓

Changer de Build

↓

Consulter ses informations

---

# Specs

---

## SL-057 — Créer le domaine Build

### Description

Introduire le domaine métier Build.

Un Build représente une version distribuable d'une application.

Le Build est indépendant des Campaigns.

Il peut être réutilisé par plusieurs études.

### Objectif produit

Permettre le versionnement des applications testées.

### Objectif technique

Créer le domaine Build.

### Principes impliqués

- Ownership par Project.
- Réutilisation.
- Immutabilité.

### Critères d'acceptation

- Domaine créé.
- Persistance opérationnelle.
- API disponibles.

### Definition of Done

Le domaine Build est opérationnel.

---

## SL-058 — Construire la gestion des Builds

### Description

Créer l'interface principale de gestion des Builds.

Cette vue est accessible depuis :

- Project Explorer
- Campaign Workspace

Les deux utilisent les mêmes composants.

### Cette Spec alimente

Content

- Liste des Builds
- Informations générales

### Objectif produit

Permettre la gestion des Builds.

### Objectif technique

Créer les composants réutilisables.

### Principes impliqués

- Composants partagés.
- Navigation cohérente.
- Aucune duplication.

### Critères d'acceptation

Les Builds sont affichés.

Les composants sont réutilisés.

### Definition of Done

Gestion des Builds disponible.

---

## SL-059 — Enregistrer un Build

### Description

Permettre d'enregistrer un nouveau Build.

Le Build est uniquement décrit par ses métadonnées.

Aucun fichier n'est hébergé.

### Informations

- Nom
- Version
- Plateforme
- Repository
- Branche
- Commit SHA
- Provenance

### Objectif produit

Historiser les versions d'une application.

### Objectif technique

Créer le workflow d'enregistrement.

### Principes impliqués

- Métadonnées uniquement.
- Validation.
- Versionnement.

### Critères d'acceptation

Un Build peut être enregistré.

Il apparaît immédiatement dans le Project.

### Definition of Done

Création opérationnelle.

---

## SL-060 — Consulter un Build

### Description

Permettre de consulter les informations d'un Build.

Cette vue présente uniquement les métadonnées.

### Informations

- Version
- Plateforme
- Repository
- Branche
- Commit
- Provenance
- Date de création

### Objectif produit

Identifier précisément une version.

### Objectif technique

Créer la vue détaillée.

### Principes impliqués

- Métadonnées uniquement.
- Lecture seule.

### Critères d'acceptation

Toutes les informations sont affichées.

### Definition of Done

Consultation disponible.

## SL-061 — Modifier les métadonnées d'un Build

### Description

Permettre de modifier les métadonnées descriptives d'un Build.

Cette Spec ne modifie jamais l'identité du Build.

Les Campaigns continuent de référencer exactement le même Build.

### Informations modifiables

- Nom
- Description
- Tags
- Notes

Les informations d'identification du Build restent inchangées.

### Objectif produit

Permettre aux équipes d'améliorer la lisibilité de leur bibliothèque de Builds.

### Objectif technique

Séparer les métadonnées descriptives de l'identité technique du Build.

### Principes impliqués

- Le Build conserve son identité.
- Les références des Campaigns restent valides.
- Les modifications sont immédiatement visibles.

### Critères d'acceptation

Les métadonnées sont persistées.

Les Campaigns continuent de référencer le même Build.

### Definition of Done

Modification fonctionnelle.

---

## SL-062 — Organiser les Builds

### Description

Créer les outils permettant de retrouver rapidement un Build.

Cette Spec enrichit la vue Builds du Project Explorer.

### Fonctionnalités

- Recherche
- Tri alphabétique
- Tri par version
- Tri par date de création
- Tri par dernière modification

Les filtres avancés seront introduits ultérieurement.

### Objectif produit

Faciliter la gestion de nombreuses versions.

### Objectif technique

Créer une bibliothèque de Builds cohérente avec les autres Explorers.

### Principes impliqués

- Recherche instantanée.
- Tri cohérent.
- Composants réutilisables.

### Critères d'acceptation

Les Builds peuvent être recherchés.

Les tris fonctionnent correctement.

### Definition of Done

Bibliothèque de Builds disponible.

---

## SL-063 — Associer un Build à une Campaign

### Description

Permettre de sélectionner le Build utilisé par une Campaign.

La Campaign référence un Build existant appartenant au même Project.

Aucun Build n'est dupliqué.

### Objectif produit

Préparer une Campaign avec l'application à tester.

### Objectif technique

Créer la relation Campaign → Build.

### Principes impliqués

- Une Campaign référence un Build.
- Plusieurs Campaigns peuvent partager le même Build.
- Le Build n'est jamais copié.

### Critères d'acceptation

Une Campaign peut sélectionner un Build.

Le Build actif est affiché dans le Campaign Workspace.

### Definition of Done

Association fonctionnelle.

---

## SL-064 — Changer le Build d'une Campaign

### Description

Permettre de remplacer le Build associé à une Campaign.

Le changement met simplement à jour la référence vers un autre Build.

### Objectif produit

Faire évoluer une étude vers une nouvelle version de l'application.

### Objectif technique

Mettre à jour la relation Campaign → Build.

### Principes impliqués

- Aucune duplication.
- Mise à jour atomique.
- Historique préservé si le Domain Model le prévoit.

### Critères d'acceptation

Le Build peut être remplacé.

Le nouveau Build devient immédiatement actif.

### Definition of Done

Remplacement fonctionnel.

---

## SL-065 — Sécuriser la gestion des Builds

### Description

Étendre le système de permissions au domaine Build.

Les Builds héritent des permissions du Project.

Cette Spec prépare les futures intégrations (GitHub, Hosting, Browser Builds) sans modifier le modèle actuel.

### Objectif produit

Garantir que seuls les utilisateurs autorisés puissent gérer les Builds.

### Objectif technique

Étendre le système d'autorisation existant.

### Principes impliqués

- Héritage des permissions.
- Contrôles centralisés.
- Aucun contrôle spécifique au Build pour le MVP.

### Critères d'acceptation

Les utilisateurs non autorisés ne peuvent pas modifier les Builds.

Les contrôles d'accès fonctionnent correctement.

### Definition of Done

Permissions validées.

---

## SL-066 — Finaliser Build Management

### Description

Stabiliser l'ensemble du domaine Build.

Cette Spec comprend :

- corrections ;
- refactoring ;
- harmonisation UX ;
- documentation ;
- revue complète ;
- tests.

### Objectif produit

Disposer d'une gestion des Builds stable avant l'arrivée de la Participation.

### Objectif technique

Garantir une base robuste permettant aux prochaines Milestones d'utiliser les Builds sans remettre en cause leur architecture.

### Principes impliqués

- Le domaine Build est désormais figé.
- Les futures Milestones enrichissent son utilisation sans modifier son modèle.
- La documentation reste la référence officielle.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Les tests sont verts.

La documentation est à jour.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M05, une équipe peut :

- enregistrer plusieurs Builds ;
- consulter leurs métadonnées ;
- modifier leurs informations descriptives ;
- organiser une bibliothèque de Builds ;
- associer un Build à une Campaign ;
- remplacer le Build d'une Campaign.

La section **Build** du Campaign Workspace est désormais entièrement fonctionnelle.

Les Milestones suivantes pourront utiliser les Builds comme base des parcours de Participation, sans modifier leur modèle métier.