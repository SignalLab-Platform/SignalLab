# M09 — Analysis Workspace

**Version :** 3.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M08 — Results Review  
**Milestone suivante :** M10 — MAP Profile

---

> Cette Milestone introduit les capacités analytiques du MVP ainsi que les Saved Analyses.
>
> Les chercheurs peuvent explorer les Responses soumises, les regrouper par Measure, filtrer leur périmètre et comparer plusieurs Campaigns ou Builds d'un même Project.
>
> Une SavedAnalysis mémorise uniquement la définition d'une analyse. Les résultats sont toujours recalculés à partir des données métier actuelles.

---

# Objectif

Permettre aux chercheurs de :

- créer une Analysis ;
- sélectionner son périmètre ;
- explorer les Responses soumises ;
- analyser les Measures ;
- appliquer des filtres ;
- comparer plusieurs Campaigns ou Builds ;
- sauvegarder et rouvrir leur configuration analytique.

---

# Valeur produit

À la fin de cette Milestone, SignalLab permet de transformer les Responses individuelles en résultats comparables dans le temps.

Les Measures assurent la continuité analytique même lorsque les questionnaires ou les formulations de Questions évoluent entre les Campaigns.

---

# Domaines concernés

- Analytics
- SavedAnalysis
- AnalysisScope
- Measure
- Campaign
- Build
- CampaignParticipation
- Submission
- Response

---

# Hors scope

Cette Milestone ne comprend pas :

- MAP, ajouté dans M10 ;
- Research Boards ;
- Notes ;
- commentaires ;
- collaboration temps réel ;
- intelligence artificielle ;
- suggestions automatiques ;
- exports ;
- rapports PDF ;
- analyses multi-Projects ;
- dashboards personnalisables.

---

# Principes métier

- Les Responses contenues dans des Submissions soumises constituent les données sources des analyses.
- Les Submissions fournissent leur regroupement, leur statut et leur contexte historique.
- Les Analytics sont des calculs et non des entités persistantes.
- Une SavedAnalysis ne stocke jamais les résultats calculés.
- Une SavedAnalysis appartient à un seul Project.
- Toutes les Campaigns d'un AnalysisScope appartiennent au même Project.
- Les analyses ne modifient jamais les Campaigns, Participations, Submissions, Responses ou Measures.
- Les résultats sont recalculés à partir des données disponibles lors de chaque exécution.
- Les Responses non soumises ou abandonnées sont exclues des résultats finalisés par défaut.

---

# Architecture UX

Les Saved Analyses sont des ressources du Project Explorer.

```text
Project Explorer

├── Campaigns
├── Builds
└── Analyses
```

L'ouverture d'une SavedAnalysis déclenche une Context Navigation vers l'Analysis Workspace.

Une nouvelle Analysis peut également être créée depuis une action **Analytics** disponible dans les contextes compatibles.

```text
Current Context

↓

Analytics

↓

New Analysis

↓

Analysis Workspace
```

Le contexte courant initialise automatiquement l'AnalysisScope sans modifier les règles d'ownership.

---

# Scénario d'acceptation

Le chercheur ouvre une Campaign.

↓

Il crée une Analysis depuis l'action Analytics.

↓

La Campaign courante initialise le Scope.

↓

Il sélectionne une Measure.

↓

SignalLab agrège les Responses soumises associées à cette Measure.

↓

Il ajoute une autre Campaign du même Project.

↓

Il compare les résultats.

↓

Il sauvegarde l'Analysis.

↓

Il la retrouve ensuite dans le Project Explorer.

---

# Specs

---

## SL-093 — Introduire SavedAnalysis et AnalysisScope

### Description

Introduire les ressources permettant de persister la définition d'une analyse.

SavedAnalysis mémorise notamment :

- son nom ;
- sa description ;
- son Project ;
- son AnalysisScope ;
- ses filtres ;
- ses sélections de Measures et de Questions ;
- sa configuration de comparaison et de présentation.

Elle ne mémorise jamais les résultats calculés.

### Objectif produit

Permettre aux chercheurs de conserver leur travail analytique.

### Objectif technique

Créer SavedAnalysis, AnalysisScope, leur persistance et leurs contrats API.

### Critères d'acceptation

Une SavedAnalysis peut être créée et persistée.

Son Scope ne peut référencer que des Campaigns du même Project.

Aucun résultat analytique n'est stocké dans la ressource.

### Definition of Done

SavedAnalysis et AnalysisScope sont opérationnels.

---

## SL-094 — Ajouter les Analyses au Project Explorer

### Description

Ajouter la catégorie **Analyses** au Project Explorer.

Elle permet de retrouver, rechercher et ouvrir les Saved Analyses appartenant au Project.

### Objectif produit

Centraliser les analyses avec les autres ressources du Project.

### Objectif technique

Intégrer les Saved Analyses au modèle Explorer existant.

### Critères d'acceptation

Les Saved Analyses sont listées.

La recherche et le tri fonctionnent.

Une SavedAnalysis peut être ouverte.

### Definition of Done

La navigation des Analyses est opérationnelle.

---

## SL-095 — Créer une Analysis

### Description

Permettre de créer une nouvelle Analysis depuis :

- le Project Explorer ;
- une Campaign ;
- un contexte Results compatible.

Le contexte courant initialise le Scope lorsque cela est possible.

### Objectif produit

Commencer rapidement une exploration sans reconstruire manuellement son périmètre.

### Objectif technique

Créer le workflow de création contextuelle.

### Critères d'acceptation

Une Analysis peut être créée depuis les contextes prévus.

Le Project propriétaire est déterminé correctement.

Le Scope initial correspond au contexte de création.

### Definition of Done

La création d'Analysis est fonctionnelle.

---

## SL-096 — Construire l'Analysis Workspace

### Description

Créer le Workspace permettant de consulter et modifier une SavedAnalysis.

Le Workspace restaure automatiquement sa configuration et recalcule ses résultats.

### Structure

- Analysis Header ;
- Scope ;
- Measures et Questions ;
- Filters ;
- Comparison ;
- Results.

### Objectif produit

Permettre de reprendre et poursuivre une analyse existante.

### Objectif technique

Créer un Workspace conforme aux conventions UX et au Data Lifecycle.

### Critères d'acceptation

Une SavedAnalysis peut être ouverte directement par URL.

Sa configuration est restaurée.

Les résultats sont recalculés sans être persistés dans la SavedAnalysis.

### Definition of Done

L'Analysis Workspace est opérationnel.

---

## SL-097 — Configurer l'Analysis Scope

### Description

Permettre de sélectionner les Campaigns participant à l'analyse.

Le Scope reste limité à un seul Project dans le MVP.

Les Builds sont déduits des Campaigns sélectionnées et peuvent être utilisés comme dimension de regroupement ou de comparaison.

### Objectif produit

Explorer différentes parties de l'historique d'un Project.

### Objectif technique

Valider et exécuter AnalysisScope.

### Critères d'acceptation

Des Campaigns du Project peuvent être ajoutées ou retirées.

Une Campaign étrangère au Project est refusée.

Toute modification du Scope déclenche le recalcul.

### Definition of Done

AnalysisScope est fonctionnel.

---

## SL-098 — Explorer les Measures et les Responses

### Description

Permettre d'analyser les données collectées selon leur nature.

Pour une Measure, SignalLab agrège les Responses provenant des CampaignFormQuestions qui lui sont associées.

Pour une Question sans Measure, SignalLab permet une exploration limitée à cette Question et à son contexte de Campaign.

### Capacités MVP

- nombre de Responses valides ;
- distribution adaptée au type de Question ;
- agrégations quantitatives pertinentes ;
- consultation des réponses qualitatives ;
- accès aux Participations sources ;
- distinction explicite entre absence de Response et valeur répondue.

### Objectif produit

Transformer les Responses en preuves lisibles sans perdre leur lien avec les données sources.

### Objectif technique

Créer les calculs analytiques par type de Question et par Measure.

### Critères d'acceptation

Les Measures peuvent être sélectionnées.

Les Responses associées sont correctement agrégées.

Les réponses qualitatives restent consultables individuellement.

Chaque résultat permet de retrouver ses données sources.

### Definition of Done

L'exploration analytique est opérationnelle.

---

## SL-099 — Filtrer les données analysées

### Description

Permettre d'affiner une Analysis grâce à des filtres persistés.

### Filtres MVP

- Campaign ;
- Build ;
- période de soumission ;
- Participant ;
- statut finalisé ;
- présence ou absence d'une Response.

Les filtres MAP seront ajoutés dans M10.

### Objectif produit

Explorer un sous-ensemble précis des données sans modifier leur source.

### Objectif technique

Créer un moteur de filtrage composable et limité au Scope de l'Analysis.

### Critères d'acceptation

Les filtres peuvent être combinés.

Les résultats sont recalculés immédiatement.

La configuration des filtres est sauvegardée avec la SavedAnalysis.

### Definition of Done

Le filtrage analytique est opérationnel.

---

## SL-100 — Comparer les Campaigns et les Builds

### Description

Permettre de comparer plusieurs ensembles de données appartenant au même AnalysisScope.

Les comparaisons peuvent notamment opposer :

- plusieurs Campaigns ;
- plusieurs Builds par l'intermédiaire de leurs Campaigns ;
- plusieurs périodes ;
- plusieurs sous-ensembles filtrés.

Les Measures constituent l'axe privilégié pour comparer des questionnaires différents.

### Objectif produit

Mesurer l'évolution de l'expérience entre plusieurs études ou versions du produit.

### Objectif technique

Créer des groupes de comparaison calculés à partir d'un même Scope.

### Critères d'acceptation

Au moins deux groupes peuvent être comparés.

Les groupes respectent le Project et le Scope.

Les résultats indiquent clairement leur population et leur origine.

### Definition of Done

Les comparaisons sont opérationnelles.

---

## SL-101 — Gérer les Saved Analyses

### Description

Permettre de gérer les Saved Analyses d'un Project.

### Fonctionnalités

- renommer ;
- modifier la description ;
- rouvrir ;
- dupliquer la configuration ;
- supprimer.

La duplication crée une nouvelle SavedAnalysis indépendante et ne copie aucun résultat calculé.

### Objectif produit

Permettre aux équipes d'organiser leur travail analytique.

### Objectif technique

Implémenter les opérations de gestion et leurs invalidations de cache.

### Critères d'acceptation

Les opérations sont persistées.

Une duplication possède une nouvelle identité.

La suppression d'une SavedAnalysis ne modifie aucune donnée source.

### Definition of Done

La gestion des Saved Analyses est fonctionnelle.

---

## SL-102 — Sécuriser l'Analysis Workspace

### Description

Garantir que seules les données et Saved Analyses autorisées puissent être consultées ou modifiées.

### Objectif produit

Préserver l'isolation des Organizations et des Projects.

### Objectif technique

Appliquer les contrôles d'autorisation à toutes les Queries et Commands analytiques.

### Critères d'acceptation

Une Campaign étrangère ne peut jamais être injectée dans un Scope.

Les accès non autorisés sont refusés.

Les données individuelles restent protégées selon les autorisations applicables.

### Definition of Done

La sécurité des Analyses est validée.

---

## SL-103 — Finaliser Analysis Workspace

### Description

Stabiliser l'ensemble de la Milestone Analysis Workspace.

Cette Spec comprend :

- corrections ;
- cohérence des calculs ;
- tests sur les agrégations ;
- tests de comparaison ;
- optimisation raisonnable ;
- harmonisation UX ;
- documentation ;
- revue complète.

### Objectif produit

Disposer des capacités analytiques nécessaires au MVP.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Les résultats sont reproductibles à partir des données sources.

Aucun résultat calculé n'est persisté comme source de vérité.

Les tests sont verts.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M09, une équipe peut :

- créer et retrouver des Saved Analyses ;
- définir leur périmètre ;
- analyser des Questions et des Measures ;
- appliquer des filtres ;
- comparer plusieurs Campaigns ou Builds ;
- retrouver les Responses à l'origine d'un résultat ;
- sauvegarder la configuration sans dupliquer les données analysées.

M10 enrichira ces capacités avec le profil MAP actuel et partagé des Participants.