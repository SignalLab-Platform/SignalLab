# M06 — Measures, Forms and Response Model

**Version :** 1.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M05 — Build Management  
**Milestone suivante :** M07 — Participation Flow

---

> Cette Milestone introduit le vocabulaire analytique et le modèle de collecte de SignalLab.
>
> Elle permet aux Organizations de définir leurs Measures, de construire des Form Templates réutilisables et de générer le Campaign Form propre à chaque Campaign.
>
> Elle introduit également les entités Submission et Response utilisées par le Participation Flow.
>
> Cette Milestone prépare la collecte sans encore construire le parcours Participant.

---

# Objectif

Permettre à une Organization de :

- définir les concepts qu'elle souhaite mesurer ;
- créer des questionnaires réutilisables ;
- publier et versionner ces questionnaires ;
- générer un formulaire indépendant pour une Campaign ;
- associer les Questions aux Measures ;
- figer le Campaign Form lors de l'activation ;
- disposer du modèle nécessaire à la collecte des Responses.

---

# Valeur produit

À la fin de cette Milestone, SignalLab possède la structure nécessaire pour transformer des questionnaires ponctuels en connaissance réutilisable.

Les Measures assurent la continuité analytique entre plusieurs Campaigns, même lorsque leurs Questions évoluent.

Les Form Templates accélèrent la préparation des études sans créer de dépendance entre les Campaigns.

---

# Domaines concernés

- Measure
- FormTemplate
- FormTemplateVersion
- Section
- Question
- CampaignForm
- CampaignFormSection
- CampaignFormQuestion
- Submission
- Response

---

# Hors scope

Cette Milestone ne contient pas :

- parcours Participant ;
- collecte effective des Responses ;
- Results ;
- Analysis ;
- MAP ;
- logique conditionnelle avancée ;
- branchements conditionnels ;
- import ou export de formulaires ;
- bibliothèque globale de Questions ;
- plusieurs Submissions par Participation.

---

# Principes métier

- Une Measure appartient à une Organization.
- Une Measure représente un concept analytique indépendant des Questions.
- Un Form Template appartient à une Organization.
- Un Form Template n'est jamais utilisé directement pour collecter des Responses.
- La publication d'un Form Template produit une version immuable.
- Un Campaign Form est une copie complète et indépendante d'une FormTemplateVersion.
- Une Campaign possède exactement un Campaign Form avant son activation.
- Le Campaign Form reste modifiable tant que la Campaign est Draft.
- Le Campaign Form devient définitivement immuable lors de l'activation de la Campaign.
- Une Submission regroupe toutes les Responses produites lors d'une tentative complète.
- Une Response répond à exactement une CampaignFormQuestion.
- Dans le MVP, une CampaignParticipation possède au maximum une Submission.
- Une Submission ne contient qu'une Response par CampaignFormQuestion.

---

# Intégration dans l'Application

Cette Milestone enrichit deux contextes existants.

## Organization Explorer

```text
Organization Explorer

├── Measures
└── Form Templates
```

Les Measures et les Form Templates appartiennent à l'Organization et peuvent être utilisés par tous ses Projects.

## Campaign Workspace

```text
Campaign Workspace

└── Form
```

La section **Form** permet de sélectionner un Form Template publié, de générer le Campaign Form puis de le configurer tant que la Campaign reste Draft.

---

# Scénario d'acceptation

Un membre crée une Measure.

↓

Il crée un Form Template.

↓

Il ajoute des Sections et des Questions.

↓

Il associe certaines Questions à des Measures.

↓

Il publie une version du Form Template.

↓

Il ouvre une Campaign Draft.

↓

Il génère son Campaign Form à partir de la version publiée.

↓

Il adapte le Campaign Form.

↓

Il active la Campaign.

↓

Le Campaign Form devient immuable.

---

# Specs

---

## SL-065 — Créer le domaine Measure

### Description

Introduire l'entité Measure.

Une Measure représente un concept que l'Organization souhaite suivre de manière cohérente dans le temps.

Elle est indépendante des Questions et peut être réutilisée dans plusieurs Form Templates et Campaign Forms.

### Objectif produit

Créer un vocabulaire analytique stable au niveau de l'Organization.

### Objectif technique

Créer le domaine Measure, sa persistance et ses contrats API.

### Principes impliqués

- Ownership par Organization.
- Identité stable.
- Réutilisation entre Projects.
- Archivage non destructif.

### Critères d'acceptation

Une Measure peut être créée.

Une Measure appartient à une seule Organization.

Une Measure peut être consultée, modifiée et archivée.

Une Measure archivée reste disponible pour l'interprétation des Campaigns historiques.

### Definition of Done

Le domaine Measure est opérationnel.

---

## SL-066 — Construire la gestion des Measures

### Description

Créer la section **Measures** de l'Organization Explorer.

Cette section permet de consulter et d'organiser le vocabulaire analytique de l'Organization.

### Fonctionnalités

- liste des Measures ;
- recherche ;
- tri ;
- création ;
- modification ;
- archivage ;
- restauration lorsque le Domain Model l'autorise.

### Objectif produit

Permettre aux équipes de retrouver et maintenir leurs concepts analytiques.

### Objectif technique

Créer les composants de gestion des Measures.

### Critères d'acceptation

Les Measures sont affichées dans l'Organization Explorer.

La recherche et le tri fonctionnent.

Les actions disponibles respectent les autorisations de l'Organization.

### Definition of Done

La gestion des Measures est fonctionnelle.

---

## SL-067 — Créer le domaine Form Template

### Description

Introduire les entités nécessaires aux questionnaires réutilisables :

- FormTemplate ;
- Section ;
- Question.

Un Form Template appartient à une Organization et sert uniquement de modèle pour les futures Campaigns.

### Objectif produit

Permettre la création de questionnaires réutilisables.

### Objectif technique

Créer le domaine Form Template, sa persistance et ses contrats API.

### Principes impliqués

- Ownership par Organization.
- Structure ordonnée.
- Validation selon le type de Question.
- Aucun usage direct pour la collecte.

### Critères d'acceptation

Un Form Template peut être créé.

Des Sections peuvent être ajoutées, réordonnées, modifiées et supprimées.

Des Questions peuvent être ajoutées, réordonnées, modifiées et supprimées.

Les types de Questions supportés par le MVP disposent de règles de validation cohérentes.

### Definition of Done

Le domaine Form Template est opérationnel.

---

## SL-068 — Construire le Form Template Workspace

### Description

Créer l'interface permettant de construire et modifier un Form Template.

Le Form Template Workspace constitue l'éditeur de questionnaire réutilisable de SignalLab.

### Cette Spec comprend notamment

- informations générales ;
- gestion des Sections ;
- gestion des Questions ;
- réorganisation ;
- configuration des Questions ;
- prévisualisation structurelle ;
- état de publication.

### Objectif produit

Permettre aux chercheurs de construire un questionnaire sans manipuler directement le Domain Model.

### Objectif technique

Créer un Workspace cohérent avec les conventions UX de SignalLab.

### Principes impliqués

- sauvegarde automatique ;
- structure ordonnée ;
- feedback immédiat ;
- aucune collecte depuis le Template.

### Critères d'acceptation

Un Form Template peut être ouvert et modifié.

Les Sections et Questions sont manipulables dans leur ordre réel.

Les modifications sont persistées automatiquement.

### Definition of Done

Le Form Template Workspace est opérationnel.

---

## SL-069 — Publier et versionner un Form Template

### Description

Permettre de publier un Form Template.

Chaque publication produit une FormTemplateVersion immuable représentant exactement la structure publiée à cet instant.

Les modifications ultérieures du Form Template ne modifient jamais les versions existantes.

### Objectif produit

Garantir la traçabilité des questionnaires utilisés par les Campaigns.

### Objectif technique

Introduire FormTemplateVersion et le workflow de publication.

### Principes impliqués

- version immuable ;
- publication explicite ;
- historique conservé ;
- provenance vérifiable.

### Critères d'acceptation

Un Form Template valide peut être publié.

La publication crée une nouvelle version immuable.

Les versions précédentes restent consultables.

Une version publiée ne peut jamais être modifiée.

### Definition of Done

Le versionnement des Form Templates est opérationnel.

---

## SL-070 — Associer les Questions aux Measures

### Description

Permettre d'associer une Question à une Measure appartenant à la même Organization.

Cette association donne un sens analytique durable aux Responses produites par la Question.

Une Question peut rester sans Measure lorsqu'elle ne doit pas contribuer à une analyse fondée sur les Measures.

### Objectif produit

Relier les questionnaires au vocabulaire analytique de l'Organization.

### Objectif technique

Créer et valider la relation Question → Measure.

### Principes impliqués

- une Question référence au maximum une Measure ;
- la Measure appartient à la même Organization ;
- l'archivage d'une Measure ne détruit aucune référence historique.

### Critères d'acceptation

Une Measure peut être sélectionnée depuis l'éditeur de Question.

Une Question ne peut pas référencer une Measure d'une autre Organization.

La relation est conservée lors de la publication du Form Template.

### Definition of Done

Les Questions peuvent être reliées aux Measures.

---

## SL-071 — Générer le Campaign Form

### Description

Permettre de générer le Campaign Form d'une Campaign Draft à partir d'une FormTemplateVersion publiée appartenant à la même Organization.

La génération produit une copie complète et indépendante.

### La copie comprend notamment

- les Sections ;
- les Questions ;
- leur ordre ;
- leur type ;
- leur configuration ;
- leurs règles de validation ;
- leurs associations aux Measures ;
- la provenance du Form Template et de sa version.

### Objectif produit

Préparer le questionnaire réel d'une Campaign.

### Objectif technique

Créer CampaignForm, CampaignFormSection et CampaignFormQuestion.

### Principes impliqués

- copie complète ;
- nouvelle identité ;
- aucune synchronisation ;
- provenance conservée.

### Critères d'acceptation

Une Campaign Draft peut sélectionner une FormTemplateVersion publiée.

Le Campaign Form est généré intégralement.

La modification du Form Template source n'affecte jamais le Campaign Form.

### Definition of Done

La génération du Campaign Form est opérationnelle.

---

## SL-072 — Configurer le Campaign Form

### Description

Créer la section **Form** du Campaign Workspace.

Tant que la Campaign reste Draft, cette section permet d'adapter le Campaign Form à l'étude concernée.

### Fonctionnalités

- consulter la provenance ;
- modifier les informations générales ;
- ajouter, modifier, réordonner ou retirer des Sections ;
- ajouter, modifier, réordonner ou retirer des Questions ;
- modifier les associations aux Measures ;
- valider la structure du formulaire.

### Objectif produit

Permettre d'adapter un questionnaire réutilisable au contexte précis d'une étude.

### Objectif technique

Réutiliser les composants d'édition compatibles sans créer de dépendance avec le Form Template source.

### Principes impliqués

- indépendance complète ;
- sauvegarde automatique ;
- modifications limitées au statut Draft.

### Critères d'acceptation

Le Campaign Form est accessible depuis le Campaign Workspace.

Il peut être modifié lorsque la Campaign est Draft.

Les modifications n'affectent jamais le Form Template source.

### Definition of Done

La configuration du Campaign Form est fonctionnelle.

---

## SL-073 — Figer le Campaign Form à l'activation

### Description

Intégrer la validation et le gel du Campaign Form au workflow d'activation de la Campaign.

L'activation est refusée tant que le Campaign Form n'est pas présent ou valide.

Après activation, sa structure devient définitivement immuable.

### Éléments figés

- Sections ;
- Questions ;
- ordre ;
- types ;
- configurations ;
- règles de validation ;
- options ;
- associations aux Measures ;
- provenance.

### Objectif produit

Garantir que toutes les Responses restent interprétables dans le temps.

### Objectif technique

Centraliser les invariants de validation et d'immutabilité.

### Critères d'acceptation

Une Campaign ne peut pas être activée sans Campaign Form valide.

L'activation renseigne l'instant de gel.

Aucune modification structurelle n'est possible après activation.

Les contrôles sont garantis côté Backend.

### Definition of Done

Le Campaign Form est correctement figé lors de l'activation.

---

## SL-074 — Créer les domaines Submission et Response

### Description

Introduire les entités Submission et Response utilisées par le Participation Flow.

Une Submission représente une tentative complète de réponse à une Campaign.

Une Response représente la réponse individuelle apportée à une CampaignFormQuestion.

Cette Spec crée le modèle et ses invariants sans construire encore l'interface Participant.

### Objectif produit

Préparer une collecte historique, cohérente et immuable.

### Objectif technique

Créer les entités, statuts, relations, configurations de persistance et contrats nécessaires à M07.

### Principes impliqués

- une Submission appartient à une CampaignParticipation ;
- une Response appartient à une Submission ;
- une Response cible une CampaignFormQuestion ;
- une seule Submission par CampaignParticipation dans le MVP ;
- une seule Response par Question dans une Submission ;
- les Responses deviennent immuables avec la Submission soumise.

### Critères d'acceptation

Submission et Response sont persistables.

Les contraintes d'unicité sont garanties.

Une Response ne peut pas référencer une Question de Form Template.

Une Submission Submitted et ses Responses ne peuvent plus être modifiées.

### Definition of Done

Le modèle de collecte est prêt pour le Participation Flow.

---

## SL-075 — Finaliser Measures, Forms and Response Model

### Description

Stabiliser l'ensemble de la Milestone.

Cette Spec comprend :

- corrections ;
- harmonisation UX ;
- refactoring ciblé ;
- tests ;
- documentation ;
- revue des invariants ;
- validation des migrations.

### Objectif produit

Disposer d'une base fiable avant la collecte réelle des Responses.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Les tests sont verts.

Les règles d'ownership, de versionnement et d'immutabilité sont vérifiées.

La documentation est à jour.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M06, une Organization peut :

- créer et organiser ses Measures ;
- construire des Form Templates ;
- publier et consulter leurs versions ;
- relier les Questions aux Measures ;
- générer un Campaign Form indépendant ;
- configurer ce formulaire avant activation ;
- garantir son immutabilité après activation.

SignalLab possède également les domaines Submission et Response nécessaires à la collecte.

La Milestone suivante peut désormais construire le Participation Flow sans redéfinir le modèle de questionnaire ou de réponse.