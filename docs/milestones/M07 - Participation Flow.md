# M07 — Participation Flow

**Version :** 3.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M06 — Measures, Forms and Response Model  
**Milestone suivante :** M08 — Results Review

---

> Cette Milestone introduit le parcours complet du Participant.
>
> Elle utilise le Campaign Form, les Submissions et les Responses définis dans M06.
>
> Elle permet à un Participant de rejoindre une Campaign active, accepter les conditions de participation, accéder au Build externe, répondre au Campaign Form puis soumettre définitivement sa participation.
>
> Les données collectées ne sont pas encore consultables par les chercheurs. Leur consultation sera introduite dans M08.

---

# Objectif

Permettre à un Participant de réaliser une Campaign du début à la fin.

Le parcours comprend :

- accès via un lien de participation ;
- consultation des informations générales ;
- acceptation des conditions et du consentement ;
- création de la CampaignParticipation ;
- préparation du Participant ;
- transition vers le Build externe ;
- complétion du Campaign Form ;
- enregistrement des Responses ;
- soumission définitive ;
- clôture de la Participation.

---

# Valeur produit

À la fin de cette Milestone, une équipe peut distribuer une étude complète et collecter des réponses exploitables directement dans SignalLab.

---

# Domaines concernés

- CampaignParticipation
- CampaignAccessLink
- Submission
- Response
- CampaignForm

---

# Hors scope

Cette Milestone ne contient pas :

- Results ;
- Analysis ;
- MAP ;
- exports ;
- notifications ;
- Scheduled Campaigns ;
- plusieurs Submissions par Participant ;
- hébergement ou stockage binaire des Builds ;
- exécution d'un Build dans SignalLab ;
- collecte de télémétrie automatique depuis un Build.

---

# Principes métier

- Une Participation appartient à une seule Campaign.
- Le Participant doit être authentifié.
- La Campaign doit être Active.
- L'ouverture d'un lien ne crée jamais de Participation.
- La Participation est créée uniquement après acceptation des conditions requises.
- Une Participation possède au maximum une Submission dans le MVP.
- La Submission regroupe toutes les Responses du Campaign Form.
- Une Response cible exactement une CampaignFormQuestion.
- Les Responses peuvent être modifiées tant que la Submission n'est pas soumise.
- La soumission est une opération explicite et atomique.
- Une Submission soumise et ses Responses sont immuables.
- La clôture de la Participation ne réécrit aucune donnée collectée.

---

# Architecture UX

Cette Milestone utilise un parcours Participant dédié, distinct du Shell principal.

```text
Campaign

↓

Participation Link

↓

Landing

↓

Consent

↓

Preparation

↓

External Build

↓

Campaign Form

↓

Submission

↓

Completion
```

---

# Scénario d'acceptation

Le chercheur partage un lien.

↓

Le Participant authentifié ouvre le lien.

↓

Il consulte les informations de la Campaign.

↓

Il accepte les conditions de participation.

↓

La CampaignParticipation est créée.

↓

Il consulte les instructions et accède au Build externe.

↓

Il revient dans SignalLab et complète le Campaign Form.

↓

Ses Responses sont enregistrées.

↓

Il soumet sa Submission.

↓

La Participation est clôturée.

---

# Specs

---

## SL-076 — Créer le domaine CampaignParticipation

### Description

Introduire le domaine métier CampaignParticipation.

Une CampaignParticipation représente la relation entre un ParticipantProfile et une Campaign.

Elle existe indépendamment de la Submission qu'elle pourra contenir.

### Objectif produit

Représenter l'engagement et l'avancement d'un Participant dans une Campaign.

### Objectif technique

Créer le domaine, ses statuts, sa persistance et ses contrats API.

### Principes impliqués

- Ownership par Campaign.
- Identité du Participant permanente.
- Une seule Participation active par Participant et par Campaign dans le MVP.
- Cycle de vie explicite.

### Critères d'acceptation

Le domaine est créé.

La persistance est opérationnelle.

Les transitions autorisées sont validées côté Backend.

### Definition of Done

Le domaine CampaignParticipation est opérationnel.

---

## SL-077 — Générer et gérer les liens de participation

### Description

Permettre aux chercheurs de générer des CampaignAccessLinks.

Un lien représente uniquement un mécanisme d'accès à une Campaign.

Son ouverture ne crée jamais de CampaignParticipation.

### Objectif produit

Distribuer facilement une Campaign active.

### Objectif technique

Créer le mécanisme de génération, de résolution et de validation des liens.

### Principes impliqués

- un lien référence une Campaign ;
- un lien possède une identité distincte ;
- la validité du lien est vérifiée avant tout engagement ;
- aucune Participation n'est créée à l'ouverture.

### Critères d'acceptation

Un lien peut être créé et désactivé.

Un lien valide ouvre le Participation Flow.

Un lien invalide ou expiré est refusé proprement.

### Definition of Done

La gestion des liens de participation est opérationnelle.

---

## SL-078 — Construire le Landing Flow

### Description

Créer le point d'entrée du parcours Participant.

Le Landing présente les informations nécessaires pour comprendre la Campaign avant tout engagement.

### Informations affichées

- nom de la Campaign ;
- description ;
- Organization ;
- Project ou produit concerné lorsque pertinent ;
- durée attendue ;
- informations générales de participation.

### Objectif produit

Permettre au Participant de décider librement s'il souhaite poursuivre.

### Objectif technique

Résoudre le contexte de Campaign sans créer d'état métier.

### Principes impliqués

- aucune Participation créée ;
- aucune Submission créée ;
- aucune Response enregistrée.

### Critères d'acceptation

Le Landing affiche les informations cohérentes avec la Campaign.

Le Participant peut quitter le parcours sans créer de donnée métier.

### Definition of Done

Le Landing Flow est opérationnel.

---

## SL-079 — Gérer le consentement et créer la Participation

### Description

Créer l'étape d'acceptation des conditions de participation.

L'acceptation explicite déclenche la création de la CampaignParticipation.

Le refus met fin au parcours sans créer de Participation.

### Cette Spec comprend notamment

- affichage des conditions ;
- affichage du consentement lorsqu'il est requis ;
- acceptation ;
- refus ;
- protection contre les créations multiples.

### Objectif produit

Obtenir un engagement explicite avant toute collecte.

### Objectif technique

Créer atomiquement la CampaignParticipation après validation des préconditions.

### Critères d'acceptation

La Participation est créée uniquement après acceptation.

Le refus ne crée aucune Participation.

Une Participation existante est retrouvée plutôt que dupliquée.

### Definition of Done

Le consentement et la création de Participation fonctionnent correctement.

---

## SL-080 — Préparer le Participant

### Description

Créer l'étape de préparation précédant l'activité étudiée.

Cette étape présente les instructions nécessaires avant l'accès au Build externe.

### Informations affichées

- instructions ;
- recommandations ;
- équipements nécessaires ;
- durée attendue ;
- modalités de retour vers SignalLab.

### Objectif produit

Garantir que le Participant commence l'étude dans de bonnes conditions.

### Objectif technique

Créer une étape extensible sans introduire de vérification technique hors MVP.

### Principes impliqués

- la Participation existe ;
- aucune Submission n'est encore requise ;
- le Participant peut reprendre le parcours existant.

### Critères d'acceptation

Les informations de préparation sont affichées.

Le Participant peut poursuivre vers le Build externe.

### Definition of Done

L'étape de préparation est opérationnelle.

---

## SL-081 — Accéder au Build externe

### Description

Créer le point de transition entre SignalLab et le mécanisme externe permettant d'accéder au Build associé à la Campaign.

Dans le MVP, SignalLab n'héberge, ne stocke, ne distribue et n'exécute aucun Build.

Le système utilise uniquement les métadonnées et la provenance technique du Build pour présenter ou ouvrir la référence externe appropriée.

### Objectif produit

Permettre au Participant de réaliser l'activité étudiée.

### Objectif technique

Résoudre la référence du Build sans dépendre d'un futur mécanisme d'hébergement.

### Principes impliqués

- la Campaign référence un Build valide ;
- le Build appartient au même Project ;
- aucun artefact binaire n'est manipulé ;
- le retour vers SignalLab reste explicite.

### Critères d'acceptation

La référence du Build est correctement résolue.

Le Participant peut accéder au mécanisme externe prévu.

Le parcours permet de revenir au Campaign Form.

### Definition of Done

La transition vers le Build externe est opérationnelle.

---

## SL-082 — Compléter le Campaign Form

### Description

Construire le Document permettant au Participant de répondre au Campaign Form figé de la Campaign.

L'ouverture du formulaire crée la Submission de la CampaignParticipation lorsqu'elle n'existe pas encore.

La première Response enregistrée fait passer la Submission dans son état de progression approprié.

### Cette Spec comprend notamment

- affichage des Sections ;
- affichage des Questions ;
- saisie selon le type de Question ;
- validation locale immédiate ;
- validation Backend ;
- progression ;
- reprise d'une Submission non soumise.

### Objectif produit

Permettre au Participant de répondre simplement au questionnaire de l'étude.

### Objectif technique

Construire l'interface de collecte à partir du Campaign Form immuable.

### Principes impliqués

- le Form Template n'est jamais utilisé directement ;
- les Responses ciblent les CampaignFormQuestions ;
- une seule Response par Question ;
- l'ordre du Campaign Form est respecté.

### Critères d'acceptation

Toutes les Questions du Campaign Form sont affichées correctement.

Les valeurs sont validées selon la configuration de chaque Question.

Une Submission existante non soumise peut être reprise.

### Definition of Done

Le Campaign Form Participant est opérationnel.

---

## SL-083 — Enregistrer les Responses

### Description

Persister les Responses pendant la complétion du Campaign Form.

Chaque modification met à jour l'unique Response associée à la CampaignFormQuestion concernée tant que la Submission reste modifiable.

### Objectif produit

Éviter la perte des réponses avant la soumission définitive.

### Objectif technique

Créer les Commands et Queries nécessaires à l'enregistrement progressif et cohérent des Responses.

### Principes impliqués

- unicité par Question ;
- validation côté Backend ;
- sauvegarde automatique ;
- aucune modification après soumission.

### Critères d'acceptation

Les Responses sont persistées pendant la saisie.

Une valeur invalide est refusée.

Une Response existante est mise à jour plutôt que dupliquée.

Une Response soumise ne peut plus être modifiée.

### Definition of Done

La persistance des Responses est opérationnelle.

---

## SL-084 — Soumettre et clôturer la Participation

### Description

Permettre au Participant de soumettre définitivement sa Submission.

La soumission valide l'ensemble du Campaign Form, renseigne la date de soumission, rend les Responses immuables et clôture la CampaignParticipation.

Cette opération est atomique.

### Objectif produit

Finaliser clairement l'étude et garantir l'intégrité des données collectées.

### Objectif technique

Implémenter la transition Submission → Submitted et CampaignParticipation → Completed.

### Principes impliqués

- action explicite ;
- validation complète ;
- atomicité ;
- immutabilité définitive ;
- une seule clôture.

### Critères d'acceptation

Une Submission valide peut être soumise.

Une Submission incomplète ou invalide est refusée avec des erreurs exploitables.

Les Responses deviennent immuables.

La Participation est clôturée une seule fois.

Un écran de fin est affiché.

### Definition of Done

La soumission et la clôture sont opérationnelles.

---

## SL-085 — Sécuriser le Participation Flow

### Description

Centraliser les validations et contrôles d'accès du parcours Participant.

### Vérifications

- User authentifié ;
- lien valide ;
- Campaign Active ;
- Build associé et exploitable ;
- Campaign Form présent et figé ;
- éligibilité du Participant ;
- Participation cohérente ;
- Submission non déjà soumise ;
- ownership de toutes les Responses.

### Objectif produit

Garantir un parcours fiable sans risque d'accès ou de collecte incohérente.

### Objectif technique

Protéger les routes, Commands et Queries du Participation Flow.

### Critères d'acceptation

Les accès invalides sont refusés.

Un Participant ne peut jamais consulter ou modifier la Participation d'un autre User.

Aucune Response ne peut être injectée dans une Submission étrangère.

Les erreurs de parcours sont présentées clairement.

### Definition of Done

Le Participation Flow est sécurisé.

---

## SL-086 — Finaliser Participation Flow

### Description

Stabiliser l'ensemble du parcours Participant.

Cette Spec comprend :

- corrections ;
- harmonisation UX ;
- gestion des états vides et d'erreur ;
- tests end-to-end ;
- tests des invariants ;
- documentation ;
- revue complète.

### Objectif produit

Disposer d'un parcours complet, fiable et simple pour une étude réelle.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Le parcours complet fonctionne depuis le lien jusqu'à la soumission.

Les tests sont verts.

La documentation est à jour.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M07, un Participant peut :

- ouvrir une Campaign active depuis un lien ;
- comprendre l'étude ;
- accepter les conditions ;
- créer sa CampaignParticipation ;
- consulter les instructions ;
- accéder au Build externe ;
- compléter le Campaign Form ;
- enregistrer ses Responses ;
- soumettre définitivement sa Submission ;
- clôturer sa Participation.

SignalLab possède alors des Submissions et Responses historiques prêtes à être consultées dans Results Review.