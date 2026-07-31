# M08 — Results Review

**Version :** 3.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M07 — Participation Flow  
**Milestone suivante :** M09 — Analysis Workspace

---

> Cette Milestone introduit la consultation des données collectées lors des Participations.
>
> Elle permet aux chercheurs d'ouvrir une Participation clôturée, de consulter sa Submission et de lire les Responses produites.
>
> Cette Milestone reste entièrement orientée lecture.
>
> Les agrégations, comparaisons et interprétations analytiques seront introduites dans M09.

---

# Objectif

Permettre aux chercheurs de consulter les résultats individuels d'une Campaign.

Le parcours comprend :

- accès à la section Results ;
- consultation des Participations disponibles ;
- ouverture d'un Participation Document ;
- consultation de la Submission ;
- lecture des Responses dans la structure du Campaign Form ;
- navigation entre les Participations.

---

# Valeur produit

À la fin de cette Milestone, une équipe peut examiner précisément ce qu'un Participant a soumis, sans modifier les données historiques.

---

# Domaines concernés

- Results
- CampaignParticipation
- Submission
- Response
- CampaignForm

---

# Hors scope

Cette Milestone ne contient pas :

- agrégations analytiques ;
- comparaison de Campaigns ;
- segmentation ;
- KPIs ;
- dashboards ;
- annotations ;
- tagging ;
- exports ;
- modification des Participations, Submissions ou Responses.

---

# Principes métier

- Results constitue une couche de consultation et non une nouvelle source de données.
- Les CampaignParticipations, Submissions et Responses restent les sources de vérité.
- Seules les données soumises sont considérées comme des résultats finalisés.
- Une Submission regroupe l'ensemble des Responses d'une tentative.
- Les Responses sont toujours interprétées à partir des CampaignFormQuestions historiques.
- Toutes les données sont accessibles en lecture seule.
- Aucune projection de Results n'est persistée comme donnée métier indépendante.

---

# Architecture UX

Cette Milestone enrichit le Campaign Workspace.

```text
Campaign Workspace

↓

Results

↓

Participation Document

├── Overview
├── Submission
└── Responses
```

Chaque Participation clôturée est consultée sous la forme d'un Document.

---

# Scénario d'acceptation

Une Campaign possède plusieurs Participations clôturées.

↓

Le chercheur ouvre Results.

↓

Il sélectionne une Participation.

↓

Le Participation Document s'ouvre.

↓

Il consulte les informations générales.

↓

Il consulte la Submission.

↓

Il parcourt les Responses regroupées selon le Campaign Form.

---

# Specs

---

## SL-087 — Introduire la consultation Results

### Description

Créer la couche de lecture permettant d'accéder aux Participations et Submissions d'une Campaign.

Results ne crée aucune entité métier persistante.

Il expose uniquement des projections adaptées à la consultation.

### Objectif produit

Rendre les données soumises accessibles aux chercheurs.

### Objectif technique

Créer les Queries et DTO de consultation sans dupliquer les données.

### Principes impliqués

- lecture seule ;
- projections calculées ;
- aucune copie ;
- isolation par Project et Organization.

### Critères d'acceptation

Les Participations accessibles sont récupérables.

Leur Submission soumise et leurs Responses peuvent être chargées.

Aucune donnée Results indépendante n'est persistée.

### Definition of Done

La consultation Results est opérationnelle.

---

## SL-088 — Créer la navigation Results

### Description

Rendre fonctionnelle la section **Results** du Campaign Workspace.

Cette section présente les Participations de la Campaign dont les données peuvent être consultées.

### Fonctionnalités

- liste des Participations ;
- état ;
- Participant ;
- date de soumission ;
- recherche simple ;
- ouverture du Participation Document.

### Objectif produit

Permettre aux chercheurs de retrouver rapidement une Participation.

### Objectif technique

Intégrer Results à la navigation locale existante.

### Critères d'acceptation

La section Results est disponible.

Les Participations consultables sont listées.

Chaque Participation autorisée peut être ouverte.

### Definition of Done

La navigation Results est opérationnelle.

---

## SL-089 — Construire le Participation Document

### Description

Créer le Document de référence pour la consultation d'une Participation.

Il rassemble les informations nécessaires à la compréhension du contexte dans lequel la Submission a été produite.

### Informations affichées

- Participant ;
- Campaign ;
- Project ;
- Build ;
- statut de la Participation ;
- dates principales ;
- statut de la Submission ;
- durée lorsque disponible.

### Objectif produit

Permettre aux chercheurs de comprendre immédiatement le contexte d'une participation individuelle.

### Objectif technique

Créer un Document conforme aux conventions UX.

### Principes impliqués

- une Participation correspond à un Document ;
- lecture seule ;
- contexte historique explicite.

### Critères d'acceptation

Le Document affiche les informations cohérentes avec le Domain Model.

Aucune modification n'est possible.

La navigation directe par URL reconstruit correctement le contexte.

### Definition of Done

Le Participation Document est opérationnel.

---

## SL-090 — Consulter la Submission

### Description

Afficher l'unité complète de collecte associée à la Participation.

La vue Submission présente son état et les métadonnées nécessaires à l'interprétation de l'envoi.

### Informations affichées

- identifiant ;
- statut ;
- date de début ;
- date de soumission ;
- durée ;
- nombre de Responses ;
- progression finale.

### Objectif produit

Distinguer clairement la Participation de la tentative de réponse qu'elle contient.

### Objectif technique

Créer une projection de lecture dédiée à la Submission.

### Critères d'acceptation

La Submission est affichée avec ses métadonnées réelles.

Une Participation sans Submission soumise est représentée sans créer de faux résultat.

### Definition of Done

La consultation de la Submission est opérationnelle.

---

## SL-091 — Consulter les Responses

### Description

Afficher toutes les Responses soumises en respectant la structure historique du Campaign Form.

Les Sections, Questions et réponses sont présentées dans l'ordre réel du formulaire soumis.

### Cette Spec comprend notamment

- regroupement par Section ;
- libellé historique de la Question ;
- valeur répondue ;
- Measure associée lorsqu'elle existe ;
- représentation adaptée au type de Question ;
- état explicite des Questions sans Response lorsque cela est pertinent.

### Objectif produit

Permettre une lecture fidèle de ce que le Participant a soumis.

### Objectif technique

Construire la projection CampaignFormQuestion + Response sans dépendre du Form Template source.

### Critères d'acceptation

Les Responses sont affichées dans le bon ordre.

Chaque valeur est présentée selon son type.

Les informations historiques restent interprétables même si le Form Template ou la Measure évolue ensuite.

### Definition of Done

La consultation des Responses est opérationnelle.

---

## SL-092 — Finaliser Results Review

### Description

Stabiliser l'ensemble de la Milestone Results Review.

Cette Spec comprend :

- corrections ;
- harmonisation UX ;
- optimisation des Queries ;
- tests d'autorisation ;
- tests de lecture historique ;
- documentation ;
- revue complète.

### Objectif produit

Disposer d'une consultation individuelle fiable avant l'introduction des analyses agrégées.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Les données restent strictement en lecture seule.

Les tests sont verts.

La documentation est à jour.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M08, une équipe peut :

- accéder aux Results d'une Campaign ;
- consulter les Participations disponibles ;
- ouvrir un Participation Document ;
- comprendre le contexte de la session ;
- consulter la Submission ;
- lire toutes les Responses selon la structure historique du Campaign Form.

La prochaine Milestone introduira les calculs, filtres et comparaisons à partir de ces données sources.