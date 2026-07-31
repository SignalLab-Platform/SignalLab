# SignalLab — Domain Model

## 1. Fondations

### Objectif

Le Domain Model définit les concepts métier fondamentaux de SignalLab, leurs responsabilités, leurs relations et les invariants qui garantissent la cohérence du produit.

Il constitue la source de vérité fonctionnelle du système.

Les choix d'interface, d'architecture technique, d'API, de stockage ou d'implémentation n'en font pas partie.

Le Domain Model décrit ce que représente le produit, indépendamment de la manière dont il est développé.

---

### Principes du domaine

Les principes suivants structurent l'ensemble du modèle métier.

#### Identité globale

Chaque utilisateur possède une identité globale SignalLab.

Cette identité est indépendante des Organizations auxquelles il appartient.

Un utilisateur peut simultanément être :

- membre d'une ou plusieurs Organizations ;
- Participant à plusieurs Campaigns ;
- administrateur d'une Organization ;
- ou toute combinaison de ces rôles.

---

#### Frontières métier

Le domaine est organisé autour de trois niveaux hiérarchiques.

Organization
├── OrganizationInvitations
└── Project
    └── Campaign

Une Campaign appartient toujours à un Project.

Un Project appartient toujours à une Organization.

Une Campaign ne peut pas exister sans Project.

Un Project ne peut pas exister sans Organization.

Ces relations constituent des invariants du domaine.

---

#### Ownership

Chaque niveau possède ses propres ressources métier.

Une Organization possède :

- ses Members ;
- ses Roles ;
- ses OrganizationInvitations ;
- ses Measures ;
- ses FormTemplates et leurs QuestionMeasureBindings ;
- ses Projects ;
- ses OrganizationActivityEntries post-MVP.

Un Project possède :

- ses Builds ;
- ses Campaigns ;
- ses SavedAnalysis ;
- ses ResearchBoards post-MVP ;
- ses Labels post-MVP ;
- ses ProjectActivityEntries post-MVP.

Une Campaign possède :

- son CampaignForm ;
- ses CampaignParticipations ;
- ses Submissions ;
- ses Responses.

Chaque ressource appartient à un seul propriétaire métier.

---

#### Isolation des données

Les données sont isolées par Organization.

Les Projects d'une même Organization partagent les ressources définies au niveau de l'Organization, mais restent indépendants concernant leurs données opérationnelles.

Aucune donnée métier n'est partagée entre deux Organizations.

---

#### FormTemplate et CampaignForm

Un FormTemplate représente un modèle réutilisable.

Lors de la création d'une Campaign, le FormTemplate est copié afin de produire un CampaignForm.

Le CampaignForm devient alors entièrement indépendant du FormTemplate.

Les modifications apportées ultérieurement au FormTemplate n'affectent jamais les Campaigns existantes.

La provenance est conservée grâce à :

- `sourceTemplateId`
- `sourceTemplateVersionId`

---

#### Immutabilité des Campaigns

Une Campaign peut être modifiée tant qu'elle n'a pas commencé.

Au moment de son activation, son CampaignForm devient définitivement immuable.

Aucune modification de structure n'est ensuite autorisée sur :

- les Sections ;
- les Questions ;
- les types de Questions ;
- les options ;
- les QuestionMeasureBindings ;
- le Build utilisé.

Cette règle garantit la cohérence historique des données collectées.

---

#### Measures

Les Measures appartiennent à l'Organization.

Elles représentent le vocabulaire analytique commun utilisé par tous les Projects de cette Organization.

Les Questions sont reliées à des Measures par des QuestionMeasureBindings explicites afin de donner un sens métier aux Responses.

Une Measure peut être réutilisée dans plusieurs FormTemplates, plusieurs CampaignForms et plusieurs Campaigns.

---

#### Analytics

Les Analytics représentent des calculs réalisés à partir des données collectées.

Ils ne constituent jamais la source de vérité.

Les données métier persistées sont notamment :

- les Campaigns ;
- les CampaignParticipations ;
- les Submissions ;
- les Responses ;
- les SavedAnalysis ;
- les ResearchBoards post-MVP.

Les Analytics peuvent toujours être recalculés à partir de ces données.

---

#### Identité des Participants

La participation anonyme n'est pas prise en charge.

Toute participation nécessite un User authentifié.

Chaque Submission est toujours rattachée à un Participant identifié.

Les permissions peuvent masquer cette identité à certains utilisateurs, mais le lien métier reste permanent.

---

#### MAP

SignalLab repose sur le modèle MAP comme système de profil motivationnel.

Les définitions, versions et calculs de MAP sont maintenus par SignalLab.

Les Organizations utilisent MAP mais ne peuvent pas modifier sa définition.

---

#### Responsabilités

Le modèle distingue explicitement les responsabilités suivantes :

- l'identité d'un User ;
- son appartenance à une Organization ;
- son rôle au sein d'une Organization ;
- son profil de Participant ;
- sa participation à une Campaign ;
- ses Submissions.

Chaque responsabilité possède sa propre entité.

Ces responsabilités ne doivent jamais être fusionnées.

---

### Hiérarchie métier

SignalLab
│
├── Users
├── MAP
│
└── Organizations
    ├── Members
    ├── Role
    |   └── Permissions[]
    ├── OrganizationInvitations
    ├── Measures
    ├── FormTemplates
    ├── OrganizationActivityEntries *(post-MVP)*
    └── Projects
        ├── Builds
        ├── Campaigns
        ├── SavedAnalysis
        ├── ResearchBoards *(post-MVP)*
        ├── Labels *(post-MVP)*
        └── ProjectActivityEntries *(post-MVP)*

Cette hiérarchie représente les principales frontières du domaine.

Elle ne préjuge pas de l'architecture technique ni du découpage des services.

---

### Cycle de vie des entités

Toutes les entités persistantes suivent les mêmes principes.

Chaque entité possède au minimum :

- un identifiant immuable ;
- une date de création ;
- une date de dernière modification.

Selon les besoins, une entité peut également conserver :

- son créateur ;
- son auteur de dernière modification ;
- son historique de versions.

Les identifiants ne changent jamais.

Les données historiques ne sont jamais réécrites.

---

### États métier du MVP

Les statuts métier du MVP sont explicites et persistés.

Ils ne sont jamais déduits uniquement :

- d'une date ;
- de l'existence d'une entité liée ;
- de la présence ou de l'absence de données associées.

Toute transition non documentée est interdite.

Une transition ne peut être réalisée que lorsque les invariants de l'état cible sont satisfaits.

Lorsqu'une transition possède une date métier associée, cette date est renseignée au moment de la transition.

L'archivage ne supprime jamais les données historiques.

Une date ne déclenche aucun changement d'état automatique dans le MVP.

Les états terminaux ne peuvent plus être quittés, sauf lorsqu'une restauration est explicitement autorisée.

#### Cycles de vie principaux du MVP

```text
Campaign
Draft ──→ Active ──→ Completed ──→ Archived
  └───────────────────────────────→ Archived

CampaignParticipation
Accepted ──→ Started ──→ Completed
    ├───────────┴──────→ Abandoned
    └──────────────────→ Expired

Submission
Draft ──→ Submitted
  └─────→ Abandoned

OrganizationMember
Active ⇄ Removed

Project
Active ⇄ Archived

Build
Active ⇄ Archived

Measure
Active ⇄ Archived

FormTemplate
Active ⇄ Archived
```

Le statut `Scheduled`, les états de Participation liés aux invitations et les réouvertures de Campaign appartiennent à la cible post-MVP.

Les règles détaillées propres à chaque état sont définies dans les chapitres des entités concernées.

---

### Périmètre du Domain Model

Le Domain Model décrit la cible fonctionnelle complète de SignalLab.

Toutes les entités présentes dans ce document ne sont pas nécessairement implémentées dès le MVP.

Chaque section précisera explicitement :

- le contrat métier ;
- le périmètre du MVP ;
- les extensions prévues hors MVP.

Le modèle métier reste ainsi stable, même si le produit évolue progressivement.

---

## 2. Identité et Participants

### Vue d'ensemble

SignalLab distingue plusieurs niveaux d'identité.

Chaque niveau représente une responsabilité métier différente.

User
├── ParticipantProfile
├── MAPAssessments
├── MAPProfile
└── OrganizationMember

ParticipantProfile
└── CampaignParticipation
    └── Submission

Cette séparation permet à une même personne d'être simultanément membre de plusieurs Organizations et Participant à plusieurs Campaigns sans dupliquer son identité.

---

## User

### Description

Le User représente l'identité globale d'une personne dans SignalLab.

Il constitue le point d'entrée du système d'authentification.

Toutes les autres identités métier dérivent d'un User.

Un User peut exister sans appartenir à une Organization.

Un User peut exister sans avoir participé à une Campaign.

---

### Responsabilités

Le User est responsable de :

- l'authentification ;
- l'identité globale ;
- les informations personnelles ;
- les préférences globales.

Le User ne porte aucune responsabilité métier liée à une Organization ou à une Campaign.

---

### Principales propriétés

- UserId
- Email
- FirstName
- LastName
- Language
- TimeZone
- Avatar
- CreatedAt
- UpdatedAt

---

### Relations

Un User peut posséder :

- un ParticipantProfile ;
- plusieurs MAPAssessments ;
- un MAPProfile actuel ;
- plusieurs OrganizationMembers ;
- plusieurs OrganizationInvitations reçues.

---

### Invariants

L'Email est unique à l'échelle de SignalLab.

L'identité d'un User est permanente.

Les informations personnelles et les préférences du User sont des données vivantes.

Leur modification ne réécrit jamais les consentements, snapshots de participation, Submissions ou Responses historiques.

La suppression d'un User ne doit jamais compromettre l'intégrité historique des données.

---

## ParticipantProfile

### Description

Le ParticipantProfile représente l'identité d'un User en tant que Participant.

Il centralise toutes les informations utilisées lors des études.

Il est indépendant des Organizations.

Il accompagne le Participant tout au long de son parcours dans SignalLab.

---

### Responsabilités

Le ParticipantProfile est responsable :

- du profil Participant ;
- des informations démographiques ;
- des préférences de participation.

---

### Principales propriétés

- ParticipantProfileId
- UserId
- BirthDate
- Gender
- Country
- Languages
- Preferences

Les informations démographiques pourront évoluer au fil du temps.

Les Campaigns conservent toujours les valeurs utilisées au moment de la participation.

---

### Relations

Un ParticipantProfile peut participer à plusieurs Campaigns.

---

### Invariants

Un User possède au maximum un ParticipantProfile.

Le ParticipantProfile n'appartient jamais à une Organization.

Le MAPProfile et les MAPAssessments appartiennent directement au User et ne sont jamais détenus par le ParticipantProfile.

Ses informations démographiques courantes sont des données vivantes.

Leur modification ne réécrit jamais les snapshots démographiques déjà conservés par les CampaignParticipations.

---

## OrganizationMember

### Description

OrganizationMember représente l'appartenance d'un User à une Organization.

Cette entité matérialise la relation entre une personne et une Organization.

Elle ne représente pas un Participant.

---

### Responsabilités

OrganizationMember est responsable :

- de l'appartenance à une Organization ;
- des Roles attribués ;
- des Permissions héritées.

---

### Principales propriétés

- OrganizationMemberId
- OrganizationId
- UserId
- Status
- JoinedAt
- RemovedAt

`RemovedAt` est facultatif et n'est renseigné que lorsque le Membership est `Removed`.

---

### OrganizationMemberStatus

```text
Active ⇄ Removed
```

Un OrganizationMember nouvellement créé possède le statut `Active`.

Un OrganizationMember `Active` peut accéder à l'Organization selon son Role et ses Permissions.

Un OrganizationMember `Removed` :

- ne dispose plus d'aucun accès à l'Organization ;
- conserve son identité et toutes les références historiques qui le désignent ;
- ne peut redevenir `Active` que par l'acceptation d'une nouvelle OrganizationInvitation valide.

Le passage à `Removed` renseigne `RemovedAt`.

La réactivation conserve le même `OrganizationMemberId`, applique le Role porté par la nouvelle OrganizationInvitation et réinitialise `RemovedAt`.

---

### Relations

Un OrganizationMember appartient à une Organization.

Un User peut posséder plusieurs OrganizationMembers.

---

### Invariants

Un User ne peut posséder qu'un seul OrganizationMember par Organization, quel que soit son statut.

Le `UserId`, l'`OrganizationId`, le `OrganizationMemberId` et la date d'entrée initiale sont immuables.

Un Membership retiré n'est jamais remplacé par une nouvelle entité pour la même paire User–Organization.

Sa réactivation exige l'acceptation d'une nouvelle OrganizationInvitation et conserve son identité historique.

Le Role actif peut évoluer selon les Permissions applicables.

Une modification de Role change les autorisations actuelles sans réécrire l'auteur, le contexte ou les Permissions associés aux événements historiques déjà enregistrés.

Les Permissions sont déterminées par les Roles attribués.

---

## CampaignParticipation

### Description

CampaignParticipation représente la relation entre un ParticipantProfile et une Campaign.

Elle existe indépendamment de toute Submission.

Dans le MVP, elle est créée après l'acceptation des conditions requises.

Les workflows post-MVP fondés sur une invitation pourront créer cette relation avant toute réponse.

---

### Responsabilités

CampaignParticipation décrit :

- l'état de participation ;
- la méthode d'accès ;
- les métadonnées de participation ;
- les Submissions réalisées.

---

### Principales propriétés

- CampaignParticipationId
- CampaignId
- ParticipantProfileId
- Status
- RecruitmentSource
- DemographicSnapshot
- ConsentSnapshot *(facultatif)*
- AcceptedAt
- StartedAt
- CompletedAt
- AbandonedAt
- ExpiredAt

---

### Relations

Une CampaignParticipation appartient à une Campaign.

Une CampaignParticipation appartient à un ParticipantProfile.

Une CampaignParticipation peut posséder plusieurs Submissions dans la cible fonctionnelle.

Dans le MVP, elle en possède au maximum une.

---

### Invariants

Une CampaignParticipation n'existe que pour une seule Campaign.

Elle ne peut jamais changer de Campaign ou de ParticipantProfile.

Ses rattachements, sa source de recrutement, ses snapshots et sa date d'acceptation sont immuables dès sa création.

Une modification du ParticipantProfile ne réécrit jamais les valeurs historiques utilisées par la participation.

---

## Submission

### Description

Une Submission représente une tentative complète de réponse à une Campaign.

Elle constitue l'unité historique de collecte.

Toutes les Responses appartiennent à une Submission.

---

### Responsabilités

La Submission est responsable :

- de la date de soumission ;
- de l'état de complétion ;
- des Responses ;
- des métadonnées de collecte.

---

### Principales propriétés

- SubmissionId
- CampaignParticipationId
- Status
- SubmittedAt
- AbandonedAt

---

### Relations

Une Submission appartient à une CampaignParticipation.

Une Submission possède plusieurs Responses.

---

### Invariants

Une Response appartient toujours à une seule Submission.

Le nombre maximal de Submissions autorisées dépend de la configuration de la Campaign.

Dans le MVP, une Campaign limite ce nombre à une seule Submission par Participant.

Une Submission `Submitted` ou `Abandoned` et toutes ses Responses sont définitivement immuables.

---

## Contrat métier

Le modèle d'identité repose sur la chaîne suivante :

User
  │
  ├── OrganizationMember
  │
  └── ParticipantProfile
      │
      └── CampaignParticipation
          │
          └── Submission
              │
              └── Response

Chaque entité représente une responsabilité distincte.

Aucune de ces responsabilités ne doit être fusionnée dans une même entité.

---

## Périmètre MVP

Le MVP implémente :

- User ;
- OrganizationMember ;
- ParticipantProfile ;
- CampaignParticipation ;
- Submission.

Chaque Campaign autorise une seule Submission par Participant.

---

## Hors MVP

Les évolutions prévues comprennent notamment :

- plusieurs Submissions par CampaignParticipation ;
- historique complet des profils démographiques ;
- participation récurrente sur plusieurs vagues d'une même Campaign ;
- anonymisation avancée ;
- fusion de comptes ;
- identité fédérée (SSO).

---

# 3. Organizations

## Vue d'ensemble

Une `Organization` représente une équipe de travail utilisant SignalLab.

Elle constitue la principale frontière de collaboration, de gouvernance et d'isolation des données.

Toutes les ressources communes sont définies au niveau de l'Organization puis réutilisées par ses `Projects`.

```
Organization
├── OrganizationMembers
├── Roles
├── OrganizationInvitations
├── Measures
├── FormTemplates
├── Projects
└── OrganizationActivityEntries *(post-MVP)*
```

Une `Organization` ne possède jamais directement de `Campaigns`.

Les `Campaigns` appartiennent toujours à un `Project`.

---

## Organization

### Description

Une `Organization` représente une entité collaborative autonome.

Elle regroupe des utilisateurs partageant un même espace de travail, un vocabulaire métier commun et un ensemble de `Projects`.

Les Organizations sont totalement indépendantes les unes des autres.

---

### Responsabilités

Une `Organization` est responsable :

* de ses `OrganizationMembers` ;
* de ses `Roles` ;
* de ses `OrganizationInvitations` ;
* de ses `Measures` ;
* de ses `FormTemplates` ;
* de ses `Projects` ;
* de ses `OrganizationActivityEntries` post-MVP.

---

### Principales propriétés

* `OrganizationId`
* `Name`
* `Country`
* `Description`
* `Logo`
* `Website`
* `CreatedAt`
* `UpdatedAt`

---

### Relations

Une `Organization` possède :

* plusieurs `OrganizationMembers` ;
* plusieurs `Roles` ;
* plusieurs `OrganizationInvitations` ;
* plusieurs `Measures` ;
* plusieurs `FormTemplates` ;
* plusieurs `Projects` ;
* plusieurs `OrganizationActivityEntries` post-MVP.

---

### Invariants

Une `Organization` constitue une frontière d'isolation métier.

Son `Name` et son `Country` sont obligatoires dès sa création.

`Website` est facultatif. Lorsqu'il est renseigné, il contient une URL valide.

Aucune ressource métier ne peut appartenir simultanément à plusieurs Organizations.

Une Organization ne possède jamais directement de Campaigns.

---

## OrganizationMember

Voir le chapitre **Identité et Participants**.

L'Organization est propriétaire de ses `OrganizationMembers`.

Chaque `OrganizationMember` matérialise l'appartenance d'un `User` à une `Organization`.

---

## Role

### Description

Un `Role` définit un ensemble cohérent d'autorisations pouvant être attribué à des `OrganizationMembers`.

Il représente un niveau d'accès réutilisable au sein d'une Organization.

Les autorisations accordées par un `Role` sont sélectionnées dans le **Permission Catalogue** de SignalLab.

---

### Responsabilités

Un `Role` est responsable :

* de définir un niveau d'accès ;
* de regrouper des permissions ;
* d'être attribué aux `OrganizationMembers`.

---

### Principales propriétés

* `RoleId`
* `OrganizationId`
* `Name`
* `Description`
* `Permissions`
* `IsSystemRole`
* `CreatedAt`
* `UpdatedAt`

---

### Relations

Un `Role` appartient à une seule `Organization`.

Plusieurs `OrganizationMembers` peuvent partager un même `Role`.

Chaque `Role` référence une ou plusieurs permissions du **Permission Catalogue**.

---

### Invariants

Les `Roles` sont propres à leur `Organization`.

Ils ne peuvent jamais être partagés entre plusieurs Organizations.

Toutes les permissions référencées doivent appartenir au **Permission Catalogue** de SignalLab.

---

## Permission Catalogue

### Description

Le **Permission Catalogue** définit l'ensemble des permissions fonctionnelles disponibles dans SignalLab.

Il constitue le vocabulaire officiel utilisé par les `Roles` pour définir les autorisations accordées aux utilisateurs.

Le catalogue est défini et maintenu par SignalLab.

Les Organizations ne peuvent ni créer, ni modifier, ni supprimer des permissions.

---

### Exemples de permissions

* `ManageOrganization`
* `ManageMembers`
* `ManageProjects`
* `ManageCampaigns`
* `ManageTemplates`
* `ManageMeasures`
* `ViewAnalytics`
* `ExportData`

Cette liste est représentative et pourra évoluer avec les fonctionnalités de SignalLab.

---

### Invariants

Le catalogue est global à SignalLab.

Toutes les Organizations utilisent le même catalogue.

Les permissions ne sont jamais attribuées directement aux utilisateurs.

Les permissions effectives d'un utilisateur résultent des `Roles` qui lui sont attribués.

---

## Rôles utilisateurs du MVP

### Distinction entre rôle contextuel et rôle d'autorisation

Le terme **Participant** décrit le rôle contextuel d'un `User` prenant part à une `Campaign`.

Il ne correspond pas à un `Role` d'Organization et n'accorde aucun accès aux ressources internes d'une Organization.

L'identité du Participant est portée par `ParticipantProfile` et sa relation à une Campaign par `CampaignParticipation`.

Un même `User` peut simultanément être Participant et `OrganizationMember` de plusieurs Organizations.

---

### Rôles système de l'Organization

Dans le MVP, chaque `OrganizationMember` possède exactement un rôle actif parmi :

* `Member` ;
* `Administrator` ;
* `Owner`.

Ces trois Roles sont créés et maintenus par SignalLab.

Une Organization ne peut ni les supprimer, ni modifier leur identité, ni redéfinir librement leurs Permissions dans le MVP.

Leur hiérarchie fonctionnelle est la suivante :

```text
Owner
└── Administrator
    └── Member
```

Cette représentation signifie qu'un `Owner` dispose de toutes les capacités d'un `Administrator`, et qu'un `Administrator` dispose de toutes les capacités d'un `Member`.

Elle ne nécessite pas un mécanisme technique d'héritage entre Roles.

---

### Member

Le rôle `Member` permet de participer aux activités de recherche de l'Organization.

Un `Member` peut notamment :

* consulter l'Organization et ses membres ;
* accéder à tous les Projects de l'Organization dans le MVP ;
* créer, consulter, modifier et archiver des Projects ;
* gérer les Builds ;
* gérer les Measures ;
* gérer les FormTemplates ;
* créer et configurer les Campaigns ;
* gérer le recrutement et les CampaignParticipations ;
* consulter les Results ;
* créer et modifier les SavedAnalysis.

Un `Member` ne peut pas :

* inviter ou retirer des OrganizationMembers ;
* modifier le Role d'un OrganizationMember ;
* modifier les paramètres généraux de l'Organization ;
* gérer la propriété de l'Organization.

---

### Administrator

Le rôle `Administrator` possède toutes les capacités du rôle `Member`.

Il peut également :

* modifier les informations générales de l'Organization ;
* inviter de nouveaux OrganizationMembers ;
* annuler les Invitations en attente ;
* retirer un `Member` ou un autre `Administrator` ;
* attribuer les rôles `Member` et `Administrator` ;
* administrer les ressources communes de l'Organization.

Un `Administrator` ne peut pas :

* attribuer le rôle `Owner` ;
* retirer ou rétrograder un `Owner` ;
* réaliser une opération qui laisserait l'Organization sans Owner.

---

### Owner

Le rôle `Owner` possède toutes les capacités du rôle `Administrator`.

Il porte également les responsabilités de gouvernance et de continuité de propriété de l'Organization.

Un `Owner` peut notamment :

* promouvoir un OrganizationMember au rôle `Owner` ;
* rétrograder ou retirer un autre `Owner` ;
* quitter l'Organization lorsqu'au moins un autre Owner y reste actif ;
* réaliser les opérations réservées à la propriété de l'Organization.

Une Organization peut posséder plusieurs Owners.

Elle doit toujours conserver au moins un Owner actif.

Le dernier Owner ne peut donc jamais :

* quitter l'Organization ;
* être retiré ;
* être rétrogradé vers un autre Role.

---

### Autorisation fondée sur les Permissions

Les fonctionnalités métier ne vérifient jamais directement le nom d'un Role.

Elles vérifient la `Permission` requise pour réaliser l'action demandée.

```text
Role
↓
Permissions
↓
Action autorisée ou refusée
```

Les rôles `Member`, `Administrator` et `Owner` constituent donc des ensembles prédéfinis de Permissions pour le MVP.

Cette règle permet d'introduire ultérieurement des Roles personnalisables sans coupler les fonctionnalités aux trois noms de rôles du MVP.

---

### Profils d'usage

Les termes `Research Lead`, `Game Designer`, `Producer` et `QA / Playtest Manager` décrivent des profils d'usage du produit.

Ils ne constituent ni des entités métier, ni des Roles d'autorisation.

Dans le MVP, ces profils utilisent généralement le rôle `Member`, mais un User exerçant l'un de ces métiers peut également être `Administrator` ou `Owner`.

---

### Périmètre MVP

Le MVP applique les règles suivantes :

* trois Roles système fixes : `Member`, `Administrator` et `Owner` ;
* exactement un Role actif par `OrganizationMember` ;
* autorisations accordées exclusivement par les Permissions du Role ;
* accès à tous les Projects de l'Organization selon les Permissions du Role ;
* au moins un Owner actif par Organization ;
* aucune Permission attribuée directement à un User ou à un OrganizationMember.

Les Roles personnalisables, l'attribution de plusieurs Roles à un même OrganizationMember et les Roles propres aux Projects appartiennent au périmètre post-MVP.

---

## OrganizationInvitation

### Description

Une `OrganizationInvitation` représente une invitation nominative envoyée à un `User` existant afin qu'il rejoigne une `Organization` avec un `Role` déterminé.

Elle constitue un mécanisme d'accès à l'Organization et ne remplace jamais l'`OrganizationMember` créé lors de son acceptation.

---

### Principales propriétés

* `OrganizationInvitationId`
* `OrganizationId`
* `RecipientUserId`
* `AssignedRoleId`
* `InvitedByOrganizationMemberId`
* `Status`
* `ExpiresAt`
* `RespondedAt`
* `CreatedAt`
* `UpdatedAt`

---

### OrganizationInvitationStatus

```text
Pending ──→ Accepted
   ├──────→ Declined
   ├──────→ Cancelled
   └──────→ Expired
```

Une invitation nouvellement créée possède le statut `Pending`.

`Accepted`, `Declined`, `Cancelled` et `Expired` sont terminaux.

---

### Relations

Une OrganizationInvitation appartient à exactement une Organization.

Elle cible exactement un User existant.

Elle référence exactement un Role système de cette Organization.

Elle est créée par un OrganizationMember disposant de la Permission nécessaire.

---

### Invariants

Une OrganizationInvitation ne peut pas cibler un User possédant déjà un OrganizationMember `Active` dans l'Organization.

Elle peut cibler un User dont l'OrganizationMember historique possède le statut `Removed` afin de permettre sa réactivation.

Une seule invitation `Pending` peut exister pour une même paire Organization–User.

Le User qui accepte l'invitation doit être le `RecipientUserId` ciblé.

Une invitation ne peut être acceptée qu'avant son expiration et tant qu'elle reste `Pending`.

L'acceptation crée atomiquement un nouvel `OrganizationMember` avec le Role assigné lorsqu'aucun Membership historique n'existe.

Lorsqu'un OrganizationMember `Removed` existe déjà pour la même paire Organization–User, l'acceptation réactive atomiquement cette même entité, lui applique le Role assigné et passe l'invitation à `Accepted`.

Une invitation acceptée, refusée, annulée ou expirée ne peut jamais être réutilisée.

Le dernier Owner et les invariants de propriété de l'Organization restent protégés indépendamment des invitations.

---

## OrganizationActivityEntry *(post-MVP)*

### Description

L'`OrganizationActivityEntry` conserve les événements métier importants produits au sein d'une Organization.

Elle permet de retracer l'historique des principales opérations réalisées dans cet espace de travail.

---

### Exemples

* création d'un `Project` ;
* création d'un `Role` ;
* modification d'un `FormTemplate` ;
* création d'une `OrganizationInvitation`.

---

### Invariants

L'historique est immuable.

Les événements enregistrés ne sont jamais modifiés ni supprimés.

---

## Contrat métier

L'Organization constitue la frontière collaborative principale de SignalLab.

```
Organization
├── OrganizationMembers
├── Roles
├── OrganizationInvitations
├── Measures
├── FormTemplates
├── Projects
└── OrganizationActivityEntries *(post-MVP)*
```

Les `Measures` et les `FormTemplates` sont des ressources partagées par tous les `Projects` de l'Organization.

Les `Projects` possèdent ensuite leurs propres ressources opérationnelles.

Les `Users` restent indépendants des Organizations.

L'appartenance à une Organization est toujours matérialisée par un `OrganizationMember`.

---

## Périmètre MVP

Le MVP implémente :

* `Organization` ;
* `OrganizationMember` ;
* `OrganizationMemberStatus` ;
* `OrganizationInvitation` ;
* `OrganizationInvitationStatus` ;
* `Role` ;
* le **Permission Catalogue** ;
* `Project`.

Les autorisations sont exclusivement accordées par les `Roles`.

Chaque `OrganizationMember` possède exactement un rôle système actif parmi `Member`, `Administrator` et `Owner`.

Une Organization conserve toujours au moins un `Owner` actif.

---

## Hors MVP

Les évolutions prévues comprennent notamment :

* Roles d'Organization personnalisables ;
* attribution de plusieurs Roles à un même `OrganizationMember` ;
* groupes de `OrganizationMembers` ;
* héritage de `Roles` ;
* rôles temporaires ;
* `OrganizationActivityEntry` et audit avancé ;
* délégation d'administration ;
* organisations hiérarchiques ;
* artefacts binaires associés aux Builds ;
* stockage et hébergement des Builds ;
* distribution et téléchargement ;
* exécution intégrée ;
* artefacts multiples par Build.

---

## 4. Projects

### Vue d’ensemble

Un Project représente l’espace de travail principal dans lequel une équipe organise une activité de recherche cohérente.

Il appartient toujours à une Organization.

Organization
└── Project
    ├── ProjectMembers *(post-MVP)*
    ├── Builds
    ├── Campaigns
    ├── SavedAnalysis
    ├── ResearchBoards *(post-MVP)*
    ├── Labels *(post-MVP)*
    └── Activity *(post-MVP)*

Le Project constitue une frontière d’organisation, de collaboration et d’isolation des données de recherche.

Les ressources appartenant à l’Organization, comme les Measures et les FormTemplates, peuvent être utilisées par plusieurs Projects.

Les ressources appartenant à un Project ne sont jamais partagées implicitement avec les autres Projects.

---

### Project

#### Description

Un Project regroupe les ressources nécessaires à un ensemble cohérent d’études ou d’expérimentations.

Il permet à une équipe de centraliser :

* les Builds étudiés ;
* les Campaigns réalisées ;
* les analyses sauvegardées ;
* les ResearchBoards post-MVP ;
* les Labels post-MVP ;
* l’historique d’activité post-MVP.

Un Project ne peut pas exister sans Organization.

---

#### Responsabilités

Un Project est responsable :

* de son périmètre de recherche ;
* de ses Members autorisés par l'Organization dans le MVP ;
* de ses ProjectMembers post-MVP ;
* de ses Builds ;
* de ses Campaigns ;
* de ses SavedAnalysis ;
* de ses ResearchBoards post-MVP ;
* de ses Labels post-MVP ;
* de son historique métier post-MVP.

---

#### Principales propriétés

* `ProjectId`
* `OrganizationId`
* `Name`
* `Description`
* `Status`
* `CreatedByOrganizationMemberId`
* `CreatedAt`
* `UpdatedAt`
* `ArchivedAt`

---

#### ProjectStatus

```text
Active ⇄ Archived
```

Un Project nouvellement créé possède le statut `Active`.

Un Project `Active` peut recevoir de nouvelles ressources selon les Permissions applicables.

Un Project `Archived` reste consultable mais ne reçoit plus de nouvelle ressource.

L'archivage ne supprime aucune donnée historique.

La restauration replace le Project dans le statut `Active`.

---

#### Relations

Un Project appartient à exactement une Organization.

Un Project possède :

* plusieurs ProjectMembers post-MVP ;
* plusieurs Builds ;
* plusieurs Campaigns ;
* plusieurs SavedAnalysis ;
* plusieurs ResearchBoards post-MVP ;
* plusieurs Labels post-MVP ;
* plusieurs ProjectActivityEntries post-MVP.

Un Project utilise les Measures définies par son Organization.

Un Project peut utiliser les FormTemplates définis par son Organization.

---

#### Invariants

Un Project appartient toujours à une seule Organization.

Un Project ne peut pas être transféré vers une autre Organization.

Les Campaigns d’un Project ne peuvent pas être déplacées vers un autre Project.

Les Builds, SavedAnalysis, ResearchBoards post-MVP et Labels d’un Project ne peuvent pas changer de Project propriétaire.

La duplication d’une ressource vers un autre Project peut être prise en charge, mais elle produit toujours une nouvelle entité indépendante.

L’archivage d’un Project ne modifie pas les données historiques qu’il contient.

---

### ProjectMember *(post-MVP)*

#### Description

Un ProjectMember représente l’accès d’un OrganizationMember à un Project spécifique.

Il permet de restreindre l’accès à certains Projects sans retirer le User de l’Organization.

User
└── OrganizationMember
    └── ProjectMember

Un ProjectMember n’est jamais créé directement à partir d’un User.

Il référence obligatoirement un OrganizationMember appartenant à la même Organization que le Project.

---

#### Responsabilités

Un ProjectMember est responsable :

* de l’appartenance d’un OrganizationMember à un Project ;
* de son niveau d’accès au Project ;
* de ses éventuels Roles propres au Project ;
* de son statut dans le Project.

---

#### Principales propriétés

* `ProjectMemberId`
* `ProjectId`
* `OrganizationMemberId`
* `Status`
* `JoinedAt`
* `RemovedAt`

---

#### ProjectMemberStatus

Active
Removed

Un ProjectMember actif peut accéder au Project selon ses Roles et Permissions.

Un ProjectMember retiré conserve son existence historique, mais ne peut plus accéder au Project.

---

#### Relations

Un ProjectMember appartient à un Project.

Un ProjectMember référence un OrganizationMember.

Un OrganizationMember peut être ProjectMember de plusieurs Projects appartenant à son Organization.

Un ProjectMember peut posséder plusieurs ProjectRoles.

---

#### Invariants

L’OrganizationMember référencé doit appartenir à la même Organization que le Project.

Un OrganizationMember ne peut posséder qu’un seul ProjectMember actif pour un même Project.

La suppression d’un OrganizationMember de l’Organization retire nécessairement ses accès actifs aux Projects de cette Organization.

Le retrait d’un ProjectMember ne supprime jamais les données qu’il a créées.

---

### ProjectRole *(post-MVP)*

#### Description

Un ProjectRole définit un ensemble de Permissions applicables dans le périmètre d’un Project.

Il permet de distinguer les responsabilités globales dans l’Organization des responsabilités propres à un Project.

Par exemple, un User peut être simple Member de l’Organization mais `ProjectAdmin` sur un Project particulier.

---

#### Responsabilités

Un ProjectRole est responsable :

* du regroupement des Permissions applicables au Project ;
* de la définition d’un niveau d’accès réutilisable ;
* de l’attribution de responsabilités locales aux ProjectMembers.

---

#### Principales propriétés

* `ProjectRoleId`
* `ProjectId`
* `Name`
* `Description`
* `Permissions`
* `IsSystemRole`
* `CreatedAt`
* `UpdatedAt`

---

#### Relations

Un ProjectRole appartient à un seul Project.

Un ProjectRole peut être attribué à plusieurs ProjectMembers.

Un ProjectMember peut posséder plusieurs ProjectRoles.

---

#### Invariants

Un ProjectRole ne peut être utilisé que dans son Project propriétaire.

Les Permissions référencées par un ProjectRole doivent appartenir au **Permission Catalogue** défini par SignalLab.

Une Organization ne peut pas créer, modifier ou supprimer de Permissions dans le catalogue.

Les Roles d’Organization et les ProjectRoles sont des concepts distincts.

---

### Résolution des Permissions

Les Permissions effectives d’un User dans un Project sont calculées à partir de plusieurs sources.

OrganizationRoles
+
ProjectRoles
=
EffectiveProjectPermissions

Les OrganizationRoles peuvent accorder des Permissions globales.

Les ProjectRoles peuvent accorder des Permissions limitées au Project concerné.

Une Permission accordée au niveau de l’Organization reste valide dans ses Projects lorsque son périmètre le permet.

Une Permission accordée par un `ProjectRole` ne s’applique jamais à un autre Project.

---

#### Exemples de Permissions liées aux Projects

* `ViewProject`
* `EditProject`
* `ArchiveProject`
* `ManageProjectMembers`
* `ManageProjectRoles`
* `ManageBuilds`
* `ManageCampaigns`
* `ViewParticipants`
* `ViewAnalytics`
* `CreateSavedAnalysis`
* `ManageResearchBoards`
* `ExportProjectData`

Cette liste représente un catalogue métier évolutif défini par SignalLab.

Elle ne constitue pas une liste définitive d’implémentation.

---

### Build

#### Description

Un Build représente une version testable, précisément identifiable et historiquement traçable du produit, prototype, jeu, service ou expérience étudiée dans une ou plusieurs Campaigns.

Il répond à la question : **quelle version concrète du produit a été étudiée ?**

Dans le MVP, un Build est uniquement un enregistrement métier de référence.

Il ne représente pas nécessairement :

* un fichier ;
* un exécutable ;
* un installateur ;
* une archive ;
* un déploiement web ;
* une Release commerciale ;
* une session d'exécution.

Un Build appartient à un Project.

---

#### Responsabilités

Un Build est responsable :

* de l'identification de la version étudiée ;
* de sa plateforme cible ;
* de sa variante éventuelle ;
* de la référence permettant de localiser ou d'identifier cette version ;
* de ses métadonnées descriptives ;
* de sa disponibilité pour les Campaigns du Project ;
* de son historique d'utilisation.

Le Build n'est responsable ni du stockage, ni de la distribution, ni de l'exécution de la version étudiée.

---

#### Principales propriétés

* `BuildId`
* `ProjectId`
* `Name`
* `Version`
* `Platform`
* `Variant`
* `ProvenanceType`
* `ProvenanceValue`
* `Description`
* `Tags`
* `Notes`
* `Status`
* `CreatedByOrganizationMemberId`
* `CreatedAt`
* `UpdatedAt`
* `ArchivedAt`

`Variant` est facultatif.

`Version` est un libellé métier obligatoire. SignalLab n'impose aucun format de versionnement particulier.

---

#### Identité d'une version testable

Dans le MVP, une version testable est identifiée par la combinaison suivante :

```text
ProjectId
+ Version
+ Platform
+ Variant
+ ProvenanceType
+ ProvenanceValue
```

Un même numéro de version peut donc correspondre à plusieurs Builds lorsque la plateforme, la variante ou la provenance diffère.

Exemples :

```text
1.4.0 / Windows / Shipping
1.4.0 / Web / Production
1.4.0 / Windows / Experimental controls
```

Deux Builds dont tous les composants d'identité sont identiques ne peuvent pas coexister dans un même Project.

Le `BuildId` reste l'identité canonique utilisée par toutes les relations métier.

---

#### BuildProvenanceType

Le MVP prend en charge exactement deux types de provenance :

```text
External
Manual
```

Les deux types utilisent la même propriété texte `ProvenanceValue`.

Le type détermine uniquement la signification et la validation de cette valeur.

##### External

`External` indique que la version est accessible ou référencée par un lien externe.

Dans ce cas :

* `ProvenanceValue` est obligatoire ;
* sa valeur doit être une URL valide ;
* SignalLab conserve et affiche le lien sans télécharger la ressource ;
* SignalLab ne vérifie ni sa disponibilité, ni ses droits d'accès, ni sa durée de validité ;
* l'Organization reste entièrement responsable de la sécurité, du contrôle d'accès et du maintien de la ressource externe.

Le lien peut être utilisé directement par les Participants lorsque l'Organization l'a configuré à cette fin.

##### Manual

`Manual` indique que la version est localisée ou identifiée par une description libre.

Dans ce cas :

* `ProvenanceValue` est obligatoire ;
* sa valeur est une description textuelle non vide ;
* aucune structure technique particulière n'est imposée.

Exemples :

```text
Build installé sur les PC du laboratoire.
Archive transmise par l'équipe gameplay le 31 juillet.
Version disponible sur la machine VR numéro 3.
```

SignalLab ne cherche pas à interpréter cette description.

---

#### BuildStatus

```text
Active ⇄ Archived
```

Un Build nouvellement créé possède le statut `Active`.

Un Build `Active` peut être sélectionné par une Campaign `Draft`.

Un Build `Archived` :

* ne peut plus être sélectionné par une nouvelle Campaign ;
* ne peut pas permettre l'activation d'une Campaign `Draft` qui le référence déjà ;
* reste visible depuis les Campaigns qui le référencent ;
* conserve toutes ses informations et relations ;
* peut être restauré.

La restauration replace le Build dans le statut `Active` sans modifier les Campaigns qui le référencent.

---

#### Relations

Un Build appartient à exactement un Project.

Un Build peut être utilisé par plusieurs Campaigns appartenant au même Project.

Une Campaign `Draft` référence zéro ou un Build.

À partir de son activation, une Campaign référence exactement un Build.

Une Campaign ne devient jamais propriétaire du Build qu'elle référence.

L'association d'un Build à une Campaign ne crée aucune copie.

---

#### Invariants

Un Build ne peut jamais être utilisé par une Campaign appartenant à un autre Project.

Son identité est immuable dès sa création.

Elle comprend :

* le Project propriétaire ;
* la version ;
* la plateforme ;
* la variante éventuelle ;
* le type de provenance ;
* la valeur de provenance.

Modifier l'un de ces éléments signifie référencer une autre version testable et nécessite la création d'un nouveau Build.

Lorsqu'un Build est `Active`, seules ses métadonnées descriptives peuvent évoluer : nom d'affichage, description, tags et notes.

Un Build `Archived` est en lecture seule jusqu'à sa restauration.

Aucune suppression physique de Build n'est proposée dans le MVP, qu'il soit ou non référencé par une Campaign.

Les propriétés nécessaires à l'interprétation d'une Campaign doivent rester disponibles.

Une Campaign conserve la référence exacte du Build sélectionné lors de son activation.

---

#### Build et accès à l'expérience

La provenance du Build indique où la version étudiée peut être localisée ou comment elle peut être identifiée.

Le `CampaignContext` et le parcours de Participation portent les instructions propres à l'étude, notamment :

* la procédure d'installation ou de lancement ;
* le matériel requis ;
* les codes ou consignes d'accès communiqués par l'Organization ;
* les actions attendues du Participant ;
* les conditions particulières de l'expérience.

```text
Build.ProvenanceValue
→ où se trouve ou comment identifier la version

CampaignContext et Participation
→ comment l'utiliser dans cette étude précise
```

SignalLab ne gère pas les autorisations de la ressource externe et ne garantit pas qu'elle reste accessible.

---

#### Périmètre MVP

Le MVP permet :

* d'enregistrer manuellement une référence de Build ;
* de définir sa version, sa plateforme et sa variante ;
* de choisir une provenance `External` ou `Manual` ;
* de consulter ses informations ;
* de modifier ses métadonnées descriptives lorsqu'il est `Active` ;
* de le rechercher, le trier, l'archiver et le restaurer ;
* de l'associer à une Campaign `Draft` ;
* de figer cette association lors de l'activation.

Le MVP ne contient aucune connaissance de Git, d'un système de compilation ou d'une plateforme de distribution particulière.

---

#### Hors MVP

Le MVP exclut notamment :

* les fichiers binaires ;
* l'upload ;
* le stockage objet ;
* l'hébergement ;
* le téléchargement géré par SignalLab ;
* les liens temporaires générés par SignalLab ;
* les quotas de stockage ;
* l'extraction automatique de métadonnées ;
* les pipelines de compilation ;
* les intégrations GitHub, Steam ou itch.io ;
* l'exécution web intégrée ;
* le lancement automatique ;
* le suivi des sessions d'exécution.

Si ces capacités sont introduites ultérieurement, leurs responsabilités restent séparées du Build :

```text
Build
├── BuildArtifact
├── BuildDistribution
└── BuildExecution
```

Le Build demeure l'identité stable de la version étudiée.

Un `BuildArtifact` représenterait un fichier ou package associé.

Une `BuildDistribution` représenterait une manière de rendre un artefact accessible.

Une `BuildExecution` représenterait le lancement effectif d'une version dans un contexte donné.

Aucune infrastructure ou abstraction inutilisée n'est implémentée dans le MVP uniquement pour préparer ces évolutions.

---
### Label *(post-MVP)*

#### Description

Un Label est une ressource d’organisation propre à un Project.

Il permet de classifier les entités du Project selon les besoins de l’équipe.

Les Labels n’ont aucune signification métier globale imposée par SignalLab.

---

#### Responsabilités

Un Label est responsable :

* d’un nom ;
* d’une description optionnelle ;
* d’une représentation visuelle optionnelle ;
* de la classification des ressources du Project.

---

#### Principales propriétés

* `LabelId`
* `ProjectId`
* `Name`
* `Description`
* `Color`
* `CreatedAt`
* `UpdatedAt`

---

#### Relations

Un Label appartient à un Project.

Un Label peut être associé à plusieurs ressources du même Project.

Selon les fonctionnalités prises en charge, ces ressources peuvent notamment être :

* des Campaigns ;
* des Builds ;
* des SavedAnalysis ;
* des ResearchBoards post-MVP.

---

#### Invariants

Un Label ne peut être utilisé que dans son Project propriétaire.

L’association d’un Label à une ressource appartenant à un autre Project est interdite.

La suppression d’un Label retire ses associations sans supprimer les ressources associées.

---

### ProjectActivityEntry *(post-MVP)*

#### Description

Une ProjectActivityEntry représente un événement métier important survenu dans un Project.

Elle alimente l’historique d’activité du Project.

---

#### Responsabilités

Une ProjectActivityEntry conserve :

* la nature de l’action ;
* son auteur ;
* sa date ;
* l’entité concernée ;
* les métadonnées nécessaires à sa compréhension.

---

#### Principales propriétés

* `ProjectActivityEntryId`
* `ProjectId`
* `ActorOrganizationMemberId`
* `ActionType`
* `TargetEntityType`
* `TargetEntityId`
* `Metadata`
* `CreatedAt`

---

#### Exemples d’événements

* création d’un Build ;
* création d’une Campaign ;
* activation d’une Campaign ;
* archivage d’une Campaign ;
* ajout d’un ProjectMember ;
* retrait d’un ProjectMember ;
* création d’une SavedAnalysis ;
* modification d’un ResearchBoard post-MVP.

---

#### Invariants

Une ProjectActivityEntry est immuable.

Elle ne peut pas changer de Project.

La suppression ou l’archivage de l’entité référencée ne supprime pas automatiquement l’entrée d’activité.

L’historique doit rester compréhensible même si la ressource d’origine n’est plus active.

---

### Contrat métier

Le Project constitue la frontière de travail principale des équipes de recherche.

Organization
├── Measures
├── FormTemplates
└── Project
    ├── ProjectMembers
    ├── ProjectRoles
    ├── Builds
    ├── Campaigns
    ├── SavedAnalysis
    ├── ResearchBoards *(post-MVP)*
    ├── Labels
    └── Activity

Les Measures et FormTemplates sont partagés au niveau de l’Organization.

Les données opérationnelles de recherche appartiennent au Project.

Une Campaign appartient toujours à un seul Project.

Un Build appartient toujours à un seul Project.

Les SavedAnalysis et les ResearchBoards post-MVP appartiennent toujours à un seul Project.

---

### Périmètre MVP

Le MVP implémente :

* Project ;
* Build ;
* ProjectStatus ;
* BuildStatus ;
* l'isolation des données par Project ;
* l'obligation d'associer exactement un Build `Active` pour pouvoir activer une Campaign.

Dans le MVP, l'accès aux Projects est déterminé exclusivement par l'appartenance à l'Organization et par les Roles de l'Organization.

Aucune Permission, Membership ou Role propre au Project n'est implémenté.

---

### Hors MVP

Les évolutions prévues comprennent notamment :

* ProjectRoles entièrement personnalisables ;
* attribution fine de Roles par Project ;
* duplication de Campaigns entre Projects ;
* duplication de Builds ;
* archivage avancé ;
* ProjectActivity détaillée ;
* Labels configurables ;
* modèles de Project ;
* automatisations propres à un Project ;
* transfert de propriété interne ;
* permissions conditionnelles ;
* ProjectMember ;
* ProjectRole ;
* permissions spécifiques aux Projects ;
* restriction de certains membres d'une Organization à certains Projects ;
* groupes de ProjectMembers ;
* Labels ;
* ProjectActivity détaillée.

Le déplacement direct d’un Project vers une autre Organization reste exclu tant qu’aucun besoin métier explicite ne le justifie.

---

## 5. Ressources de recherche

### Vue d'ensemble

Les ressources de recherche définissent le vocabulaire et les modèles utilisés par les Projects d'une Organization.

Elles sont créées une seule fois puis réutilisées dans plusieurs Projects et plusieurs Campaigns.

Organization
├── Measures
└── FormTemplates

Ces ressources n'appartiennent jamais directement à un Project.

Les Projects les consomment afin de construire leurs Campaigns.

---

## Measure

### Description

Une Measure représente un concept métier que l'Organization souhaite comprendre et suivre de manière cohérente dans le temps.

Une Measure ne collecte aucune donnée directement.

Les Questions lui sont associées par des `QuestionMeasureBindings` explicites qui décrivent la manière dont chaque Response contribue au concept mesuré.

Une même Measure peut être utilisée dans plusieurs FormTemplates, plusieurs CampaignForms et plusieurs Campaigns.

Cette séparation permet de conserver une continuité analytique malgré l'évolution des questionnaires.

---

### Responsabilités

Une Measure est responsable :

* de représenter un concept analytique unique ;
* de fournir un vocabulaire commun à l'Organization ;
* d'assurer la continuité des analyses dans le temps ;
* de définir la direction d'interprétation produit de son score.

---

### Principales propriétés

* `MeasureId`
* `OrganizationId`
* `Key`
* `Name`
* `Description`
* `Category`
* `OutcomeDirection`
* `Status`
* `CreatedAt`
* `UpdatedAt`
* `ArchivedAt`

---

### MeasureOutcomeDirection

`OutcomeDirection` décrit la manière dont une valeur élevée de la Measure doit être interprétée du point de vue de l'expérience étudiée.

```text
HigherIsFavorable
HigherIsUnfavorable
Neutral
```

#### HigherIsFavorable

Une valeur élevée représente un résultat plus favorable.

Exemple : satisfaction, lisibilité ou engagement.

#### HigherIsUnfavorable

Une valeur élevée représente un résultat moins favorable.

Exemple : frustration, confusion ou inconfort.

#### Neutral

La Measure n'est pas orientée vers une notion générale de résultat favorable ou défavorable.

Exemple : temps de complétion lorsque sa valeur optimale dépend du contexte.

`OutcomeDirection` ne décrit jamais la direction d'une Question particulière.

La direction d'une Question par rapport à une Measure appartient au `QuestionMeasureBinding` correspondant.

---

### MeasureScore et OutcomeScore

Le `MeasureScore` représente la quantité réelle du concept mesuré sur une échelle harmonisée de `0` à `100`.

Exemple :

```text
Frustration = 80
```

signifie que le niveau de frustration mesuré est élevé.

Lorsque `OutcomeDirection` n'est pas `Neutral`, SignalLab peut également produire un `OutcomeScore` facilitant une lecture orientée vers un résultat favorable.

```text
HigherIsFavorable
OutcomeScore = MeasureScore

HigherIsUnfavorable
OutcomeScore = 100 - MeasureScore

Neutral
OutcomeScore indisponible
```

Le `OutcomeScore` est une projection analytique.

Il ne remplace jamais le `MeasureScore` et ne doit jamais être présenté comme la valeur brute de la Measure.

La direction finale d'une Response vers une lecture favorable est induite par la combinaison de la direction de la Measure et de la direction du binding.

```text
EffectiveOutcomeDirection
= MeasureOutcomeDirection × QuestionMeasureContributionDirection
```

Ainsi, une contribution inversée à une Measure défavorable produit bien une orientation favorable, selon le principe conceptuel `- × - = +`.

---

### MeasureStatus

```text
Active ⇄ Archived
```

Une Measure `Active` peut recevoir de nouveaux QuestionMeasureBindings.

Une Measure `Archived` ne peut plus être associée à une nouvelle Question.

Les FormTemplateVersions, CampaignForms, Responses et Analyses historiques continuent néanmoins d'utiliser les bindings qui la référencent.

La restauration replace la Measure dans le statut `Active`.

---

### Relations

Une Measure appartient à une Organization.

Une Measure peut être référencée par plusieurs QuestionMeasureBindings.

Une Measure peut être utilisée dans plusieurs FormTemplates et plusieurs CampaignForms par l'intermédiaire de ces bindings.

---

### Invariants

Une Measure ne change jamais de propriétaire.

Son `MeasureId`, son `OrganizationId` et sa clé canonique sont immuables.

Son identité sémantique est stable : une modification peut améliorer son nom, sa description ou sa catégorie, mais ne peut jamais changer le concept analytique représenté.

Une correction rédactionnelle est autorisée.

Une redéfinition du concept mesuré nécessite la création d'une nouvelle Measure.

`OutcomeDirection` fait partie de l'identité sémantique de la Measure.

Tant que la Measure n'a jamais été utilisée dans une FormTemplateVersion publiée ou un CampaignForm historique, sa direction peut être corrigée.

Dès qu'elle a été utilisée historiquement, une modification de direction qui changerait l'interprétation des résultats nécessite une nouvelle Measure.

Une Measure peut exister sans être utilisée.

Une Measure `Archived` est en lecture seule jusqu'à sa restauration.

L'archivage ne modifie jamais les Campaigns ou bindings historiques.

---

## FormTemplate

### Description

Un FormTemplate représente un modèle réutilisable de questionnaire.

Il sert de point de départ à la création des Campaigns.

Le FormTemplate n'est jamais utilisé directement pour collecter des réponses.

Lorsqu'une Campaign est créée, son FormTemplate est copié afin de produire un CampaignForm indépendant.

---

### Responsabilités

Le FormTemplate est responsable :

* de définir la structure d'un questionnaire ;
* d'organiser les Sections et les Questions ;
* de servir de modèle pour les futures Campaigns.

---

### Principales propriétés

* `FormTemplateId`
* `OrganizationId`
* `Name`
* `Description`
* `Status`
* `CreatedAt`
* `UpdatedAt`

---

### FormTemplateStatus

```text
Active ⇄ Archived
```

Un FormTemplate `Active` peut être modifié et publié.

Chaque publication produit une nouvelle `FormTemplateVersion` immuable sans modifier le statut du FormTemplate.

Un FormTemplate `Archived` reste consultable mais ne peut plus être modifié, publié ou sélectionné pour générer un nouveau CampaignForm.

La restauration replace le FormTemplate dans le statut `Active`.

Les indications suivantes sont des projections d'interface et non des statuts métier :

- aucune version publiée ;
- version publiée disponible ;
- modifications non publiées.

---

### Relations

Un FormTemplate appartient à une Organization.

Un FormTemplate possède plusieurs Sections.

Chaque Section possède plusieurs Questions.

Un FormTemplate peut être utilisé pour créer plusieurs CampaignForms.

---

### Invariants

Un FormTemplate `Active` reste modifiable et peut produire plusieurs publications successives.

Sa publication ne fige pas le FormTemplate lui-même : elle crée une nouvelle FormTemplateVersion immuable.

Un FormTemplate `Archived` est en lecture seule jusqu'à sa restauration.

Les modifications d'un FormTemplate n'affectent jamais les FormTemplateVersions ou CampaignForms existants.

Le FormTemplate reste la référence métier de son évolution.

La création d'une Campaign produit toujours une copie complète d'une FormTemplateVersion publiée.

---

## FormTemplateVersion

### Description

Chaque publication d'un FormTemplate produit une nouvelle version.

Les versions permettent d'identifier précisément le modèle ayant servi à créer une Campaign.

Le contenu historique reste consultable.

---

### Responsabilités

Une FormTemplateVersion est responsable :

* de conserver une version figée du questionnaire ;
* d'assurer la traçabilité des Campaigns ;
* de permettre les comparaisons entre versions.

---

### Principales propriétés

* `FormTemplateVersionId`
* `FormTemplateId`
* `VersionNumber`
* `PublishedAt`
* `PublishedByOrganizationMemberId`

---

### Relations

Une FormTemplateVersion appartient à un FormTemplate.

Un CampaignForm référence exactement une FormTemplateVersion.

---

### Invariants

Une FormTemplateVersion est entièrement immuable dès sa création.

Cette immutabilité couvre notamment :

* son identité et son numéro de version ;
* ses Sections et Questions ;
* leur ordre ;
* leurs types, libellés et descriptions ;
* leurs options, configurations et règles de validation ;
* leurs QuestionMeasureBindings, incluant les Measures, modes de contribution, directions et poids ;
* son auteur et sa date de publication.

Une erreur découverte après publication est corrigée dans le FormTemplate actif puis par la publication d'une nouvelle FormTemplateVersion.

La version précédente reste inchangée et consultable.

Une Campaign référence toujours la version exacte utilisée lors de sa création.

---

## Section

### Description

Une Section regroupe plusieurs Questions au sein d'un formulaire.

Elle permet de structurer la progression d'un Participant.

Les Sections n'ont pas de signification analytique propre.

---

### Responsabilités

Une Section est responsable :

* d'organiser les Questions ;
* de définir leur ordre d'affichage ;
* de faciliter la lecture du formulaire.

---

### Principales propriétés

* `SectionId`
* `Title`
* `Description`
* `Order`

---

### Relations

Une Section appartient à un seul formulaire.

Une Section possède plusieurs Questions.

---

### Invariants

L'ordre des Sections est propre à chaque formulaire.

Une Section ne peut appartenir qu'à un seul formulaire.

---

## QuestionType

### Description

`QuestionType` définit la structure de réponse, les règles de validation et la famille analytique déterministe d'une Question.

Le MVP prend en charge exactement les types suivants :

```text
Scale
Number
Boolean
SingleChoice
MultipleChoice
ShortText
LongText
```

---

### Scale

`Scale` représente une échelle ordonnée comportant au moins deux valeurs.

Sa configuration définit notamment un minimum, un maximum, un pas et les libellés d'extrémité éventuels.

Sa famille analytique est `Ordinal`.

Elle peut être utilisée par un binding `Scored` lorsque ses bornes sont valides.

---

### Number

`Number` représente une valeur numérique.

Sa configuration peut définir un minimum, un maximum, une précision et une unité d'affichage.

Sa famille analytique est `Scalar` uniquement lorsque des bornes minimale et maximale finies sont définies et distinctes.

Sans ces deux bornes, elle reste analysable comme valeur brute mais appartient à `NonScorable` pour les scores de Measure harmonisés du MVP.

---

### Boolean

`Boolean` représente une valeur à deux états.

Sa famille analytique est `Binary`.

Un binding `Scored` utilise structurellement `False = 0` et `True = 100`, avant application éventuelle de la direction `Inverted`.

---

### SingleChoice

`SingleChoice` représente un choix unique parmi une liste d'options.

Lorsque la configuration déclare explicitement les options comme ordonnées, sa famille analytique est `Ordinal`.

Sinon, sa famille est `Categorical` et elle ne peut pas participer à un binding `Scored`.

---

### MultipleChoice

`MultipleChoice` représente la sélection de zéro, une ou plusieurs options selon les règles configurées.

Sa famille analytique est `MultiCategorical`.

Il produit des effectifs et taux de sélection mais ne participe pas à un score harmonisé dans le MVP.

---

### ShortText et LongText

`ShortText` et `LongText` représentent des réponses textuelles libres.

Leur famille analytique est `Textual`.

Ils peuvent être associés à des Measures par des bindings `Contextual`, mais jamais `Scored` dans le MVP.

---

### Invariants

Le type et la configuration d'une Question déterminent sa famille analytique sans analyser son libellé.

Toute configuration doit être valide avant la publication d'une FormTemplateVersion ou l'activation d'une Campaign.

La copie vers une FormTemplateVersion puis un CampaignForm conserve intégralement le type et sa configuration historique.

Les dates, fichiers, médias, matrices, classements complexes et types personnalisés appartiennent au périmètre post-MVP.

---

## Question

### Description

Une Question représente une information demandée au Participant.

Elle constitue la plus petite unité de collecte du questionnaire.

Une Question peut être reliée à zéro, une ou plusieurs Measures par des `QuestionMeasureBindings` explicites.

SignalLab ne déduit jamais ces relations à partir du libellé de la Question.

---

### Responsabilités

Une Question est responsable :

* de son libellé ;
* de son type ;
* de ses règles de validation ;
* de sa configuration de réponse ;
* de ses QuestionMeasureBindings éventuels.

---

### Principales propriétés

* `QuestionId`
* `Type`
* `Label`
* `Description`
* `Required`
* `Configuration`
* `Order`
* `MeasureBindings`

---

### Relations

Une Question appartient à une Section.

Une Question possède zéro, un ou plusieurs QuestionMeasureBindings.

Les Responses référencent les Questions du CampaignForm et jamais directement les Questions du FormTemplate.

---

### Invariants

Une Question appartient à une seule Section.

Une Question ne contient aucun `MeasureId` direct.

Toute relation analytique avec une Measure est portée par un QuestionMeasureBinding.

Une Question peut contribuer à plusieurs Measures avec des directions et des poids différents.

SignalLab ne déduit jamais une Measure, une direction ou un poids depuis le texte de la Question.

Les Questions et leurs bindings copiés dans un CampaignForm deviennent immuables lors de l'activation de la Campaign.

---

## QuestionMeasureBinding

### Description

Un `QuestionMeasureBinding` représente la relation analytique explicite entre une Question et une Measure.

Il décrit si et comment la Response à cette Question contribue à la compréhension de la Measure.

Cette relation porte sa propre direction et son propre poids.

---

### Principales propriétés

* `QuestionMeasureBindingId`
* `QuestionId`
* `MeasureId`
* `ContributionMode`
* `ContributionDirection`
* `Weight`

---

### QuestionMeasureContributionMode

```text
Scored
Contextual
```

#### Scored

La Response peut contribuer quantitativement au score de la Measure.

Une relation `Scored` n'est valide que si le type et la configuration de la Question permettent de produire une valeur scalaire déterministe.

#### Contextual

La Question enrichit l'interprétation de la Measure sans participer à son score quantitatif.

Cela concerne notamment :

* les Questions textuelles ;
* les explications qualitatives ;
* les choix catégoriels non ordonnés ;
* toute information reliée au concept mais non convertible en valeur scalaire justifiée.

Une Question peut être `Scored` pour une Measure et `Contextual` pour une autre.

---

### QuestionMeasureContributionDirection

```text
Aligned
Inverted
```

#### Aligned

Une valeur de Response élevée représente davantage de la Measure.

#### Inverted

Une valeur de Response élevée représente moins de la Measure.

La direction est définie manuellement par le chercheur.

Elle n'est jamais déduite du libellé de la Question.

---

### Weight

`Weight` représente l'importance relative de cette Question parmi les Questions qui contribuent à la même Measure.

Dans le MVP :

```text
Weight > 0
DefaultWeight = 1.0
```

Les poids :

* n'ont pas besoin de totaliser `1`, `100` ou toute autre valeur fixe ;
* sont normalisés automatiquement pendant le calcul ;
* sont indépendants entre plusieurs Measures associées à la même Question ;
* ne représentent jamais une répartition d'un capital limité entre les Measures.

Exemple :

```text
Question : « Les impacts étaient faciles à lire »

Lisibilité des combats
├── Scored
├── Aligned
└── Weight 1.0

Satisfaction des combats
├── Scored
├── Aligned
└── Weight 0.4
```

---

### Relations

Un QuestionMeasureBinding appartient à exactement une Question.

Il référence exactement une Measure de la même Organization que le formulaire.

Une Question peut posséder plusieurs bindings.

Une Measure peut être référencée par plusieurs bindings.

---

### Invariants

Une même paire Question–Measure ne peut apparaître qu'une seule fois dans un même formulaire.

Une relation `Scored` possède obligatoirement une `ContributionDirection` et un `Weight` strictement positif.

Une relation `Contextual` ne participe à aucun score de Measure.

Le choix de la Measure, du mode, de la direction et du poids relève toujours d'une configuration explicite du chercheur.

Aucune IA, analyse sémantique, recommandation automatique ou détection par mots-clés n'intervient dans cette relation.

Lorsqu'un FormTemplateVersion est publié, ses bindings deviennent immuables avec les Questions correspondantes.

Lorsqu'un CampaignForm est créé, les bindings sont copiés intégralement et deviennent indépendants de leur source.

Ils peuvent être adaptés tant que la Campaign est `Draft`, puis deviennent définitivement immuables lors de son activation.

---

## Contrat métier

Les ressources de recherche définissent le langage commun de l'Organization.

```text
Organization
├── Measures
└── FormTemplates
        └── Questions
                └── QuestionMeasureBindings
                        └── Measures
```

Les Measures représentent les concepts métier.

Les Questions collectent les Responses.

Les QuestionMeasureBindings définissent explicitement comment ces Responses contribuent aux Measures.

Les FormTemplateVersions et CampaignForms conservent une copie historique immuable de ces relations.

---

## Périmètre MVP

Le MVP implémente :

* `Measure` ;
* `MeasureOutcomeDirection` ;
* `FormTemplate` ;
* `Section` ;
* `Question` ;
* le catalogue `QuestionType` du MVP ;
* `QuestionMeasureBinding` ;
* plusieurs Measures par Question ;
* les modes `Scored` et `Contextual` ;
* les directions `Aligned` et `Inverted` ;
* les poids relatifs ;
* la copie vers `CampaignForm` ;
* le versioning des FormTemplates.

Toutes les FormTemplateVersions publiées restent consultables.

La version publiée la plus récente peut être proposée par défaut lors de la génération d'un CampaignForm, sans rendre les versions précédentes inactives.

---

## Hors MVP

Les évolutions prévues comprennent notamment :

* types de Questions supplémentaires ou personnalisés ;
* bibliothèque de Questions réutilisables ;
* Sections conditionnelles ;
* logique avancée de navigation ;
* validation personnalisée ;
* branches conditionnelles ;
* localisation avancée ;
* import/export de FormTemplates ;
* comparaison graphique entre versions ;
* suggestion automatique de Measures ;
* détection automatique de polarité ;
* calibrations statistiques personnalisées ;
* mappings de valeurs configurables.

---

## 6. Campaigns et CampaignForms

### Vue d'ensemble

Une Campaign représente une étude de recherche unique menée dans le cadre d'un Project.

Elle constitue l'unité principale d'exécution de la recherche dans SignalLab.

Chaque Campaign possède exactement :

* un Project ;
* un CampaignForm ;
* un cycle de vie ;
* des CampaignParticipations ;
* des Submissions.

Une Campaign `Draft` peut temporairement ne référencer aucun Build.

À partir de son activation, elle référence exactement un Build.

Project
└── Campaign
    ├── Build
    ├── CampaignForm
    ├── CampaignParticipations
    └── Submissions

Une Campaign ne peut jamais exister indépendamment d'un Project.

Une Campaign ne peut jamais contenir plusieurs CampaignForms.

---

### Campaign

#### Description

Une Campaign représente une étude distincte, réalisée sur un Build donné, à l'aide d'un questionnaire figé.

Elle regroupe toutes les informations nécessaires à la collecte et à l'interprétation des réponses.

Une Campaign appartient toujours à un seul Project.

---

#### Responsabilités

Une Campaign est responsable :

* de l'identification de l'étude ;
* du Build étudié ;
* du CampaignForm utilisé ;
* de son contexte de recherche ;
* de sa période d'activité ;
* de ses règles de participation ;
* de sa définition de consentement éventuelle ;
* de son cycle de vie ;
* de ses CampaignParticipations ;
* de ses Submissions.

---

#### Principales propriétés

* `CampaignId`
* `ProjectId`
* `BuildId` *(facultatif en `Draft`, obligatoire à partir de l'activation)*
* `Name`
* `Description`
* `Status`
* `Context`
* `ParticipationSettings`
* `ConsentDefinition`
* `ScheduledStartAt` *(post-MVP)*
* `ScheduledEndAt` *(post-MVP)*
* `ActivatedAt`
* `CompletedAt`
* `ArchivedAt`
* `CreatedByOrganizationMemberId`
* `CreatedAt`
* `UpdatedAt`

Le `CampaignForm` est une entité détenue par la Campaign et n'est pas représenté uniquement par un identifiant externe.

---

#### Relations

Une Campaign appartient à exactement un Project.

Une Campaign `Draft` référence zéro ou un Build appartenant au même Project.

À partir de son activation, elle référence exactement un Build.

Une Campaign possède exactement un CampaignForm.

Une Campaign possède plusieurs CampaignParticipations.

Une Campaign peut posséder plusieurs Submissions par l'intermédiaire de ses CampaignParticipations.

Une Campaign peut être utilisée dans plusieurs SavedAnalysis et, post-MVP, dans plusieurs ResearchBoards appartenant au même Project.

---

#### Invariants

Une Campaign appartient toujours à un seul Project.

Une Campaign ne peut pas être déplacée vers un autre Project.

Son `CampaignId`, son `ProjectId`, son créateur et sa date de création sont immuables.

Le Build éventuel d'une Campaign `Draft` doit appartenir au même Project.

Une Campaign possède toujours exactement un CampaignForm avant son activation.

Une Campaign ne peut pas être activée sans Build `Active` appartenant à son Project.

Une Campaign ne peut pas être activée sans CampaignForm valide.

Une Campaign Active ne peut plus changer de Build.

Une Campaign Active ne peut plus modifier son CampaignForm, son CampaignContext historique ou ses règles de participation figées.

Les données historiques d'une Campaign ne sont jamais supprimées par son archivage.

---

#### Règles d'immutabilité

Tant que la Campaign est `Draft`, peuvent notamment évoluer :

* le Build associé ;
* le CampaignForm ;
* le CampaignContext ;
* les paramètres d'accès et de participation ;
* le nom et la description.

Lors de l'activation, deviennent définitivement immuables :

* le `BuildId` ;
* le CampaignForm complet ;
* le CampaignContext nécessaire à l'interprétation de l'étude ;
* les instructions et informations présentées aux Participants ;
* les conditions et règles de consentement ;
* les paramètres d'accès et de participation ;
* la limite de Submissions autorisées.

Après l'activation, peuvent uniquement évoluer :

* le statut selon les transitions autorisées ;
* les timestamps correspondant à ces transitions ;
* le nom et la description internes lorsqu'ils servent uniquement au classement et n'altèrent ni les informations présentées aux Participants ni l'interprétation des Responses.

Toute information destinée aux Participants ou nécessaire à l'interprétation historique appartient au CampaignContext ou à une configuration figée, et non aux métadonnées internes modifiables.

La complétion ou l'archivage ne défige jamais la Campaign.

---

### CampaignStatus

Le cycle de vie du MVP repose sur les statuts suivants :

```text
Draft
Active
Completed
Archived
```

Le statut indique l'état métier actuel de la Campaign.

Il ne doit pas être déduit uniquement de ses dates.

---

### Draft

#### Description

`Draft` représente une Campaign en cours de préparation.

La Campaign n'est pas encore ouverte aux Participants.

#### Modifications autorisées

Tant qu'une Campaign est en `Draft`, sa configuration peut être préparée et modifiée selon les Permissions applicables.

#### Transitions autorisées

```text
Draft → Active
Draft → Archived
```

Le passage à `Active` constitue une activation métier explicite.

L'archivage depuis `Draft` permet d'abandonner la préparation sans supprimer la Campaign.

---

### Active

#### Description

`Active` représente une Campaign dont la collecte est ouverte.

Les mécanismes de recrutement du MVP peuvent créer de nouvelles CampaignParticipations.

Les Participants peuvent commencer leur parcours et produire une Submission.

#### Effets de l'activation

L'activation renseigne `ActivatedAt`.

Elle applique les règles de gel définies par le domaine.

#### Transitions autorisées

```text
Active → Completed
```

Une Campaign `Active` ne peut pas revenir en `Draft` dans le MVP.

---

### Completed

#### Description

`Completed` représente une Campaign dont la collecte est terminée.

Elle n'accepte plus de nouvelle CampaignParticipation ni de nouvelle Submission.

Ses données restent disponibles pour les Results et les Analyses.

#### Effets de la complétion

La complétion renseigne `CompletedAt`.

Les CampaignParticipations qui ne peuvent plus être poursuivies sont clôturées selon les règles applicables.

#### Transitions autorisées

```text
Completed → Archived
```

Une Campaign `Completed` ne peut pas être réouverte dans le MVP.

---

### Archived

#### Description

`Archived` représente une Campaign retirée de l'activité courante.

Elle reste conservée à des fins historiques et analytiques.

Une Campaign `Archived` :

- ne peut pas accepter de nouvelle CampaignParticipation ;
- ne peut pas accepter de nouvelle Submission ;
- reste consultable selon les Permissions ;
- reste utilisable dans les analyses historiques ;
- conserve toutes ses relations.

Le statut `Archived` est terminal dans le MVP.

---

### Règles de transition du MVP

```text
Draft ─────→ Active ─────→ Completed ─────→ Archived
  └────────────────────────────────────────→ Archived
```

Les transitions non définies sont interdites.

Une transition ne peut être effectuée que si tous les invariants du statut cible sont respectés.

---

### Cycle de vie post-MVP

La cible fonctionnelle pourra introduire notamment :

- le statut `Scheduled` ;
- les transitions `Draft → Scheduled`, `Scheduled → Draft` et `Scheduled → Active` ;
- le recrutement précédant l'activation ;
- la réouverture `Completed → Active` ;
- la restauration d'une Campaign archivée.

Ces capacités ne font pas partie du cycle de vie implémenté dans le MVP.

---

### Activation d'une Campaign

L'activation représente une opération métier explicite.

Elle ne consiste pas uniquement à changer la valeur du statut.

Avant l'activation, SignalLab doit vérifier que :

* le Project est Active ;
* le Build existe et appartient au Project ;
* le Build possède le statut `Active` ;
* le CampaignForm existe ;
* le CampaignForm contient une structure valide ;
* toutes les Questions sont valides ;
* toutes les Measures référencées appartiennent à l'Organization ;
* les paramètres de participation sont cohérents ;
* la définition de consentement est valide lorsqu'un consentement explicite est requis ;
* les dates éventuelles sont cohérentes.

Si l'une de ces conditions échoue, l'activation est refusée.

---

### CampaignForm

#### Description

Le CampaignForm représente le questionnaire propre à une Campaign.

Il est créé à partir d'une copie d'une FormTemplateVersion.

Une fois créé, il devient indépendant de son FormTemplate d'origine.

Le CampaignForm constitue la source de vérité historique concernant les Questions réellement présentées aux Participants.

---

#### Responsabilités

Le CampaignForm est responsable :

* de la structure du questionnaire ;
* de l'ordre des Sections ;
* de l'ordre des Questions ;
* du contenu des Questions ;
* des règles de validation ;
* des options disponibles ;
* des QuestionMeasureBindings ;
* de la provenance du FormTemplate utilisé.

---

#### Principales propriétés

* `CampaignFormId`
* `CampaignId`
* `SourceTemplateId`
* `SourceTemplateVersionId`
* `Title`
* `Description`
* `Sections`
* `CreatedAt`
* `FrozenAt`

`SourceTemplateId` et `SourceTemplateVersionId` sont optionnels uniquement lorsqu'un CampaignForm peut être créé sans FormTemplate.

Dans le périmètre actuellement retenu, la création à partir d'un FormTemplate constitue le workflow normal.

---

#### Relations

Un CampaignForm appartient à exactement une Campaign.

Un CampaignForm contient plusieurs CampaignFormSections.

Chaque CampaignFormSection contient plusieurs CampaignFormQuestions.

Un CampaignForm peut référencer plusieurs Measures appartenant à l'Organization du Project.

---

#### Invariants

Un CampaignForm ne peut appartenir qu'à une seule Campaign.

Son `CampaignFormId`, son `CampaignId`, sa date de création et ses références de provenance sont immuables dès sa création.

Un CampaignForm ne peut pas être partagé entre plusieurs Campaigns.

La modification du FormTemplate source ne modifie jamais le CampaignForm.

Le CampaignForm ne synchronise jamais son contenu avec le FormTemplate source.

Tant que la Campaign est `Draft`, son contenu peut être adapté selon les Permissions applicables.

Lors de l'activation, le CampaignForm devient intégralement immuable, notamment pour :

* son titre et sa description ;
* sa provenance ;
* ses Sections et Questions ;
* leur ordre ;
* leurs types, libellés et descriptions ;
* leurs options, configurations et règles de validation ;
* leurs CampaignFormQuestionMeasureBindings copiés, avec leurs modes, directions et poids.

`FrozenAt` est renseigné lors de l'activation et ne peut ensuite plus être modifié.

Une Campaign `Completed` ou `Archived` ne défige jamais son CampaignForm.

---

### Copie d'un FormTemplate

La création d'un CampaignForm à partir d'un FormTemplate produit une copie complète et indépendante.

FormTemplate
└── FormTemplateVersion
        │
        │ copie
        ▼
CampaignForm

La copie inclut notamment :

* les Sections ;
* les Questions ;
* leur ordre ;
* leur type ;
* leur libellé ;
* leurs descriptions ;
* leurs options ;
* leurs règles de validation ;
* leurs CampaignFormQuestionMeasureBindings copiés, avec leurs modes, directions et poids ;
* toutes les données nécessaires à leur interprétation.

La copie conserve uniquement une référence de provenance vers la version source.

---

#### Règle d'indépendance

Après la copie :

* une modification du FormTemplate n'affecte pas le CampaignForm ;
* une modification du CampaignForm n'affecte pas le FormTemplate ;
* aucune synchronisation automatique ou manuelle n'est réalisée ;
* aucune relation de dépendance fonctionnelle ne subsiste.

La provenance sert uniquement à la traçabilité.

---

### CampaignFormSection

#### Description

Une CampaignFormSection représente une Section copiée dans le CampaignForm.

Elle appartient exclusivement au CampaignForm concerné.

---

#### Principales propriétés

* `CampaignFormSectionId`
* `CampaignFormId`
* `SourceSectionId`
* `Title`
* `Description`
* `Order`

`SourceSectionId` peut être conservé à des fins de traçabilité, mais ne crée aucune dépendance avec la Section source.

---

#### Relations

Une CampaignFormSection appartient à un CampaignForm.

Une CampaignFormSection possède plusieurs CampaignFormQuestions.

---

#### Invariants

Une CampaignFormSection ne peut pas changer de CampaignForm.

Son ordre est propre au CampaignForm.

Elle devient immuable lors de l'activation de la Campaign.

---

### CampaignFormQuestion

#### Description

Une CampaignFormQuestion représente une Question telle qu'elle existe dans une Campaign précise.

Elle constitue la référence directe utilisée par les Responses.

Elle ne dépend plus de la Question contenue dans le FormTemplate source.

---

#### Principales propriétés

* `CampaignFormQuestionId`
* `CampaignFormSectionId`
* `SourceQuestionId`
* `Type`
* `Label`
* `Description`
* `Required`
* `Configuration`
* `Order`
* `MeasureBindings`

`SourceQuestionId` sert uniquement à la traçabilité.

---

#### Relations

Une CampaignFormQuestion appartient à une CampaignFormSection.

Une CampaignFormQuestion possède zéro, un ou plusieurs CampaignFormQuestionMeasureBindings copiés.

Une CampaignFormQuestion peut posséder plusieurs Responses provenant de Submissions différentes.

---

#### Invariants

Une Response référence toujours une CampaignFormQuestion.

Une Response ne référence jamais directement une Question de FormTemplate.

Toutes les Measures référencées par ses bindings appartiennent à l'Organization propriétaire du Project.

Tant que la Campaign est `Draft`, la CampaignFormQuestion et ses bindings peuvent être adaptés selon les Permissions applicables.

Lors de l'activation, la CampaignFormQuestion et tous ses bindings deviennent définitivement immuables.

---

### CampaignFormQuestionMeasureBinding

#### Description

Un `CampaignFormQuestionMeasureBinding` représente la copie propre à une Campaign d'un QuestionMeasureBinding provenant d'une FormTemplateVersion.

Il constitue la source de vérité historique utilisée par les Analytics pour interpréter une Response.

Il ne reste jamais synchronisé avec le binding source.

---

#### Principales propriétés

* `CampaignFormQuestionMeasureBindingId`
* `CampaignFormQuestionId`
* `SourceQuestionMeasureBindingId`
* `MeasureId`
* `ContributionMode`
* `ContributionDirection`
* `Weight`

`SourceQuestionMeasureBindingId` sert uniquement à la traçabilité.

---

#### Relations

Un CampaignFormQuestionMeasureBinding appartient à exactement une CampaignFormQuestion.

Il référence exactement une Measure appartenant à l'Organization propriétaire du Project.

---

#### Invariants

Le binding copié conserve exactement :

* la Measure ;
* le mode de contribution ;
* la direction ;
* le poids.

Une même paire CampaignFormQuestion–Measure ne peut apparaître qu'une seule fois.

Une modification du FormTemplate ou du QuestionMeasureBinding source ne modifie jamais cette copie.

Le binding peut être adapté tant que la Campaign est `Draft`.

Lors de l'activation, il devient définitivement immuable et reste la relation utilisée par toutes les Analyses historiques de la Campaign.

---

### CampaignContext

#### Description

Le CampaignContext décrit les conditions dans lesquelles l'expérience étudiée est réalisée.

Il permet de conserver les informations nécessaires à l'interprétation des résultats.

Le CampaignContext constitue un concept métier distinct de la description générale de la Campaign.

---

#### Responsabilités

Le CampaignContext peut notamment décrire :

* le type d'expérience ;
* le mode de participation ;
* l'environnement de test ;
* le matériel utilisé ;
* la durée attendue ;
* les instructions générales ;
* les conditions particulières de l'étude.

---

#### Principales propriétés

Le CampaignContext peut contenir :

* `ExperienceType`
* `ParticipationMode`
* `Environment`
* `Platform`
* `ExpectedDuration`
* `Instructions`
* `AdditionalContext`

La structure exacte peut évoluer selon les besoins métier identifiés.

Le Domain Model conserve néanmoins `CampaignContext` comme concept explicite.

---

#### Invariants

Le CampaignContext appartient à une seule Campaign et ne peut jamais être transféré.

Il reste modifiable tant que la Campaign est `Draft`.

Lors de l'activation, toutes les informations nécessaires à l'interprétation historique ou présentées aux Participants deviennent immuables.

Les métadonnées internes sans incidence analytique doivent rester portées par la Campaign elle-même et ne doivent pas être mélangées au CampaignContext figé.

---

### CampaignParticipationSettings

#### Description

Les CampaignParticipationSettings définissent les règles générales d'accès et de participation à une Campaign.

Ils ne représentent pas les mécanismes de recrutement eux-mêmes.

---

#### Principales propriétés

* `MaxSubmissionsPerParticipant`
* `RecruitmentStartsAt`
* `RecruitmentEndsAt`
* `AllowParticipantWithdrawal`
* `RequireExplicitConsent`
* `AccessMode`

---

#### AccessMode

Les modes d'accès peuvent notamment inclure :

InvitationOnly
AccessLink
Application
Announcement

Tous les modes ne sont pas nécessairement disponibles dans le MVP.

---

#### Invariants

`MaxSubmissionsPerParticipant` doit être supérieur ou égal à un.

Dans le MVP :

```text
MaxSubmissionsPerParticipant = 1
AccessMode = AccessLink
RequireExplicitConsent = true ou false selon la Campaign
AllowParticipantWithdrawal = true ou false selon la Campaign
```

`RecruitmentStartsAt` et `RecruitmentEndsAt` ne sont pas configurables dans le MVP.

Les dates de participation doivent être cohérentes avec le cycle de vie de la Campaign.

Une Campaign non Active ne peut pas accepter de nouvelle Submission.

Dans le MVP, les CampaignParticipations ne peuvent être créées que lorsque la Campaign est `Active`.

Les paramètres de participation restent modifiables en `Draft` puis deviennent immuables lors de l'activation lorsqu'ils affectent :

* l'accès à l'étude ;
* l'éligibilité ;
* le consentement ;
* le déroulement du parcours ;
* le nombre de Submissions autorisées ;
* l'interprétation des données collectées.

Les périodes distinctes de recrutement et de collecte, ainsi que le recrutement d'une Campaign `Scheduled`, appartiennent à la cible post-MVP.

---

### CampaignConsentDefinition

#### Description

La `CampaignConsentDefinition` décrit les conditions que le Participant doit accepter pour créer sa CampaignParticipation lorsque `RequireExplicitConsent` est activé.

Elle constitue la source dont est produit le `ConsentSnapshot` historique.

---

#### Principales propriétés

* `Title`
* `Content`
* `AdditionalConfirmationLabel`

`AdditionalConfirmationLabel` est facultatif.

---

#### Invariants

`CampaignParticipationSettings.RequireExplicitConsent` constitue l'unique source de vérité indiquant si un consentement explicite est requis.

Lorsque `RequireExplicitConsent = true`, la Campaign doit posséder exactement une CampaignConsentDefinition avant son activation, et son titre comme son contenu sont obligatoires et non vides.

Lorsque `RequireExplicitConsent = false`, aucune CampaignConsentDefinition n'est nécessaire et aucun ConsentSnapshot n'est créé.

La CampaignConsentDefinition appartient à une seule Campaign.

Elle reste modifiable tant que la Campaign est `Draft`.

Lors de l'activation, elle devient immuable avec les autres informations présentées aux Participants.

Lors de l'acceptation d'une Campaign exigeant un consentement explicite, son contenu effectif est copié dans le `ConsentSnapshot` de la CampaignParticipation.

Une modification ultérieure d'une définition de consentement post-MVP ne réécrit jamais les ConsentSnapshots existants.

---

### Modification après activation

Après l'activation, toute modification susceptible de changer la signification des données est interdite.

Cela inclut notamment :

* remplacer le Build ;
* ajouter une Question ;
* retirer une Question ;
* déplacer une Question ;
* modifier le type d'une Question ;
* modifier ses options ;
* modifier ses règles de validation ;
* modifier ses QuestionMeasureBindings ;
* modifier une Section ;
* remplacer le CampaignForm.

Lorsqu'une équipe souhaite modifier l'un de ces éléments, elle doit créer une nouvelle Campaign.

La duplication de la Campaign existante pourra servir de point de départ lorsque cette capacité post-MVP sera disponible.

---

### Duplication d'une Campaign

Une Campaign peut être dupliquée dans son Project.

Cette capacité appartient à la vision post-MVP et n'est pas implémentée dans le MVP actuel.

La duplication crée :

* une nouvelle Campaign ;
* un nouveau CampaignForm indépendant ;
* de nouveaux identifiants ;
* aucun Participant ;
* aucune CampaignParticipation ;
* aucune Submission ;
* aucune Response.

La nouvelle Campaign commence en `Draft`.

Elle peut conserver une référence au Build d'origine si celui-ci reste utilisable dans le même Project.

La duplication ne crée aucun lien de synchronisation entre les Campaigns.

---

### Contrat métier

Le modèle de Campaign repose sur les règles suivantes :

Project
└── Campaign
    ├── zéro ou un Build en Draft
    ├── exactement un Build à partir de l'activation
    ├── exactement un CampaignForm
    ├── un CampaignContext
    ├── des ParticipationSettings
    ├── une CampaignConsentDefinition éventuelle
    └── des CampaignParticipations
        └── des Submissions
            └── des Responses

Le CampaignForm est une copie indépendante d'une FormTemplateVersion.

Le Build et le CampaignForm deviennent figés à l'activation.

Une Campaign ne contient jamais plusieurs formulaires.

Une nouvelle configuration d'étude nécessite une nouvelle Campaign.

---

### Périmètre MVP

Le MVP implémente :

* `Campaign` ;
* `CampaignStatus` ;
* `CampaignForm` ;
* `CampaignFormSection` ;
* `CampaignFormQuestion` ;
* `CampaignContext` dans une version limitée ;
* `CampaignParticipationSettings` ;
* `CampaignConsentDefinition` ;
* les statuts `Draft`, `Active`, `Completed` et `Archived` ;
* les transitions nécessaires entre ces statuts ;
* l'activation manuelle immédiate ;
* une seule Submission par Participant ;
* la copie d'une FormTemplateVersion ;
* le gel du Build et du CampaignForm lors de l'activation.

---

### Hors MVP

Les évolutions prévues peuvent comprendre :

* récurrence de Campaigns ;
* vagues de collecte ;
* duplication avancée entre Projects ;
* restauration d'une Campaign Archived ;
* règles conditionnelles de planification ;
* plusieurs Submissions par Participant ;
* modes de participation supplémentaires ;
* CampaignContext enrichi ;
* fermeture automatique selon un quota ;
* activation conditionnée par une validation ;
* workflows d'approbation ;
* historique détaillé des transitions de statut ;
* statut Scheduled ;
* planification du lancement ;
* retour de Scheduled vers Draft ;
* recrutement précédant l'activation ;
* duplication d'une Campaign dans son Project ;
* duplication d'une Campaign entre Projects.

Aucune de ces évolutions ne remet en cause les invariants selon lesquels une Campaign possède exactement un CampaignForm et référence exactement un Build à partir de son activation.

---

## 7. Recrutement et participation

### Vue d'ensemble

Une Campaign peut recruter des Participants par différents mécanismes.

Quel que soit le mécanisme utilisé, la relation métier créée est toujours une `CampaignParticipation`.

Les mécanismes de recrutement ne constituent pas des relations métier distinctes.

Ils représentent uniquement différentes façons d'initier une participation.

Campaign
├── Recruitment
│   ├── Invitations
│   ├── AccessLinks
│   ├── Announcements
│   └── Applications
│
└── CampaignParticipations
        └── Submissions

Une `CampaignParticipation` constitue la source de vérité concernant la participation d'un `ParticipantProfile` à une `Campaign`.

---

### Disponibilité du recrutement

Dans le MVP, les mécanismes de recrutement et la création de CampaignParticipations ne sont disponibles que lorsque la Campaign est `Active`.

Une Campaign `Draft`, `Completed` ou `Archived` ne peut pas créer de nouvelle CampaignParticipation.

L'ouverture d'un lien d'accès ne crée jamais à elle seule une CampaignParticipation.

La CampaignParticipation est créée uniquement après l'acceptation des conditions requises par le Participant.

Le recrutement d'une Campaign `Scheduled` et la création d'une participation avant l'activation appartiennent à la cible post-MVP.

---

## CampaignParticipation

### Description

Une `CampaignParticipation` représente la relation métier entre un `ParticipantProfile` et une `Campaign`.

Dans le MVP, elle est créée après l'acceptation des conditions requises depuis un mécanisme d'accès valide.

Elle existe indépendamment de la `Submission` qu'elle pourra contenir.

---

### Responsabilités

Une `CampaignParticipation` est responsable :

- de représenter la participation d'un Participant à une Campaign ;
- de suivre son état d'avancement ;
- de conserver les informations de consentement ;
- de référencer sa Submission ;
- de conserver les métadonnées de participation.

---

### Principales propriétés

- `CampaignParticipationId`
- `CampaignId`
- `ParticipantProfileId`
- `Status`
- `RecruitmentSource`
- `DemographicSnapshot`
- `ConsentSnapshot` *(facultatif)*
- `AcceptedAt`
- `StartedAt`
- `CompletedAt`
- `AbandonedAt`
- `ExpiredAt`
- `CreatedAt`
- `UpdatedAt`

---

### Relations

Une `CampaignParticipation` appartient à exactement une `Campaign`.

Une `CampaignParticipation` appartient à exactement un `ParticipantProfile`.

Dans le MVP, une CampaignParticipation possède au maximum une Submission.

---

### Invariants

Un `ParticipantProfile` ne peut posséder qu'une seule CampaignParticipation pour une même Campaign dans le MVP.

Le `CampaignParticipationId`, le `CampaignId`, le `ParticipantProfileId`, la source de recrutement, les snapshots et la date d'acceptation sont immuables dès la création.

Une CampaignParticipation ne peut jamais changer de Campaign.

Une CampaignParticipation ne peut jamais changer de ParticipantProfile.

Une CampaignParticipation ne peut être créée que lorsque la Campaign est `Active`.

Son statut et ses timestamps n'évoluent qu'au travers des transitions documentées.

Lorsqu'elle atteint `Completed`, `Abandoned` ou `Expired`, la CampaignParticipation devient un enregistrement historique entièrement non modifiable.

Une modification ultérieure du ParticipantProfile n'altère jamais ses snapshots.

---

### DemographicSnapshot

Le `DemographicSnapshot` conserve les informations démographiques nécessaires à l'étude telles qu'elles existaient lors de l'acceptation de la participation.

Il appartient exclusivement à la CampaignParticipation et est immuable dès sa création.

Il ne contient jamais le profil MAP.

---

### ConsentSnapshot

Le `ConsentSnapshot` conserve les conditions effectivement acceptées par le Participant lorsque la Campaign exige un consentement explicite.

Il est absent lorsque `CampaignParticipationSettings.RequireExplicitConsent = false`.

Lorsqu'il existe, il contient au minimum le contenu ou la version des conditions applicables.

La date d'acceptation canonique est portée par `CampaignParticipation.AcceptedAt`.

Il appartient exclusivement à la CampaignParticipation et est immuable dès sa création.

Une modification future des conditions de la Campaign ou du profil du Participant ne réécrit jamais ce snapshot.

---

## CampaignParticipationStatus

Le cycle de vie du MVP repose sur les statuts suivants :

```text
Accepted
Started
Completed
Abandoned
Expired
```

Le statut représente l'état métier actuel de la participation.

Il ne doit pas être déduit indirectement de la Submission.

---

### Accepted

Le Participant a accepté les conditions requises.

La CampaignParticipation existe, mais l'activité de l'étude n'a pas encore commencé.

Transitions autorisées :

```text
Accepted → Started
Accepted → Abandoned
Accepted → Expired
```

---

### Started

Le Participant a commencé l'étude.

La Submission Draft peut être créée ou complétée.

Transitions autorisées :

```text
Started → Completed
Started → Abandoned
Started → Expired
```

---

### Completed

La Submission a été soumise avec succès.

`Completed` est un état terminal.

---

### Abandoned

Le Participant a explicitement interrompu ou quitté sa participation avant la soumission.

Une simple fermeture du navigateur ou une interruption technique ne constitue jamais automatiquement un abandon.

`Abandoned` est un état terminal.

---

### Expired

La participation ne peut plus être poursuivie, notamment parce que la Campaign a été clôturée avant sa complétion.

`Expired` est un état terminal.

---

### Règles de transition du MVP

```text
Accepted ───→ Started ───→ Completed
    ├────────────┴───────→ Abandoned
    └────────────────────→ Expired
```

Toute transition non définie est interdite.

---

### États post-MVP

Les états `Invited` et `Declined` appartiennent aux workflows de recrutement fondés sur une invitation préalable.

Dans ces workflows futurs, une relation de recrutement pourra exister avant l'acceptation du Participant.

Ils ne font pas partie du cycle de vie MVP de CampaignParticipation.

---

## RecruitmentSource

`RecruitmentSource` décrit le mécanisme ayant créé la `CampaignParticipation`.

Il ne représente pas une relation métier indépendante.

Les valeurs actuellement prévues sont :

Invitation
AccessLink
Application
Announcement

D'autres mécanismes pourront être ajoutés sans modifier le modèle.

---

## CampaignInvitation

### Description

Une `CampaignInvitation` représente une invitation nominative envoyée à un Participant.

Elle constitue un mécanisme de recrutement.

Elle n'est pas la participation elle-même.

Lorsqu'une invitation est acceptée, elle crée ou active une `CampaignParticipation`.

---

### Principales propriétés

* `CampaignInvitationId`
* `CampaignId`
* `ParticipantProfileId`
* `Status`
* `ExpiresAt`
* `SentAt`

---

### Invariants

Une invitation appartient à une seule Campaign.

Une invitation ne peut être acceptée qu'une seule fois.

Une invitation expirée ne peut plus créer de nouvelle participation.

---

## CampaignAccessLink

### Description

Un `CampaignAccessLink` permet de rejoindre une Campaign grâce à un lien de participation.

Le lien ne représente pas une participation.

Chaque utilisation valide permet d'ouvrir le parcours de participation.

Dans le MVP, la CampaignParticipation n'est créée qu'après l'acceptation des conditions requises.

---

### Principales propriétés

* `CampaignAccessLinkId`
* `CampaignId`
* `Token`
* `Status`
* `ExpiresAt`
* `CreatedAt`
* `DisabledAt`

---

### CampaignAccessLinkStatus

```text
Active ⇄ Disabled
```

Un lien nouvellement créé possède le statut `Active`.

Un lien `Disabled` ne peut plus initier de nouveau parcours mais peut être réactivé tant que la Campaign reste `Active`.

`ExpiresAt` est facultatif. Lorsque cette date est dépassée, le lien est considéré comme invalide sans transition automatique de statut.

---

### Invariants

Un lien appartient à une seule Campaign.

Un lien n'est utilisable que lorsque son statut est `Active`, que sa date d'expiration éventuelle n'est pas dépassée et que sa Campaign est `Active`.

La validité d'un lien est indépendante des participations déjà créées.

L'ouverture ou la validation technique d'un lien ne crée pas automatiquement de CampaignParticipation.

L'utilisation d'un lien ne garantit pas qu'une participation sera effectivement créée ou commencée.

Le MVP n'impose aucune limite globale d'utilisations d'un lien.

---

## CampaignApplication

### Description

Une `CampaignApplication` représente une candidature soumise par un Participant afin de rejoindre une Campaign.

Elle est utilisée lorsque la participation nécessite une validation préalable.

Une candidature acceptée crée une `CampaignParticipation`.

---

### Principales propriétés

* `CampaignApplicationId`
* `CampaignId`
* `ParticipantProfileId`
* `Status`
* `SubmittedAt`
* `ReviewedAt`

---

### Invariants

Une candidature appartient à une seule Campaign.

Une candidature refusée ne crée jamais de participation.

Une candidature acceptée ne peut créer qu'une seule participation.

---

## CampaignAnnouncement

### Description

Une `CampaignAnnouncement` représente une publication permettant de rendre une Campaign visible auprès d'une population donnée.

Elle constitue un canal de recrutement.

Elle ne représente pas une participation.

---

### Responsabilités

Une `CampaignAnnouncement` est responsable :

* de la visibilité d'une Campaign ;
* de son contenu de présentation ;
* de sa période de diffusion.

---

### Principales propriétés

* `CampaignAnnouncementId`
* `CampaignId`
* `Title`
* `Description`
* `PublishedAt`
* `ExpiresAt`

---

### Invariants

Une annonce appartient à une seule Campaign.

Une annonce peut conduire à plusieurs `CampaignParticipations`.

---

## Consentement

Une `CampaignParticipation` peut nécessiter un consentement explicite.

Dans le MVP, lorsque des conditions doivent être acceptées avant la création de la CampaignParticipation, leur acceptation est vérifiée avant cette création.

Lorsque des confirmations complémentaires sont requises avant le commencement de l'étude, elles constituent un prérequis au passage vers `Started`.

Lorsque `RequireExplicitConsent = true`, les conditions effectivement acceptées sont résolues depuis la `CampaignConsentDefinition` figée de la Campaign puis conservées dans le `ConsentSnapshot` immuable de la CampaignParticipation.

Lorsque `RequireExplicitConsent = false`, aucun ConsentSnapshot n'est créé.

La date d'acceptation canonique de la CampaignParticipation reste portée par `CampaignParticipation.AcceptedAt`.

---

## Contrat métier

Le recrutement et la participation reposent sur les principes suivants :

Recruitment Mechanism
        │
        ▼
CampaignParticipation
        │
        ▼
Submission

Les mécanismes de recrutement permettent d'initier un parcours pouvant créer une participation.

Ils ne créent une CampaignParticipation que lorsque les conditions propres au workflow concerné sont satisfaites.

La `CampaignParticipation` représente toujours la relation métier entre un `ParticipantProfile` et une `Campaign`.

Les `Submissions` appartiennent à une `CampaignParticipation`.

Les `Responses` appartiennent aux `Submissions`.

---

## Périmètre MVP

Le MVP implémente :

- `CampaignParticipation` ;
- `CampaignParticipationStatus` avec `Accepted`, `Started`, `Completed`, `Abandoned` et `Expired` ;
- `CampaignAccessLink` et son cycle `Active ⇄ Disabled` ;
- une expiration facultative des liens ;
- le consentement explicite lorsque requis ;
- une seule participation par Participant et par Campaign ;
- la création de la CampaignParticipation uniquement après acceptation des conditions.

Les Invitations de Campaign, Applications et Announcements appartiennent au périmètre post-MVP.

---

## Hors MVP

Les évolutions prévues comprennent notamment :

- statuts `Invited` et `Declined` pour CampaignParticipation ;
- `CampaignInvitation` ;
- `CampaignApplication` ;
- `CampaignAnnouncement` ;
- campagnes de recrutement multi-canaux ;
- invitations groupées ;
- relances automatiques ;
- campagnes publiques ;
- sélection automatique des candidatures ;
- nombre maximal d'utilisations des liens et gestion de quotas ;
- segmentation avancée ;
- intégration avec des panels externes ;
- historique détaillé du recrutement.

Aucune de ces évolutions ne modifie le rôle central de la `CampaignParticipation`.

---

## 8. Collecte des données

### Vue d'ensemble

La collecte des données repose sur deux concepts complémentaires :

* la `Submission`, qui représente une tentative complète de réponse à une `Campaign` ;
* les `Responses`, qui représentent les réponses individuelles apportées aux Questions du `CampaignForm`.

CampaignParticipation
└── Submission
    └── Responses

Une `Submission` constitue l'unité historique de collecte.

Les `Responses` ne peuvent exister qu'à l'intérieur d'une `Submission`.

---

## Submission

### Description

Une `Submission` représente la tentative complète de réponse associée à une CampaignParticipation.

Elle regroupe toutes les Responses produites avant une soumission définitive.

Une Submission appartient toujours à une seule CampaignParticipation.

---

### Responsabilités

Une Submission est responsable :

- de son statut de complétion ;
- de ses métadonnées de collecte ;
- des Responses associées.

La progression générale du Participant appartient à la CampaignParticipation.

---

### Principales propriétés

- `SubmissionId`
- `CampaignParticipationId`
- `Status`
- `SubmittedAt`
- `AbandonedAt`
- `Duration`
- `CreatedAt`
- `UpdatedAt`

---

### Relations

Une Submission appartient à une CampaignParticipation.

Une Submission possède plusieurs Responses.

---

### Invariants

Une Submission ne peut appartenir qu'à une seule CampaignParticipation.

Son `SubmissionId`, son `CampaignParticipationId` et sa date de création sont immuables dès sa création.

Dans le MVP, une CampaignParticipation possède au maximum une Submission.

Les Responses d'une Submission ne peuvent jamais être transférées vers une autre Submission.

Tant qu'elle est `Draft`, ses Responses peuvent être créées, modifiées ou retirées.

Le passage à `Submitted` renseigne `SubmittedAt` puis rend définitivement immuables la Submission et toutes ses Responses.

Le passage à `Abandoned` renseigne `AbandonedAt` puis rend également immuables la Submission et toutes ses Responses éventuelles.

Une Submission ne revient jamais à `Draft`.

Une Submission conserve son historique lorsqu'elle est abandonnée.

---

## SubmissionStatus

Le cycle de vie du MVP repose sur les statuts suivants :

```text
Draft
Submitted
Abandoned
```

---

### Draft

La Submission existe mais n'a pas encore été soumise.

Elle peut ne contenir aucune Response.

Ses Responses peuvent être créées ou modifiées.

Transitions autorisées :

```text
Draft → Submitted
Draft → Abandoned
```

---

### Submitted

Le Participant a validé la soumission.

`SubmittedAt` est renseigné au moment de la transition.

La Submission et ses Responses deviennent définitivement immuables.

Aucune Response ne peut ensuite être ajoutée, modifiée ou supprimée.

La CampaignParticipation associée devient `Completed`.

`Submitted` est un état terminal.

---

### Abandoned

La CampaignParticipation a été abandonnée alors qu'une Submission Draft existait.

`AbandonedAt` est renseigné au moment de la transition.

La Submission et ses éventuelles Responses deviennent immuables.

Les données déjà saisies restent conservées comme progression abandonnée, mais ne sont pas considérées comme des Responses soumises et ne participent pas aux Results.

`Abandoned` est un état terminal.

---

### Absence de statut Started

`Started` n'est pas un statut de Submission dans le MVP.

Le commencement de l'étude est porté par `CampaignParticipationStatus.Started`.

La Submission reste `Draft` pendant toute sa complétion.

Cette séparation évite deux sources de vérité pour le même événement métier.

---

## Response

### Description

Une `Response` représente la réponse apportée à une `CampaignFormQuestion`.

Elle constitue la plus petite unité de donnée collectée.

---

### Responsabilités

Une `Response` est responsable :

* de la valeur répondue ;
* de la Question concernée ;
* des métadonnées nécessaires à son interprétation.

---

### Principales propriétés

* `ResponseId`
* `SubmissionId`
* `CampaignFormQuestionId`
* `Value`
* `AnsweredAt`

Le format exact de `Value` dépend du type de la Question.

---

### Relations

Une `Response` appartient à une `Submission`.

Une `Response` référence une `CampaignFormQuestion`.

---

### Invariants

Une `Response` appartient toujours à une seule `Submission`.

Son `ResponseId`, son `SubmissionId` et son `CampaignFormQuestionId` sont immuables dès sa création.

Une `Response` référence toujours une `CampaignFormQuestion`.

Une `Submission` ne peut contenir qu'une seule `Response` par `CampaignFormQuestion`.

Une `Response` ne référence jamais une `Question` provenant directement d'un `FormTemplate`.

Sa valeur et ses métadonnées peuvent évoluer uniquement tant que sa Submission est `Draft`.

Lorsque la Submission devient `Submitted` ou `Abandoned`, la Response devient définitivement immuable.

---

## Valeurs de réponse

Le Domain Model ne contraint pas la représentation technique des valeurs.

Dans le MVP, une `Response` contient exclusivement une représentation compatible avec le catalogue de QuestionTypes :

* une valeur numérique d'échelle ou de Number ;
* un booléen ;
* une option ;
* plusieurs options ;
* une chaîne de caractères courte ou longue.

La validation de ces valeurs relève des règles historiques définies par la `CampaignFormQuestion`.

Les dates, fichiers, médias et représentations supplémentaires appartiennent au post-MVP.

---

## Familles analytiques des Responses

Pour les Analytics, chaque type de CampaignFormQuestion appartient de manière déterministe à une famille analytique définie par sa structure et sa configuration historique.

```text
Scalar
Ordinal
Binary
Categorical
MultiCategorical
Textual
NonScorable
```

### Scalar

Valeur numérique bornée pouvant être harmonisée sur une échelle commune.

### Ordinal

Valeur appartenant à une suite d'options explicitement ordonnées.

La position dans cet ordre peut être harmonisée lorsque le nombre d'options est supérieur à un.

### Binary

Valeur à deux états pouvant être représentée structurellement par `0` et `100` lorsqu'un binding `Scored` l'autorise.

### Categorical

Choix unique sans ordre naturel.

Il produit des effectifs et proportions mais aucun score scalaire automatique.

### MultiCategorical

Ensemble de choix sans ordre naturel.

Il produit des taux de sélection et des distributions, mais aucun score scalaire automatique.

### Textual

Texte libre utilisé comme preuve qualitative et comme relation `Contextual`.

### NonScorable

Valeur qui ne dispose d'aucune transformation quantitative déterministe définie dans le MVP.

---

### Déduction déterministe

La famille analytique est déduite uniquement à partir :

* du type historique de la CampaignFormQuestion ;
* de ses bornes ;
* de ses options ;
* de leur ordre éventuel ;
* de sa configuration de validation.

SignalLab n'analyse jamais le libellé de la Question pour déterminer sa famille, sa Measure, sa direction ou son poids.

Une relation `Scored` n'est valide que pour une famille pouvant produire une valeur scalaire déterministe.

Une relation `Contextual` reste autorisée quelle que soit la famille compatible avec le besoin métier.

---

## Historique

Une `Submission` représente toujours une photographie fidèle de ce qui a été soumis à un instant donné.

Les modifications ultérieures du `CampaignForm`, des `Measures` ou des `FormTemplates` ne modifient jamais les `Responses` déjà collectées.

L'interprétation historique reste toujours reproductible.

---

## Contrat métier

Le modèle de collecte repose sur la hiérarchie suivante :

Campaign
└── CampaignParticipation
    └── Submission
        └── Response

Une `Submission` représente une tentative.

Une `Response` représente une réponse individuelle.

Une `Submission` devient immuable lorsqu'elle est `Submitted`.

Les `Responses` deviennent immuables avec leur `Submission`.

---

## Périmètre MVP

Le MVP implémente :

- `Submission` ;
- `SubmissionStatus` avec `Draft`, `Submitted` et `Abandoned` ;
- `Response` ;
- les représentations de valeur correspondant au catalogue de QuestionTypes du MVP ;
- une seule Submission par CampaignParticipation ;
- une seule Response par CampaignFormQuestion ;
- le gel des données après soumission.

---

## Hors MVP

Les évolutions prévues comprennent notamment :

* plusieurs `Submissions` par `CampaignParticipation` ;
* sauvegarde intermédiaire avancée ;
* reprise sur plusieurs appareils ;
* historique détaillé des modifications avant soumission ;
* validation asynchrone ;
* réponses collaboratives ;
* pièces jointes enrichies ;
* import de réponses externes.

Ces évolutions ne remettent pas en cause le principe selon lequel les `Responses` appartiennent toujours à une `Submission`.

---

## 9. Analyse des données

### Vue d'ensemble

L'Analysis Workspace constitue le principal moteur d'exploitation des données du MVP.

Il doit permettre d'extraire le maximum de connaissance des Responses avant même l'introduction du Research Workspace.

SignalLab distingue :

* les `Results`, qui permettent de consulter fidèlement les données individuelles d'une Campaign ;
* les `Analytics`, qui calculent des projections à partir des données métier ;
* une `Analysis`, qui représente un travail temporaire d'exploration ;
* une `SavedAnalysis`, qui persiste uniquement la configuration de ce travail.

```text
Responses historiques
        ↓
CampaignFormQuestionMeasureBindings historiques
        ↓
Moteur analytique déterministe
        ↓
Scores, distributions, comparaisons et preuves
        ↓
Analysis temporaire ou SavedAnalysis
```

Les Responses, CampaignFormQuestions et CampaignFormQuestionMeasureBindings historiques restent la source de vérité.

Aucun résultat calculé n'est persisté comme donnée métier.

---

## Results

### Responsabilité

`Results` est limité à une seule Campaign.

Il permet notamment :

* de consulter les CampaignParticipations ;
* de consulter les Submissions ;
* de lire les Responses individuelles ;
* de reconstruire le CampaignForm historique ;
* de comprendre exactement ce qu'un Participant a soumis.

`Results` ne réalise pas d'agrégation multi-Campaigns, de comparaison de groupes ni de segmentation analytique sauvegardée.

---

## Analytics

### Description

Les Analytics regroupent les traitements déterministes permettant de produire des informations à partir des données collectées.

Ils exploitent notamment :

* les Campaigns ;
* les Builds ;
* les CampaignParticipations ;
* les DemographicSnapshots ;
* les Submissions ;
* les Responses ;
* les CampaignFormQuestions ;
* les CampaignFormQuestionMeasureBindings historiques ;
* les Measures ;
* les MAPProfiles autorisés lorsque cette segmentation est utilisée.

Les Analytics représentent une capacité du système et non une entité persistante.

---

### Principes

Les Analytics :

* ne modifient jamais les données du domaine ;
* n'interprètent jamais le texte d'une Question ;
* n'utilisent aucune IA dans le MVP ;
* appliquent uniquement des règles déterministes et versionnées ;
* produisent des résultats entièrement traçables jusqu'aux sources ;
* affichent toujours la population, la couverture et les données manquantes pertinentes ;
* ne transforment jamais une relation observée en conclusion causale.

---

## Données incluses dans les Analyses

Les Analyses finalisées utilisent exclusivement les Submissions dont le statut est :

```text
Submitted
```

Sont toujours exclues des agrégations :

* les Submissions `Draft` ;
* les Submissions `Abandoned` ;
* les Responses appartenant à ces Submissions ;
* les CampaignParticipations sans Submission soumise.

Les Campaigns `Active`, `Completed` et `Archived` peuvent contribuer.

Une Campaign `Draft` ne contribue jamais puisqu'elle ne peut contenir aucune Submission soumise.

L'unité analytique canonique du MVP est la `Submission`.

Comme une CampaignParticipation possède au maximum une Submission dans le MVP, une Submission correspond également à un Participant dans une Campaign, mais le calcul conserve la Submission comme unité afin de rester compatible avec les évolutions futures.

---

## AnalysisScope

### Description

L'`AnalysisScope` définit le périmètre des données analysées.

### Principales propriétés

* `ProjectId`
* `CampaignIds`

### Invariants

Une Analysis appartient à exactement un Project.

Son Scope contient une ou plusieurs Campaigns appartenant à ce Project.

Aucune Campaign d'un autre Project ou d'une autre Organization ne peut y être ajoutée.

Les Builds sont résolus à partir des Campaigns sélectionnées et ne sont jamais copiés dans le Scope comme source de vérité indépendante.

Une modification du Scope déclenche un recalcul complet.

Une Analysis ouverte depuis une Campaign peut initialiser son Scope avec cette Campaign.

Une Analysis ouverte depuis un Project démarre avec un Scope à configurer.

L'analyse multi-Projects et inter-Organizations est hors MVP.

---

## Analysis et SavedAnalysis

### Analysis

Une `Analysis` représente une exploration temporaire des données.

Elle peut être modifiée, filtrée et comparée sans être persistée.

### SavedAnalysis

Une `SavedAnalysis` persiste la configuration nécessaire pour reconstruire une Analysis.

Elle conserve notamment :

* `SavedAnalysisId` ;
* `ProjectId` ;
* `Name` ;
* `Description` ;
* `AnalysisScope` ;
* les Questions et Measures sélectionnées ;
* les `AnalysisGroups` ;
* les filtres ;
* les seuils de couverture éventuels ;
* les métriques affichées ;
* les visualisations sélectionnées parmi celles autorisées ;
* la configuration de présentation ;
* la version du moteur analytique utilisée pour interpréter la configuration ;
* `CreatedByOrganizationMemberId` ;
* `CreatedAt` ;
* `UpdatedAt`.

Une SavedAnalysis ne conserve jamais :

* les Responses résolues ;
* les Submissions résolues ;
* les SubmissionMeasureScores ;
* les scores de population ;
* les distributions ;
* les corrélations ;
* les tailles d'effet ;
* les intervalles d'incertitude ;
* les scores MAP ;
* les images des visualisations.

Tous les résultats sont recalculés lors de l'ouverture ou de l'exécution.

Une SavedAnalysis ne peut référencer que des ressources accessibles depuis son Project.

---

## Moteur de score déterministe

### Étape 1 — Résolution de la valeur

Le système lit :

* la CampaignFormQuestion historique ;
* son type ;
* sa configuration ;
* ses bornes ou options ;
* la Response ;
* le CampaignFormQuestionMeasureBinding historique.

### Étape 2 — Détermination de la famille analytique

La famille est déduite uniquement de la structure de la Question.

Aucune analyse du libellé n'est réalisée.

### Étape 3 — Normalisation structurelle

Pour une valeur scalaire bornée :

```text
NormalizedScore
= 100 × (Value - Minimum) / (Maximum - Minimum)
```

Pour une valeur ordinale, la position de l'option dans l'ordre déclaré est transformée sur `0–100`.

Pour une valeur binaire scoreable :

```text
False = 0
True = 100
```

Aucune Question catégorielle non ordonnée, multi-catégorielle, textuelle ou non scoreable n'est convertie automatiquement en score.

### Étape 4 — Direction de contribution

```text
Aligned
ContributionScore = NormalizedScore

Inverted
ContributionScore = 100 - NormalizedScore
```

La direction provient exclusivement du binding configuré par le chercheur.

### Étape 5 — Pondération par Submission

Pour une Measure donnée :

```text
SubmissionMeasureScore
= Σ (ContributionScore × Weight)
  / Σ Weight répondu
```

Le calcul utilise uniquement les bindings `Scored` disposant d'une Response valide dans cette Submission.

Chaque Submission produit au maximum un score par Measure.

Une Campaign utilisant plusieurs Questions pour une Measure ne pèse donc pas davantage qu'une Campaign n'en utilisant qu'une seule.

### Étape 6 — Agrégation de population

```text
PopulationMeasureScore
= moyenne des SubmissionMeasureScores valides
```

Les agrégations utilisent toujours les scores par Submission et jamais toutes les Responses fusionnées directement.

### Étape 7 — Projection orientée résultat

Lorsque la Measure possède une OutcomeDirection non neutre, le système peut calculer un OutcomeScore sans modifier le MeasureScore.

---

## Données manquantes et couverture

Aucune valeur manquante n'est assimilée à :

* zéro ;
* faux ;
* une valeur neutre ;
* une moyenne ;
* une option par défaut.

Pour chaque Submission et chaque Measure, SignalLab calcule :

```text
ExpectedWeight
AnsweredWeight
WeightCoverage = AnsweredWeight / ExpectedWeight
```

`ExpectedWeight` correspond à la somme des poids des bindings `Scored` applicables à cette Submission.

`AnsweredWeight` correspond à la somme des poids disposant d'une Response valide.

Un SubmissionMeasureScore peut être produit lorsque :

* au moins une contribution `Scored` valide existe ;
* toutes les Questions `Required` contribuant à la Measure possèdent une Response valide.

Une Question optionnelle manquante ne bloque pas nécessairement le score mais réduit sa couverture.

Chaque résultat expose au minimum :

* `SubmissionCount` ;
* `ScoredSubmissionCount` ;
* `ValidResponseCount` ;
* `MissingResponseCount` ;
* la couverture moyenne ;
* le poids répondu ;
* le poids attendu.

Une SavedAnalysis peut définir un seuil minimal de couverture pour certains résultats.

Ce seuil reste un filtre analytique explicite et ne modifie jamais les données sources.

---

## Analyse par Question

L'analyse par Question conserve toujours la valeur historique brute.

### Scalar et Ordinal

Le MVP peut produire :

* effectif valide ;
* moyenne ;
* médiane ;
* minimum ;
* maximum ;
* quartiles ;
* écart-type ;
* percentiles ;
* distribution ;
* données manquantes.

Une échelle ordinale peut afficher une moyenne descriptive, mais la distribution et la médiane restent toujours accessibles.

### Binary et Categorical

Le MVP peut produire :

* effectif par option ;
* proportion par option ;
* effectif valide ;
* données manquantes.

Le dénominateur d'une proportion est toujours explicite.

### MultiCategorical

Le MVP peut produire :

* effectif de Submissions ayant sélectionné chaque option ;
* taux de sélection par option ;
* nombre moyen d'options sélectionnées ;
* données manquantes.

Le total des proportions peut dépasser `100 %` et cette propriété doit être indiquée.

### Textual

Le MVP peut produire :

* nombre de Responses textuelles ;
* liste des verbatims ;
* recherche textuelle simple ;
* filtres structurels, démographiques, de Measure et MAP ;
* accès à la Response, à la Submission et à la CampaignParticipation source.

Aucun résumé, thème, sentiment ou cluster automatique n'est produit.

### NonScorable

La valeur peut rester consultable dans Results et dans les preuves sources, mais ne produit aucune agrégation quantitative spécifique dans le MVP.

---

## Analyse par Measure

Une analyse de Measure peut produire :

* le MeasureScore moyen sur `0–100` ;
* la médiane ;
* l'OutcomeScore lorsqu'il est défini ;
* les quartiles ;
* l'écart-type ;
* les percentiles ;
* la distribution des SubmissionMeasureScores ;
* la population ;
* la couverture ;
* les données manquantes ;
* les Questions contributrices ;
* les poids ;
* les directions ;
* les Questions `Contextual` ;
* les Campaigns et Builds sources ;
* les diagnostics de cohérence ;
* les comparaisons entre groupes.

Une Question textuelle ou catégorielle peut rester liée à la Measure en mode `Contextual` sans participer à son score.

Cette relation permet de consulter les preuves qualitatives ou contextuelles associées aux valeurs quantitatives.

---

## AnalysisGroup et filtres

### AnalysisGroup

Un `AnalysisGroup` représente un sous-ensemble nommé de la population du même AnalysisScope.

```text
AnalysisGroup
├── Name
└── Filters[]
```

Une Analysis peut définir plusieurs groupes pour les comparer.

### Filtres structurels

Le MVP peut filtrer par :

* Campaign ;
* Build, résolu depuis les Campaigns ;
* période de soumission ;
* présence ou absence d'une Response ;
* valeur ou intervalle d'une Response compatible ;
* intervalle de SubmissionMeasureScore ;
* seuil de couverture.

### Filtres démographiques

Les filtres démographiques utilisent exclusivement le `DemographicSnapshot` immuable de la CampaignParticipation.

Ils peuvent notamment porter sur :

* le pays ;
* la langue ;
* la tranche d'âge calculée depuis le snapshot ;
* le genre lorsque cette donnée existe ;
* les autres propriétés démographiques explicitement collectées dans le MVP.

Une modification ultérieure du ParticipantProfile ne change jamais ces filtres historiques.

### Participant individuel

L'identité d'un Participant n'est pas un filtre analytique principal.

L'exploration individuelle appartient à Results.

Une Analysis permet toutefois de retrouver ses données sources selon les Permissions applicables.

---

## Statistiques automatiques

L'utilisateur ne configure aucune formule statistique.

SignalLab sélectionne une méthode déterministe, documentée et versionnée selon la projection demandée.

### Statistiques descriptives

Le MVP prend en charge :

* moyenne ;
* médiane ;
* quartiles ;
* minimum ;
* maximum ;
* écart-type ;
* percentiles ;
* effectifs ;
* proportions ;
* couverture.

### Relations entre Measures

La relation monotone entre deux Measures est calculée par défaut avec :

```text
Spearman ρ
```

Chaque résultat expose :

* le coefficient ;
* le nombre de paires valides ;
* la couverture commune ;
* un intervalle d'incertitude à `95 %` ;
* un avertissement explicite indiquant qu'une corrélation ne démontre aucune causalité.

### Comparaisons entre groupes

Selon la famille des données, SignalLab peut calculer automatiquement :

* différence de moyenne ;
* différence de médiane ;
* différence en points `0–100` ;
* différence de proportions ;
* différence moyenne standardisée ;
* `Hedges g` pour les scores quantitatifs.

Les tailles d'effet sont descriptives.

Elles ne démontrent ni causalité ni significativité scientifique à elles seules.

### Intervalles d'incertitude

Les moyennes, médianes, corrélations et tailles d'effet peuvent être accompagnées d'un intervalle à `95 %`.

La méthode exacte de calcul appartient à la version du moteur analytique et doit être documentée afin qu'une configuration identique reste interprétable.

L'utilisateur ne choisit pas la méthode dans le MVP.

---

## Diagnostic de cohérence d'une Measure composite

Lorsqu'une Measure est calculée à partir de plusieurs Questions `Scored`, SignalLab produit automatiquement un diagnostic de cohérence des contributions.

Il peut comprendre :

* le nombre de Questions contributrices ;
* le nombre de Submissions complètes ;
* la matrice de corrélations entre Questions ;
* la corrélation moyenne entre Questions ;
* un indicateur standardisé de cohérence interne lorsque le calcul est défini ;
* les Questions dont le comportement diverge fortement des autres ;
* la couverture par Question.

Ce diagnostic ne constitue jamais une validation psychométrique ou scientifique de la Measure.

SignalLab ne retire, ne repondère et ne réoriente automatiquement aucune Question.

La décision d'ajuster le formulaire appartient au chercheur et produit une nouvelle version historique lorsque nécessaire.

---

## Visualisations analytiques du MVP

SignalLab ne constitue pas un dashboard builder.

Il propose un ensemble fini de visualisations déterminées par les familles de données et les sujets sélectionnés.

### 1. Vue détaillée d'une Measure

Elle peut afficher :

* MeasureScore ;
* OutcomeScore ;
* moyenne ;
* médiane ;
* écart-type ;
* intervalle d'incertitude ;
* histogramme ;
* box plot ;
* courbe cumulative ou percentiles ;
* population ;
* couverture.

### 2. Comparaison entre Campaigns ou Builds

Elle peut utiliser :

* barres groupées ;
* dumbbell chart ;
* slope chart ;
* courbe d'évolution ;
* écarts bruts ;
* écarts standardisés.

### 3. Profil multi-Measures

La représentation par défaut utilise des barres horizontales.

Un radar peut être proposé comme représentation secondaire lorsque le nombre de Measures reste lisible.

La vue indique explicitement si elle affiche les MeasureScores ou les OutcomeScores.

### 4. Heatmap

Les matrices MVP peuvent notamment représenter :

* Measures × Campaigns ;
* Measures × Builds ;
* Measures × AnalysisGroups ;
* Measures × segments MAP ;
* couverture × Campaigns.

### 5. Contributions et poids

Cette vue expose :

* les Questions contributrices ;
* leur score harmonisé ;
* leur poids ;
* leur contribution pondérée ;
* leur direction ;
* leur couverture ;
* leur diagnostic de cohérence.

### 6. Preuves qualitatives associées

Une Measure peut afficher :

* ses Questions `Contextual` ;
* les verbatims associés ;
* une recherche textuelle ;
* les verbatims des Submissions ayant des scores faibles ou élevés ;
* l'accès aux données sources.

Cette capacité ne crée aucune Note ou commentaire collaboratif.

### 7. Couverture et données manquantes

Cette vue peut afficher :

* couverture par Measure ;
* couverture par Question ;
* couverture par Campaign ;
* poids répondu et attendu ;
* heatmap des lacunes.

### 8. Scatter entre deux Measures

Chaque point représente une Submission disposant des deux scores.

Les points peuvent être distingués par Campaign, Build ou AnalysisGroup.

Le scatter reste une exploration de relation et non une preuve causale.

### 9. Matrice de corrélations

La matrice présente les coefficients de Spearman entre plusieurs Measures.

Elle indique la population commune, la couverture et les avertissements de faible échantillon.

Un drill-down peut ouvrir le scatter correspondant.

### 10. Différences standardisées

Un forest plot peut afficher :

* Hedges g ;
* l'intervalle à `95 %` ;
* la direction favorable ou défavorable ;
* la population de chaque groupe.

### 11. Graphe Question–Measure

Un graphe biparti peut afficher :

* les Questions ;
* les Measures ;
* l'épaisseur des relations selon les poids ;
* la direction ;
* les relations `Scored` et `Contextual` ;
* les Measures sans couverture ;
* les Questions sans Measure ;
* les Measures dépendant d'une seule Question.

Cette vue constitue un outil de conception et d'audit du questionnaire.

### 12. Segmentation MAP

Lorsque M10 est disponible, le MVP peut afficher :

* comparaison des scores par segment MAP ;
* heatmap Measures × segments MAP ;
* distributions côte à côte ;
* scatter dimension MAP × Measure ;
* forest plot entre segments ;
* population MAP utilisable et indisponible.

Les règles de confidentialité et de profil vivant restent applicables.

---

## Choix déterministe des visualisations

SignalLab propose uniquement les représentations compatibles avec les données disponibles.

Exemples :

```text
Measure quantitative
→ score, histogramme, box plot, évolution, comparaison

Question catégorielle
→ fréquences, proportions, comparaison

Question textuelle
→ liste, recherche, filtrage par scores

Deux Measures
→ scatter, corrélation

Plusieurs Measures et Campaigns
→ barres, heatmap, profil
```

L'utilisateur peut sélectionner une représentation parmi les options autorisées.

Il ne configure aucune formule, aucun mapping ni aucune méthode statistique dans le MVP.

---

## Traçabilité

Toute valeur analytique reste reliée à ses sources.

Chaque résultat expose au minimum :

* `CalculatedAt` ;
* le Project ;
* les Campaigns ;
* les Builds ;
* le Scope ;
* les AnalysisGroups ;
* les filtres ;
* la population ;
* les données manquantes ;
* la couverture ;
* les CampaignFormQuestions sources ;
* les CampaignFormQuestionMeasureBindings historiques ;
* les poids ;
* les directions ;
* la méthode de normalisation ;
* la méthode statistique ;
* la version du moteur analytique.

Depuis un agrégat, un membre autorisé peut retrouver :

```text
AnalyticalResult
→ SubmissionMeasureScores
→ Responses
→ CampaignFormQuestions
→ CampaignFormQuestionMeasureBindings
→ Submissions
→ CampaignParticipations
```

La traçabilité vers une identité individuelle respecte toujours les Permissions applicables.

MAP ne permet jamais de révéler individuellement pourquoi un profil est indisponible.

---

## Recalcul et fraîcheur

Une SavedAnalysis est recalculée notamment :

* lors de son ouverture ;
* lors d'une modification du Scope ;
* lors d'une modification des filtres ;
* lors d'une modification des groupes ;
* lorsqu'une nouvelle donnée source pertinente est disponible ;
* lorsqu'un MAPProfile courant ou sa préférence de partage évolue pour une segmentation MAP.

Une Analysis incluant une Campaign `Active` peut donc évoluer à mesure que de nouvelles Submissions sont soumises.

Chaque exécution renseigne `CalculatedAt` et identifie la version du moteur analytique.

Un futur besoin de reproduction exacte d'un résultat à une date donnée nécessiterait un `AnalysisSnapshot`, explicitement hors MVP.

---

## MAP dans les Analyses

Les Analyses résolvent toujours les profils MAP au moment du calcul.

```text
AnalysisScope
↓
CampaignParticipations et Submissions autorisées
↓
Users correspondants
↓
MAPProfiles actuels, partagés et compatibles
↓
Segmentation MAP
```

Une SavedAnalysis peut conserver une configuration de segmentation MAP, mais jamais les scores MAP résolus.

Le MVP peut filtrer ou grouper sur une dimension MAP avec des opérateurs de seuil ou d'intervalle.

Il utilise les scores continus de la MAPModelVersion concernée.

Il ne crée aucun type de joueur exclusif, aucune catégorie faible/moyenne/élevée automatique et aucune norme populationnelle implicite.

---

## Research Workspace post-MVP

Le `ResearchBoard`, les `Notes`, les `Insights`, les `Hypotheses` et les `Decisions` appartiennent à la cible post-MVP.

Ils apporteront ultérieurement un contexte d'organisation, de raisonnement et de collaboration autour des preuves produites par les Analyses.

```text
Analyses
↓
Preuves
↓
Hypothèses
↓
Interprétations
↓
Conclusions
↓
Décisions
```

Le moteur analytique du MVP doit être suffisamment riche pour que le futur Research Workspace puisse organiser ses résultats sans les recalculer selon un autre modèle métier.

### ResearchBoard

Un ResearchBoard représentera un espace de travail appartenant à un Project et capable de référencer des SavedAnalysis, des Notes et d'autres preuves sans devenir propriétaire de leurs données.

### Note

Une Note représentera une observation, une hypothèse, une interprétation ou une décision rédigée dans un ResearchBoard.

Ces concepts ne sont pas implémentés dans le MVP.

---

## Contrat métier

```text
CampaignFormQuestions historiques
        └── CampaignFormQuestionMeasureBindings historiques
                ↓
Submitted Submissions et Responses
                ↓
Analytics déterministes et versionnés
                ↓
Analysis temporaire
                └── SavedAnalysis optionnelle
```

Les Responses constituent la source de vérité.

Les CampaignFormQuestionMeasureBindings historiques définissent comment les Responses contribuent aux Measures.

Les Analytics produisent des preuves recalculables.

Les SavedAnalysis conservent uniquement leur configuration.

Le Research Workspace reste post-MVP.

---

## Périmètre MVP

Le MVP implémente :

* l'analyse par Question ;
* l'analyse par Measure ;
* les scores harmonisés `0–100` ;
* les poids et directions historiques ;
* le calcul par Submission avant toute agrégation ;
* la couverture et les données manquantes ;
* `AnalysisScope` limité à un Project ;
* `AnalysisGroup` ;
* les filtres structurels et démographiques ;
* les statistiques descriptives ;
* Spearman entre Measures ;
* les différences standardisées et Hedges g ;
* les intervalles d'incertitude déterministes ;
* le diagnostic de cohérence des Measures composites ;
* les douze capacités visuelles décrites dans ce chapitre ;
* la traçabilité complète ;
* `SavedAnalysis` sans persistance des résultats ;
* la segmentation MAP dynamique lorsque M10 est disponible.

---

## Hors MVP

Le MVP exclut notamment :

* toute IA ;
* le NLP ;
* l'analyse automatique de sentiment ;
* le clustering automatique de verbatims ;
* les suggestions automatiques de Measures ;
* la détection automatique de polarité ou de relations ;
* les formules personnalisées ;
* les mappings personnalisés ;
* les calibrations IRT ;
* les tables de conversion configurables ;
* les normes statistiques saisies manuellement ;
* la pondération de populations ;
* la régression ;
* la prédiction ;
* les analyses multi-Projects ;
* les analyses inter-Organizations ;
* les ResearchBoards post-MVP ;
* les Notes et commentaires collaboratifs ;
* les exports ;
* les rapports PDF ;
* les dashboards libres ;
* les snapshots figés d'Analysis ;
* les visualisations arbitraires ou personnalisées.

Ces exclusions ne remettent pas en cause la puissance descriptive, longitudinale et segmentée du moteur analytique du MVP.

---

# 10. MAP

## Vue d'ensemble

MAP est le modèle motivationnel utilisé par SignalLab pour décrire les raisons générales qui conduisent un User à jouer à des jeux vidéo.

MAP constitue une grille de lecture personnelle et transversale.

Il ne représente ni une préférence pour un jeu précis, ni une évaluation d'une Campaign, ni une donnée appartenant à une Organization.

```text
User
├── MAPAssessments
└── MAPProfile actuel
```

Les Organizations utilisent les profils MAP uniquement dans le cadre d'Analyses autorisées portant sur leurs propres données de recherche.

Elles ne deviennent jamais propriétaires du profil, de l'assessment ou de ses réponses.

---

## Ownership

Chaque `MAPAssessment` et chaque `MAPProfile` appartient directement à un seul `User`.

Le `ParticipantProfile` permet de représenter ce User dans les parcours de Participation, mais il ne possède aucune donnée MAP.

Les données MAP sont donc :

- globales à SignalLab ;
- personnelles au User ;
- indépendantes des Organizations ;
- indépendantes des Projects ;
- indépendantes des Campaigns ;
- disponibles même avant toute CampaignParticipation.

Une Organization ne peut jamais créer, modifier, transférer ou supprimer une donnée MAP au nom d'un User.

---

## MAPModelVersion

### Description

Une `MAPModelVersion` représente une version publiée de la définition du modèle MAP maintenu par SignalLab.

Elle décrit notamment :

- les dimensions motivationnelles ;
- les items du questionnaire ;
- leur ordre ;
- les options de réponse ;
- l'affectation des items aux dimensions ;
- les règles de validation ;
- les règles de calcul.

### Principales propriétés

- `MAPModelVersionId`
- `Version`
- `Name`
- `Description`
- `Dimensions`
- `Items`
- `ScoringDefinition`
- `IsActive`
- `PublishedAt`

### Invariants

Une MAPModelVersion publiée est entièrement immuable.

Une seule MAPModelVersion est active pour la création de nouveaux MAPAssessments à un instant donné.

L'activation d'une nouvelle version ne modifie jamais :

- les assessments existants ;
- les réponses déjà soumises ;
- les profils MAP déjà calculés ;
- les CampaignParticipations ;
- les Analyses sauvegardées.

Chaque MAPAssessment conserve définitivement la version active sélectionnée lors de sa création.

Les anciennes versions restent disponibles afin de préserver l'interprétation et le recalcul déterministe des profils qui les utilisent.

Aucune migration automatique d'un profil vers une nouvelle MAPModelVersion n'est réalisée.

---

## MAPAssessment

### Description

Un `MAPAssessment` représente le questionnaire personnel complété par un User afin de calculer ou de mettre à jour son profil MAP.

Il est distinct d'un FormTemplate, d'un CampaignForm et de toute CampaignParticipation.

Une Campaign ne peut jamais intégrer directement un MAPAssessment dans son CampaignForm.

### Principales propriétés

- `MAPAssessmentId`
- `UserId`
- `MAPModelVersionId`
- `Status`
- `Responses`
- `StartedAt`
- `SubmittedAt`
- `AbandonedAt`

### MAPAssessmentStatus

```text
Draft ──→ Submitted
  └─────→ Abandoned
```

#### Draft

Un assessment `Draft` peut être complété progressivement.

Ses réponses peuvent être ajoutées, modifiées ou retirées selon les règles de la MAPModelVersion.

Sa référence vers la MAPModelVersion est immuable dès sa création.

Une activation ultérieure d'une nouvelle MAPModelVersion ne change donc jamais le questionnaire déjà commencé.

#### Submitted

La soumission exige que toutes les réponses requises soient présentes et valides.

Lors de la soumission :

- l'assessment et ses réponses deviennent immuables ;
- le profil est calculé selon sa MAPModelVersion ;
- le MAPProfile actuel du User est créé ou mis à jour ;
- la provenance du calcul est conservée ;
- `SubmittedAt` est renseigné.

La validation, la soumission, le calcul du profil et son remplacement constituent une seule opération métier atomique.

Si le calcul ne peut pas aboutir, l'assessment ne devient pas `Submitted` et le profil courant reste inchangé.

Un assessment `Submitted` ne peut jamais revenir à `Draft`.

#### Abandoned

Un assessment `Abandoned` est terminal et devient non modifiable.

Il ne produit aucun MAPProfile et n'est jamais utilisé dans les Analyses.

### Invariants

Un MAPAssessment appartient à un seul User et ne peut jamais être transféré.

Il est toujours calculé avec la MAPModelVersion qu'il référence.

Une Organization ou une Campaign ne peut jamais :

- créer un assessment pour un User ;
- le rendre obligatoire ;
- répondre à sa place ;
- accéder à ses réponses brutes.

---

## Historique des assessments

Un User peut soumettre plusieurs MAPAssessments successifs.

Tous les assessments soumis sont conservés comme données historiques immuables.

```text
User
├── MAPAssessment 1 — Submitted
├── MAPAssessment 2 — Submitted
└── MAPAssessment 3 — Submitted
        ↓
   MAPProfile actuel
```

Dans le MVP :

- seul le dernier assessment soumis détermine le profil actuel ;
- les anciens assessments ne sont pas utilisés directement par les Analyses ;
- aucune comparaison historique des profils n'est proposée ;
- les anciens assessments conservent la provenance et permettent un recalcul déterministe si nécessaire.

---

## MAPProfile

### Description

Le `MAPProfile` représente le profil motivationnel actuel d'un User.

Il est calculé à partir d'un MAPAssessment soumis et de la MAPModelVersion correspondante.

Le profil ne constitue pas un type de joueur exclusif.

Il représente un ensemble de scores indépendants formant un profil multidimensionnel.

### Principales propriétés

- `MAPProfileId`
- `UserId`
- `SourceAssessmentId`
- `MAPModelVersionId`
- `DimensionScores`
- `CalculatedAt`
- `SharingStatus`
- `SharingUpdatedAt`

### Invariants

Un User possède au maximum un MAPProfile actuel.

Le User propriétaire ne change jamais.

Un nouvel assessment soumis remplace atomiquement :

- l'assessment source ;
- la MAPModelVersion ;
- les scores des dimensions ;
- la date de calcul.

La préférence de partage existante est conservée lors du recalcul du profil.

Le remplacement du profil courant ne modifie jamais :

- les CampaignParticipations ;
- les DemographicSnapshots ;
- les ConsentSnapshots ;
- les Submissions ;
- les Responses ;
- les SavedAnalysis.

---

## Partage du profil MAP

### MAPSharingStatus

Le MVP utilise un partage global unique.

```text
Private ⇄ SharedForResearch
```

#### Private

`Private` est la valeur par défaut lors de la création du premier profil.

Le profil ne peut être utilisé dans aucune Analysis d'Organization.

#### SharedForResearch

Le profil peut être utilisé comme grille de segmentation dans les Analyses autorisées lorsque le User a produit des données dans le périmètre analysé.

Le partage ne rend jamais le profil :

- public ;
- recherchable ;
- visible dans un annuaire ;
- propriété d'une Organization ;
- accessible hors d'un AnalysisScope autorisé.

### Modification du partage

Seul le User propriétaire peut modifier le partage de son profil.

Une Organization, un Administrator ou un Owner ne peut jamais outrepasser cette préférence.

La modification prend effet immédiatement côté Backend.

Lorsqu'un profil déjà partagé est recalculé à partir d'un nouvel assessment, le nouveau profil reste partagé.

Le User doit être informé de ce comportement avant la soumission du nouvel assessment.

Le partage par Organization, Project ou Campaign appartient au périmètre post-MVP.

---

## Utilisation dans les Analyses

Les Analyses résolvent toujours les profils MAP au moment du calcul.

```text
AnalysisScope
↓
CampaignParticipations et Responses autorisées
↓
Users correspondants
↓
MAPProfiles actuels, partagés et compatibles
↓
Segmentation MAP
```

Une Organization peut utiliser un profil MAP uniquement lorsque toutes les conditions suivantes sont satisfaites :

- le User possède des Responses dans une Campaign de cette Organization ;
- ces Responses appartiennent à l'AnalysisScope courant ;
- le profil actuel est `SharedForResearch` ;
- sa MAPModelVersion est compatible avec le calcul demandé ;
- le membre consultant possède les Permissions analytiques nécessaires.

Aucune valeur MAP n'est copiée dans :

- CampaignParticipation ;
- Submission ;
- Response ;
- SavedAnalysis ;
- AnalysisScope.

Une SavedAnalysis peut conserver une configuration de segmentation MAP, mais jamais les scores résolus des Participants.

Une même SavedAnalysis peut donc produire un résultat différent lorsqu'elle est recalculée si un User :

- soumet un nouvel assessment ;
- active son partage ;
- désactive son partage.

Cette évolution ne réécrit aucune donnée historique de Campaign.

Elle modifie uniquement la grille de lecture actuelle appliquée à ces données.

---

## Désactivation du partage

La désactivation du partage entraîne immédiatement :

- l'exclusion du profil de tous les nouveaux calculs MAP ;
- l'invalidation des projections ou caches analytiques qui l'utilisent ;
- son retrait lors de la réouverture ou du recalcul d'une SavedAnalysis.

Elle ne supprime jamais :

- le MAPProfile ;
- les MAPAssessments ;
- les CampaignParticipations ;
- les Submissions ;
- les Responses.

Les Responses du User continuent de participer aux Results et aux Analyses qui ne dépendent pas d'une segmentation MAP.

---

## Profil MAP indisponible

Pour une Analysis donnée, un profil MAP peut être indisponible notamment parce que :

- le User ne possède aucun profil ;
- son profil est privé ;
- sa MAPModelVersion n'est pas compatible avec le calcul demandé.

Une Organization ne doit pas pouvoir distinguer individuellement ces raisons.

Le système expose uniquement l'état commun :

```text
MAP indisponible
```

Une Analysis peut afficher :

- la population totale ;
- le nombre de profils MAP utilisables ;
- le nombre de profils MAP indisponibles.

Elle ne révèle jamais quel Participant possède un profil privé.

---

## Compatibilité des versions

Chaque profil conserve la MAPModelVersion utilisée pour son calcul.

Deux versions ne sont jamais considérées comme comparables implicitement.

Dans le MVP :

- un segment MAP opère sur une seule MAPModelVersion ;
- les profils calculés avec une autre version sont indisponibles pour ce segment ;
- aucune conversion automatique des scores n'est réalisée ;
- chaque résultat analytique MAP identifie la version utilisée.

Une future règle explicite de compatibilité pourra autoriser certaines comparaisons sans modifier les profils existants.

---

## Confidentialité

Le User propriétaire peut :

- commencer et compléter ses assessments ;
- consulter son profil actuel ;
- consulter la version du modèle utilisée ;
- remplacer son profil par un nouvel assessment ;
- activer ou désactiver son partage.

Une Organization ne peut jamais :

- consulter les réponses brutes d'un MAPAssessment ;
- modifier un MAPProfile ;
- déclencher un assessment au nom d'un User ;
- savoir si un profil individuellement indisponible est absent ou privé ;
- exploiter un profil hors d'un AnalysisScope autorisé ;
- utiliser MAP pour obtenir une identité qui lui serait autrement masquée.

Dans le MVP, les membres d'une Organization utilisent MAP uniquement au travers d'Analyses agrégées autorisées.

Aucune fiche individuelle ne présente le profil motivationnel complet d'un Participant à un membre de l'Organization.

Le facteur MAP nommé `Addiction` décrit un motif autodéclaré dans l'instrument de recherche.

Il ne constitue jamais un diagnostic médical ou clinique.

---

## MAP et CampaignParticipation

MAP reste facultatif.

Une Campaign ne peut jamais, dans le MVP :

- exiger qu'un User possède un MAPProfile ;
- exiger qu'il partage son profil ;
- empêcher une CampaignParticipation faute de profil ;
- intégrer le MAPAssessment dans son CampaignForm.

Un User peut participer avec :

- aucun profil MAP ;
- un profil privé ;
- un profil partagé.

MAP enrichit l'interprétation analytique après la collecte, mais ne conditionne jamais l'accès à l'étude.

---

## Contrat métier

Le domaine MAP repose sur les règles suivantes :

```text
SignalLab
└── MAPModelVersions

User
├── MAPAssessments
└── MAPProfile actuel
```

Les MAPModelVersions sont définies par SignalLab.

Les assessments et le profil appartiennent au User.

Les Organizations ne peuvent exploiter le profil que comme projection analytique contextuelle, lorsque son partage et les Permissions l'autorisent.

Les données historiques de Campaign ne contiennent jamais de snapshot MAP.

---

## Périmètre MVP

Le MVP implémente :

- les MAPModelVersions publiées et immuables ;
- une seule version active pour les nouveaux assessments ;
- le cycle `Draft`, `Submitted`, `Abandoned` du MAPAssessment ;
- un MAPProfile actuel par User ;
- le calcul déterministe depuis le dernier assessment soumis ;
- le partage global `Private` ou `SharedForResearch` ;
- le profil privé par défaut ;
- l'utilisation dynamique du profil courant dans les Analyses ;
- l'exclusion immédiate après désactivation du partage ;
- l'indisponibilité unifiée protégeant la confidentialité ;
- la segmentation limitée à une seule MAPModelVersion ;
- l'absence totale de snapshot MAP dans les Participations.

---

## Hors MVP

Les évolutions prévues comprennent notamment :

- partage distinct par Organization, Project ou Campaign ;
- comparaison historique des profils d'un User ;
- historique de profils matérialisé ;
- règles explicites de compatibilité entre MAPModelVersions ;
- migration ou recalcul assisté vers une nouvelle version ;
- normes statistiques de référence versionnées ;
- profils individuels consultables avec consentement renforcé ;
- recommandations personnalisées fondées sur MAP ;
- intégration optionnelle de MAP à certains workflows de recrutement ;
- traductions supplémentaires validées psychométriquement.

---

# 11. Architecture du domaine

## Objectif

Ce chapitre regroupe les règles fondamentales qui structurent l'ensemble du Domain Model.

Ces règles s'appliquent à toutes les entités du domaine, indépendamment de leur responsabilité propre.

Elles constituent les principes architecturaux qui garantissent la cohérence fonctionnelle de SignalLab.

---

## Hiérarchie métier

L'ensemble du domaine repose sur des hiérarchies de propriété explicites.

User
├── ParticipantProfile
├── MAPAssessments
├── MAPProfile
└── OrganizationMembers

Organization
└── Project
    ├── Build
    ├── Campaign
    │   ├── CampaignForm
    │   │   └── CampaignFormQuestionMeasureBindings
    │   ├── CampaignParticipation
    │   │   └── Submission
    │   │       └── Response
    │   └── Recruitment Resources
    │
    ├── SavedAnalysis
    └── ResearchBoard *(post-MVP)*

Les ressources partagées par plusieurs Projects appartiennent directement à l'Organization.

Organization
├── Measures
├── FormTemplates
│   └── Questions
│       └── QuestionMeasureBindings
└── Projects

Cette hiérarchie constitue la structure de référence du domaine.

---

## Ownership

Chaque ressource métier possède exactement un propriétaire métier.

Une ressource ne peut jamais appartenir simultanément à plusieurs propriétaires.

Le propriétaire d'une ressource est défini lors de sa création.

Il ne change jamais durant son cycle de vie.

Lorsqu'une ressource doit être utilisée dans un autre contexte, elle est dupliquée plutôt que déplacée.

---

## Identity

Chaque entité possède une identité stable et unique.

L'identité d'une entité est indépendante de son contenu.

Deux entités contenant exactement les mêmes données restent deux entités distinctes dès lors que leurs identités diffèrent.

Une duplication crée toujours une nouvelle identité.

---

## Source de vérité

Les données persistées constituent toujours la source de vérité.

Toutes les projections sont calculées à partir de ces données.

Exemples :

* Analytics ;
* indicateurs ;
* graphiques ;
* statistiques ;
* tableaux de bord.

Aucune projection ne constitue une donnée métier indépendante.

---

## Responsabilité unique

Chaque entité possède une responsabilité métier clairement identifiée.

Une responsabilité ne doit jamais être répartie entre plusieurs entités.

Inversement, une même entité ne doit pas regrouper plusieurs responsabilités métier indépendantes.

---

## Relations métier

Les relations du domaine sont explicites.

Une relation métier est représentée par une entité lorsqu'elle possède son propre cycle de vie ou ses propres informations.

Exemples :

* `OrganizationMember`
* `ProjectMember`
* `CampaignParticipation`

Les mécanismes permettant de créer une relation ne constituent pas nécessairement des relations métier.

Exemples :

* `CampaignInvitation`
* `CampaignAccessLink`
* `CampaignApplication`

Ces mécanismes servent à créer ou modifier une relation existante.

---

## Immutabilité historique

### Objectif

Les données historiques doivent rester reproductibles et interprétables dans le temps.

L'immutabilité ne signifie pas que toutes les données de SignalLab sont figées dès leur création.

Elle distingue quatre catégories de données selon leur responsabilité métier.

---

### 1. Données immuables dès la création

Les éléments qui définissent l'identité, la propriété ou la provenance d'une entité ne changent jamais.

Cela comprend notamment :

* l'identifiant de l'entité ;
* son propriétaire métier ;
* son rattachement hiérarchique ;
* son auteur de création ;
* sa date de création ;
* ses références de provenance historique.

Exemples :

```text
Project.OrganizationId
Build.ProjectId
Campaign.ProjectId
CampaignForm.CampaignId
FormTemplateVersion.FormTemplateId
CampaignParticipation.CampaignId
CampaignParticipation.ParticipantProfileId
Submission.CampaignParticipationId
Response.SubmissionId
Response.CampaignFormQuestionId
```

Une ressource ne peut jamais être déplacée vers un autre propriétaire en modifiant l'un de ces identifiants.

Lorsqu'un besoin de réutilisation existe dans un autre contexte, une nouvelle entité indépendante doit être créée par duplication explicite lorsque cette capacité est autorisée.

---

### 2. Données figées par une transition métier

Certaines données restent modifiables pendant une phase de préparation puis deviennent définitivement immuables après un événement métier.

```text
Campaign Draft
    ↓ CampaignActivated
Configuration historique figée

Submission Draft
    ↓ SubmissionSubmitted ou SubmissionAbandoned
Submission et Responses figées
```

Le verrouillage est garanti par les invariants du Domain et par le Backend.

Le masquage ou la désactivation d'un contrôle dans l'interface ne constitue jamais à lui seul une garantie d'immutabilité.

---

### 3. Données vivantes

Les données vivantes représentent l'état actuel d'une ressource et peuvent évoluer lorsque son cycle de vie l'autorise.

Cela comprend notamment :

* le profil courant d'un User ;
* les préférences utilisateur ;
* les métadonnées descriptives autorisées ;
* le statut d'une entité selon ses transitions documentées ;
* le contenu d'un FormTemplate `Active` ;
* la configuration d'une SavedAnalysis ;
* le contenu d'un ResearchBoard post-MVP ;
* le profil MAP courant.

Une donnée vivante ne réécrit jamais un snapshot ou un enregistrement historique déjà produit.

Les règles spécifiques au profil MAP sont définies séparément.

---

### 4. Projections recalculables

Les Results, Analytics, graphiques, indicateurs et tableaux de bord sont des projections calculées à partir des données métier.

Ils ne constituent pas des données historiques autonomes.

Ils peuvent être recalculés lorsque :

* leur configuration évolue ;
* une donnée vivante autorisée évolue ;
* les sources sélectionnées changent.

Une projection ne modifie jamais les données qui lui servent de source.

---

### Matrice d'immutabilité du MVP

| Ressource | Modifiable pendant la préparation | Déclencheur de gel | Données restant évolutives après le gel |
|---|---|---|---|
| `FormTemplate` | Structure et métadonnées tant qu'il est `Active` | Aucun gel global | Nouvelles modifications et nouvelles publications |
| `FormTemplateVersion` | Jamais | Publication | Aucune |
| `QuestionMeasureBinding` de template | Modifiable dans le FormTemplate actif | Publication de la FormTemplateVersion | Le template actif peut recevoir de nouveaux bindings |
| `CampaignFormQuestionMeasureBinding` | Modifiable dans une Campaign `Draft` | Activation de la Campaign | Aucune |
| `Campaign` | Configuration complète en `Draft` | Activation | Statut, timestamps de transition et métadonnées internes autorisées |
| `CampaignForm` | Structure complète en `Draft` | Activation de la Campaign | Aucune |
| `Build` | Métadonnées descriptives lorsqu'il est `Active` | Identité et provenance figées dès la création | Nom d'affichage, description, tags et notes |
| `Measure` | Métadonnées sans changement de sens lorsqu'elle est `Active` | Identité sémantique et OutcomeDirection figées dès usage historique | Nom, description et catégorie sans redéfinition du concept |
| `CampaignParticipation` | Statut selon les transitions autorisées | Création pour les snapshots ; état terminal pour l'ensemble | Aucune après `Completed`, `Abandoned` ou `Expired` |
| `Submission` | Responses tant qu'elle est `Draft` | `Submitted` ou `Abandoned` | Aucune |
| `Response` | Valeur tant que la Submission est `Draft` | Gel de la Submission | Aucune |
| `OrganizationActivityEntry` post-MVP et événements persistés | Jamais après émission | Création | Aucune |

---

### Snapshots historiques

Lorsqu'une donnée vivante est nécessaire à l'interprétation d'un événement passé, la ressource historique conserve un snapshot immuable de la valeur utilisée à cet instant.

Dans le MVP, une CampaignParticipation conserve notamment :

* les données démographiques nécessaires à l'étude au moment de son acceptation ;
* le contenu ou la version des conditions et du consentement acceptés ;
* la date de cette acceptation.

Une modification ultérieure du ParticipantProfile ne réécrit jamais ces snapshots.

Le profil MAP constitue explicitement une donnée vivante et n'est pas copié dans ces snapshots.

---

### Archivage et suppression

L'archivage est non destructif.

Une ressource archivée est en lecture seule. Lorsqu'une restauration est autorisée par son cycle de vie, elle doit être restaurée avant toute nouvelle modification.

Il retire une ressource des usages actifs tout en conservant :

* son identité ;
* ses relations ;
* ses données historiques ;
* les références provenant d'autres ressources.

Une suppression physique n'est autorisée que pour une ressource de préparation qui :

* n'a produit aucune donnée historique ;
* n'est référencée par aucune autre ressource ;
* n'est nécessaire à aucune trace d'activité.

La suppression physique est interdite dès qu'une ressource participe à l'interprétation historique, notamment pour :

* une Campaign activée ;
* un Build référencé par une Campaign ;
* une FormTemplateVersion publiée ;
* un CampaignForm figé ;
* une CampaignParticipation ;
* une Submission ;
* une Response ;
* une Measure historiquement référencée.

Indépendamment de ces conditions générales, aucune suppression physique de Build n'est proposée dans le MVP.

---

### Correction d'une donnée historique

Une donnée historique incorrecte n'est jamais corrigée silencieusement en place.

Selon le type de ressource, la correction produit :

* une nouvelle FormTemplateVersion ;
* une nouvelle Campaign ;
* un nouveau Build ;
* une nouvelle Measure lorsque le concept analytique change ;
* une nouvelle ressource indépendante lorsque la duplication est autorisée.

L'ancienne donnée reste conservée avec son identité et son contexte d'origine.

Le MVP ne prévoit aucun workflow de modification rétroactive d'une Submission soumise ou de ses Responses.

---

## Références

Une entité peut référencer une autre entité sans en devenir propriétaire.

La référence ne modifie jamais la propriété métier.

Exemples :

* un `QuestionMeasureBinding` référence une `Measure` sans en devenir propriétaire ;
* une `SavedAnalysis` référence des `Campaigns` ;
* un `ResearchBoard` post-MVP référence des `SavedAnalysis`.

Les données référencées restent entièrement indépendantes.

---

## Duplication

La duplication constitue le mécanisme standard de réutilisation.

Une duplication :

* crée une nouvelle identité ;
* crée un nouveau propriétaire ;
* rompt toute synchronisation implicite.

Les modifications effectuées sur la copie n'ont aucun impact sur la ressource d'origine.

---

## Évolution

Les ressources historiques sont préservées par versionnement, création d'une nouvelle entité ou duplication explicite.

Une évolution ou une correction ne modifie jamais rétroactivement l'historique.

Exemples :

* un nouveau `FormTemplateVersion` est créé ;
* une nouvelle `Campaign` est créée ou dupliquée lorsque cette capacité est disponible ;
* un nouveau Build représente une nouvelle identité de version testable ;
* une nouvelle Measure représente un concept analytique différent ;
* une nouvelle `SavedAnalysis` remplace éventuellement une précédente.

---

## Capacités du système

Certaines fonctionnalités représentent des capacités du système plutôt que des entités métier.

Ces capacités manipulent les données du domaine sans devenir elles-mêmes des ressources persistées.

Exemples :

* Analytics ;
* export ;
* import ;
* recherche ;
* validation.

Les capacités peuvent évoluer indépendamment du Domain Model.

---

## Cohérence du domaine

Le Domain Model repose sur les principes suivants :

* chaque donnée possède une source de vérité unique ;
* chaque ressource possède un propriétaire unique ;
* chaque entité possède une responsabilité unique ;
* chaque relation métier est explicitement modélisée ;
* les mécanismes sont distingués des états métier ;
* les projections sont distinguées des données persistées ;
* l'historique est toujours reproductible ;
* la duplication est préférée au déplacement ;
* les références ne modifient jamais la propriété métier.

Ces principes constituent les fondations de l'architecture fonctionnelle de SignalLab.

---

# 12. Domain Events

## Objectif

Les Domain Events représentent les événements métier significatifs qui jalonnent le cycle de vie des entités du domaine.

Ils décrivent des faits ayant eu lieu.

Lorsqu'ils sont persistés, leur identité, leur auteur, leur date, leur contexte et leur contenu sont immuables dès leur émission.

Ils ne représentent ni des commandes, ni des intentions, ni des détails d'implémentation.

Les Domain Events permettent de comprendre comment le domaine évolue dans le temps.

---

## Principes

Un Domain Event :

* représente un fait métier accompli ;
* possède une signification fonctionnelle claire ;
* est immuable ;
* appartient au langage du domaine.

Les Domain Events ne définissent pas la manière dont ils sont implémentés techniquement.

Ils constituent uniquement le vocabulaire des événements métier.

---

## Cycle de vie d'une Organization

OrganizationCreated

Cet événement marque la création d'une nouvelle Organization.

---

## Cycle de vie d'un OrganizationMember

OrganizationMemberCreated
OrganizationMemberRemoved
OrganizationMemberReactivated

### OrganizationMemberCreated

Un Membership est créé avec le statut `Active`, notamment lors de la création de l'Organization pour son premier Owner ou lors de l'acceptation d'une première OrganizationInvitation.

### OrganizationMemberRemoved

Un Membership `Active` perd son accès, passe à `Removed` et renseigne `RemovedAt` sans supprimer son identité ni ses références historiques.

### OrganizationMemberReactivated

L'acceptation d'une nouvelle OrganizationInvitation réactive le même Membership historique, applique le Role assigné et réinitialise `RemovedAt`.

---

## Cycle de vie d'une OrganizationInvitation

OrganizationInvitationCreated
OrganizationInvitationAccepted
OrganizationInvitationDeclined
OrganizationInvitationCancelled
OrganizationInvitationExpired

### OrganizationInvitationCreated

Une invitation nominative est créée avec le statut `Pending`.

### OrganizationInvitationAccepted

L'invitation est acceptée par son User destinataire. Un OrganizationMember est créé ou un Membership `Removed` existant est réactivé atomiquement avec le Role assigné.

### OrganizationInvitationDeclined

Le User destinataire refuse l'invitation.

### OrganizationInvitationCancelled

Un OrganizationMember autorisé annule une invitation encore Pending.

### OrganizationInvitationExpired

L'invitation Pending dépasse sa date de validité et ne peut plus être acceptée.

---

## Cycle de vie d'un Project

ProjectCreated
ProjectArchived
ProjectRestored

### ProjectCreated

Un nouveau Project est créé au sein d'une Organization avec le statut `Active`.

### ProjectArchived

Le Project passe de `Active` à `Archived`.

Les ressources historiques restent conservées.

### ProjectRestored

Le Project passe de `Archived` à `Active`.

---

## Cycle de vie d'un Build

BuildCreated
BuildArchived
BuildRestored

### BuildCreated

Un nouveau Build est créé avec le statut `Active`.

L'événement conserve l'identité de la version testable, son `BuildProvenanceType` et sa `ProvenanceValue`.

### BuildArchived

Le Build passe de `Active` à `Archived`.

### BuildRestored

Le Build passe de `Archived` à `Active`.

---

## Cycle de vie d'une Measure

MeasureCreated
MeasureArchived
MeasureRestored

### MeasureCreated

Une nouvelle Measure est créée avec le statut `Active`.

### MeasureArchived

La Measure passe de `Active` à `Archived`.

### MeasureRestored

La Measure passe de `Archived` à `Active`.

---

## Cycle de vie des QuestionMeasureBindings

### QuestionMeasureBindingCreated

Une relation explicite est créée entre une Question de FormTemplate actif et une Measure.

### QuestionMeasureBindingUpdated

Le mode de contribution, la direction ou le poids d'un QuestionMeasureBinding encore modifiable est changé.

### QuestionMeasureBindingRemoved

Un QuestionMeasureBinding encore modifiable est retiré du FormTemplate actif.

La publication d'une FormTemplateVersion fige définitivement les QuestionMeasureBindings publiés.

### CampaignFormQuestionMeasureBindingCreated

Un binding est copié depuis une FormTemplateVersion ou ajouté explicitement dans le CampaignForm d'une Campaign `Draft`.

### CampaignFormQuestionMeasureBindingUpdated

Le mode, la direction ou le poids d'un CampaignFormQuestionMeasureBinding est modifié pendant que la Campaign reste `Draft`.

### CampaignFormQuestionMeasureBindingRemoved

Un CampaignFormQuestionMeasureBinding est retiré pendant que la Campaign reste `Draft`.

L'activation de la Campaign fige définitivement tous les CampaignFormQuestionMeasureBindings.

---

## Cycle de vie d'un FormTemplate

FormTemplateCreated
FormTemplateVersionPublished
FormTemplateArchived
FormTemplateRestored

### FormTemplateCreated

Un nouveau FormTemplate est créé avec le statut `Active`.

### FormTemplateVersionPublished

Une nouvelle FormTemplateVersion immuable est produite.

Le statut du FormTemplate reste `Active`.

### FormTemplateArchived

Le FormTemplate passe de `Active` à `Archived`.

### FormTemplateRestored

Le FormTemplate passe de `Archived` à `Active`.

---

## Cycle de vie d'une Campaign

CampaignCreated
CampaignActivated
CampaignCompleted
CampaignArchived

### CampaignCreated

Une nouvelle Campaign est créée avec le statut `Draft`.

### CampaignActivated

La Campaign passe de `Draft` à `Active`.

Cet événement applique les invariants associés à l'activation.

### CampaignCompleted

La Campaign passe de `Active` à `Completed`.

La collecte est terminée.

### CampaignArchived

La Campaign passe de `Draft` ou `Completed` à `Archived`.

Les données historiques restent disponibles.

### Événements post-MVP

`CampaignScheduled`, `CampaignUnscheduled`, `CampaignReopened` et `CampaignRestored` pourront être introduits avec les transitions post-MVP correspondantes.

---

## Cycle de vie d'une CampaignParticipation

ParticipationAccepted
ParticipationStarted
ParticipationAbandoned
ParticipationCompleted
ParticipationExpired

### ParticipationAccepted

Une CampaignParticipation est créée après l'acceptation des conditions requises.

Son statut initial est `Accepted`.

### ParticipationStarted

La CampaignParticipation passe de `Accepted` à `Started`.

### ParticipationAbandoned

La CampaignParticipation passe de `Accepted` ou `Started` à `Abandoned`.

### ParticipationCompleted

La CampaignParticipation passe de `Started` à `Completed` après la soumission réussie de sa Submission.

### ParticipationExpired

La CampaignParticipation passe de `Accepted` ou `Started` à `Expired` lorsqu'elle ne peut plus être poursuivie.

### Événements post-MVP

`ParticipantInvited` et `ParticipationDeclined` appartiennent aux workflows d'invitation post-MVP.

---

## Cycle de vie d'une Submission

SubmissionCreated
SubmissionSubmitted
SubmissionAbandoned

### SubmissionCreated

Une Submission est créée avec le statut `Draft`.

Ses Responses peuvent être créées ou modifiées.

### SubmissionSubmitted

La Submission passe de `Draft` à `Submitted`.

Cet événement entraîne notamment :

- le verrouillage de la Submission ;
- le verrouillage des Responses associées ;
- le passage de la CampaignParticipation à `Completed` ;
- la disponibilité des données soumises pour les Results et les Analyses.

### SubmissionAbandoned

La Submission passe de `Draft` à `Abandoned`.

Les données éventuellement déjà saisies sont conservées selon les règles applicables, sans être considérées comme soumises.

---

## Cycle de vie de MAP

### MAPModelVersionPublished

Une nouvelle version du modèle MAP est publiée par SignalLab.

---

### MAPModelVersionActivated

Une MAPModelVersion devient la version utilisée pour la création des nouveaux MAPAssessments.

---

### MAPAssessmentStarted

Un User commence un MAPAssessment avec la MAPModelVersion active.

---

### MAPAssessmentSubmitted

Un User soumet un MAPAssessment complet et valide.

---

### MAPAssessmentAbandoned

Un User abandonne définitivement un MAPAssessment Draft.

---

### MAPProfileCalculated

Le MAPProfile actuel d'un User est créé ou recalculé à partir d'un assessment soumis.

---

### MAPProfileSharingChanged

Le User modifie la préférence de partage de son MAPProfile.

---

## Cycle de vie des ressources d'analyse

### SavedAnalysisCreated

Une configuration d'Analysis est persistée dans un Project.

### SavedAnalysisUpdated

Le Scope, les filtres, groupes, métriques ou visualisations d'une SavedAnalysis sont modifiés.

### SavedAnalysisDeleted

Une SavedAnalysis est supprimée sans supprimer aucune donnée source.

Les événements `ResearchBoardCreated`, `ResearchBoardUpdated` et `ResearchBoardArchived` appartiennent au périmètre post-MVP.

Ces événements n'ont aucun impact sur les données collectées ni sur les résultats analytiques recalculables.

---

## Relations entre événements

Les Domain Events peuvent entraîner d'autres évolutions du domaine.

Exemple :

CampaignActivated
        │
        ├── CampaignForm frozen
        ├── Build reference frozen
        └── Recruitment enabled

Autre exemple :

SubmissionSubmitted
        │
        ├── Submission locked
        ├── Responses locked
        ├── CampaignParticipation completed
        └── Results and Analytics available

Ces conséquences appartiennent au domaine.

Leur implémentation technique est indépendante.

---

## Principes d'évolution

Les Domain Events décrivent des faits passés.

Ils ne sont jamais utilisés pour exprimer une intention.

Exemples :

Correct :

* `CampaignActivated`
* `SubmissionSubmitted`
* `ParticipationCompleted`

Incorrect :

* `ActivateCampaign`
* `SubmitResponses`
* `CompleteParticipation`

Les événements décrivent ce qui s'est produit, jamais ce qui devrait se produire.

---

## Contrat métier

Les Domain Events constituent la chronologie officielle du domaine.

Ils permettent de décrire l'évolution des ressources métier sans dépendre d'une implémentation technique.

Ils servent de langage commun entre les experts métier, les Product Managers et les développeurs.

Leur existence n'implique aucune architecture événementielle particulière.

Ils représentent uniquement les faits significatifs du domaine.

---

# 13. Principales décisions de conception

## Objectif

Ce chapitre documente les décisions structurantes qui ont guidé la conception du Domain Model.

Il ne décrit pas le fonctionnement du domaine.

Il explique pourquoi certaines solutions ont été retenues plutôt que d'autres.

Ces décisions constituent la mémoire architecturale de SignalLab.

---

## Décision 1 — Le `CampaignForm` est une copie du `FormTemplateVersion`

### Contexte

Une Campaign utilise un questionnaire dérivé d'un `FormTemplate`.

Le `FormTemplate` peut continuer à évoluer après la création de la Campaign.

### Choix retenu

Lors de la création d'une Campaign, le `FormTemplateVersion` est copié afin de créer un `CampaignForm` indépendant.

### Justification

Cette approche garantit :

* la reproductibilité historique ;
* l'indépendance des Campaigns ;
* la stabilité des données collectées.

### Conséquences

Les modifications ultérieures du `FormTemplate` n'ont aucun impact sur les Campaigns existantes.

---

## Décision 2 — Les `Measures` appartiennent à l'`Organization`

### Contexte

Les Projects d'une même Organization utilisent souvent le même vocabulaire métier.

### Choix retenu

Les `Measures` sont des ressources partagées au niveau de l'Organization.

### Justification

Cette organisation :

* évite les duplications ;
* garantit un vocabulaire commun ;
* facilite les analyses transverses.

### Conséquences

Les Projects ne possèdent jamais leurs propres `Measures`.

---

## Décision 3 — Une `CampaignParticipation` existe avant toute `Submission`

### Contexte

Le recrutement fait partie intégrante du domaine.

Une invitation ou une acceptation constitue déjà une information métier.

### Choix retenu

Une CampaignParticipation existe indépendamment de toute Submission.

Dans le MVP, elle est créée uniquement après l'acceptation des conditions requises par le Participant.

La cible post-MVP pourra la créer plus tôt dans certains workflows de recrutement, notamment après une invitation nominative.

### Justification

Cette séparation permet :

- de distinguer recrutement, participation et collecte ;
- de représenter une participation acceptée avant toute Submission ;
- de faire évoluer ultérieurement les workflows de recrutement sans modifier le modèle de collecte.

### Conséquences

Une CampaignParticipation peut exister sans aucune Submission.

Une invitation, un refus ou l'ouverture d'un lien ne créent pas automatiquement une CampaignParticipation dans le MVP.

---

## Décision 4 — Les mécanismes sont distincts des états métier

### Contexte

Plusieurs concepts permettent d'initier une participation :

* `CampaignInvitation` ;
* `CampaignAccessLink` ;
* `CampaignApplication`.

### Choix retenu

Ces concepts sont modélisés comme des mécanismes de recrutement.

Ils ne remplacent jamais la `CampaignParticipation`.

### Justification

La relation métier durable reste toujours la participation.

Les mécanismes représentent uniquement la manière dont cette relation est créée.

### Conséquences

Le Domain Model reste centré sur les états métier plutôt que sur les flux.

---

## Décision 5 — Les Analytics sont une capacité, pas une entité

### Contexte

Les résultats analytiques sont calculés à partir des données collectées.

### Choix retenu

Le Domain Model ne contient aucune entité `Analytics`.

Les analyses sont représentées uniquement par leur configuration (`SavedAnalysis`).

### Justification

Les résultats sont recalculables.

Une seule source de vérité est conservée.

### Conséquences

Les analyses sauvegardées restent toujours cohérentes avec les données disponibles.

---

## Décision 6 — Le `ResearchBoard` est post-MVP

### Contexte

Le MVP doit maximiser la puissance d'extraction et de comparaison des données avant d'ajouter un espace collaboratif de raisonnement.

### Choix retenu

Le moteur Analytics et les SavedAnalysis appartiennent au MVP.

Le ResearchBoard, les Notes, Insights, Hypotheses et Decisions appartiennent au post-MVP.

### Justification

Cette séparation permet de concentrer l'effort initial sur la production de preuves riches, traçables et segmentables.

Le futur Research Workspace pourra ensuite organiser ces preuves sans imposer une refonte du moteur analytique.

### Conséquences

Aucun ResearchBoard ni Note n'est requis pour exploiter pleinement les Analyses du MVP.

Lorsqu'il sera introduit, un ResearchBoard référencera les ressources sans en devenir propriétaire.

---

## Décision 7 — Les relations Question–Measure sont explicites, multiples et pondérées

### Contexte

Une même Question peut contribuer à plusieurs concepts analytiques et une Measure peut dépendre de plusieurs Questions d'importance différente.

Une relation directe `Question.MeasureId` ne permet pas de représenter cette richesse.

### Choix retenu

La relation est matérialisée par `QuestionMeasureBinding` avec :

* une Measure ;
* un mode `Scored` ou `Contextual` ;
* une direction `Aligned` ou `Inverted` ;
* un poids relatif strictement positif pour les contributions scorées.

### Justification

Cette relation many-to-many conserve la puissance analytique sans demander à l'utilisateur de configurer des formules statistiques.

Elle permet de relier quantitatif et qualitatif, d'inverser explicitement une contribution et de pondérer plusieurs indicateurs.

### Conséquences

Aucune Question ne possède de `MeasureId` direct.

Aucune Measure, direction ou pondération n'est déduite du texte.

Les QuestionMeasureBindings sont figés dans les FormTemplateVersions puis copiés comme CampaignFormQuestionMeasureBindings propres à chaque Campaign.

---

## Décision 8 — Les scores de Measure sont harmonisés automatiquement par règles structurelles

### Contexte

Les Campaigns peuvent utiliser des échelles différentes pour suivre le même concept.

Refuser toute comparaison ferait perdre la continuité analytique qui constitue la force de SignalLab.

### Choix retenu

Les valeurs scalaires, ordinales et binaires compatibles sont harmonisées sur `0–100` à partir de leur structure historique, puis orientées et pondérées selon leurs bindings.

Le calcul produit d'abord un score par Submission avant toute agrégation de population.

### Justification

Cette méthode permet des comparaisons longitudinales déterministes sans IA ni paramétrage statistique avancé.

Le passage par la Submission garantit un poids égal entre Participants et Campaigns utilisant des nombres différents de Questions.

### Conséquences

Les Questions catégorielles non ordonnées et textuelles ne sont jamais transformées arbitrairement en score.

Les données manquantes ne sont jamais imputées.

Chaque résultat expose sa couverture et sa provenance de calcul.

---

## Décision 9 — Le MVP analytique est ambitieux mais opinionated

### Contexte

L'Analysis Workspace constitue la principale source de valeur du MVP, tandis que le Research Workspace est reporté.

### Choix retenu

Le MVP inclut un ensemble riche mais fini de statistiques et de visualisations déterminées par les familles de données.

L'utilisateur choisit son Scope, ses filtres, ses groupes et une visualisation compatible, mais ne configure aucune formule, calibration ou méthode statistique.

### Justification

Cette approche maximise la puissance d'analyse tout en préservant une UX accessible aux chercheurs qui ne sont pas statisticiens.

### Conséquences

Le moteur calcule automatiquement les statistiques descriptives, Spearman, Hedges g, intervalles d'incertitude et diagnostics de cohérence selon des règles documentées et versionnées.

Les résultats restent des preuves descriptives, jamais des conclusions causales automatiques.

---

## Décision 10 — La duplication est préférée au déplacement

### Contexte

Certaines ressources pourraient être déplacées entre Projects ou Organizations.

### Choix retenu

Le Domain Model privilégie systématiquement la duplication.

### Justification

Cette approche :

* préserve l'historique ;
* évite les effets de bord ;
* simplifie les règles métier.

### Conséquences

Chaque copie possède une identité propre.

Aucune synchronisation implicite n'existe entre les copies.

---

## Décision 11 — Les données historiques sont immuables

### Contexte

Les analyses doivent rester reproductibles dans le temps, y compris lorsqu'une erreur est découverte après l'utilisation d'une ressource.

### Choix retenu

Les ressources historiques ne sont jamais modifiées rétroactivement.

Toute correction qui change l'identité ou le sens historique produit une nouvelle version ou une nouvelle entité.

### Justification

Cette règle garantit :

* la reproductibilité des Campaigns ;
* la traçabilité des corrections ;
* la stabilité des Submissions et Responses ;
* l'absence de réinterprétation silencieuse de l'historique.

### Conséquences

Une FormTemplateVersion, un CampaignForm figé, une Submission soumise et une Response historique ne sont jamais corrigés en place.

Le Domain Model privilégie le versionnement, la création d'une nouvelle référence et la duplication explicite lorsque cette capacité est disponible.

L'ancienne référence reste conservée.

---

## Décision 12 — Chaque ressource possède un propriétaire unique

### Contexte

Une ressource ne doit jamais appartenir simultanément à plusieurs contextes métier.

### Choix retenu

Chaque entité possède un unique propriétaire métier.

### Justification

Cette règle simplifie les responsabilités et la compréhension du domaine.

### Conséquences

Les références ne modifient jamais la propriété des ressources.

---

## Décision 13 — Les projections ne sont jamais la source de vérité

### Contexte

Les graphiques, indicateurs et tableaux de bord sont dérivés des données collectées.

### Choix retenu

Seules les données persistées constituent la source de vérité.

Les projections sont recalculées à la demande.

### Justification

Cette approche garantit la cohérence des analyses et limite les duplications de données.

### Conséquences

Toute évolution des algorithmes analytiques bénéficie immédiatement aux analyses existantes.

---

## Décision 14 — Le profil MAP est actuel, personnel et non snapshoté

### Contexte

MAP doit permettre de segmenter les données de recherche sans transformer le profil motivationnel personnel en donnée appartenant à une Campaign ou à une Organization.

Le profil peut évoluer lorsqu'un User réalise un nouvel assessment ou modifie son consentement de partage.

### Choix retenu

Le MAPProfile appartient directement au User et représente uniquement son profil actuel.

Les CampaignParticipations, Submissions, Responses, SavedAnalysis et AnalysisScopes ne conservent jamais de copie de ses scores.

Les Analyses résolvent dynamiquement le profil actuel, partagé et compatible au moment de leur calcul.

### Justification

Cette approche :

- respecte la propriété personnelle et la confidentialité du profil ;
- évite de multiplier les copies sensibles ;
- permet au User de retirer immédiatement son profil des futures segmentations ;
- conserve les Responses historiques sans les réécrire ;
- distingue clairement données de recherche historiques et grille de lecture motivationnelle actuelle.

### Conséquences

Une SavedAnalysis peut produire un résultat MAP différent lorsqu'elle est recalculée.

Les comparaisons historiques de profils ou les analyses fondées sur un snapshot MAP nécessiteraient une décision métier distincte et restent hors MVP.

---

## Décision 15 — Le Build est une référence métier, pas un artefact

### Contexte

SignalLab doit identifier précisément la version étudiée sans devenir un système de gestion de fichiers, de code source, de compilation ou de distribution.

Les workflows des Organizations peuvent reposer sur des outils très différents et ne disposent pas nécessairement d'un repository ou d'un commit identifiable.

### Choix retenu

Dans le MVP, le Build conserve uniquement :

* l'identité de la version testable ;
* sa plateforme et sa variante éventuelle ;
* une provenance `External` ou `Manual` ;
* ses métadonnées descriptives.

La provenance utilise un unique champ texte `ProvenanceValue` :

* une URL lorsqu'elle est `External` ;
* une description libre lorsqu'elle est `Manual`.

SignalLab ne connaît ni repository, ni branche, ni commit, ni pipeline de compilation.

Il ne stocke, ne distribue et n'exécute aucun artefact dans le MVP.

L'Organization reste responsable de la sécurité, des droits d'accès et de la disponibilité de toute ressource externe référencée.

### Justification

Cette approche :

* conserve uniquement les informations nécessaires à l'identification de ce qui a été testé ;
* reste compatible avec des workflows techniques ou manuels très différents ;
* évite de coupler le Domain Model à Git ou à une plateforme particulière ;
* empêche l'introduction prématurée d'une infrastructure de hosting ;
* maintient une frontière claire entre version étudiée, accès externe et déroulement de la Campaign.

### Conséquences

L'identité et la provenance d'un Build sont immuables dès sa création.

Une correction de ces informations nécessite un nouveau Build.

Les instructions propres à l'étude restent portées par le CampaignContext ou le parcours de Participation.

Les futures responsabilités de fichier, distribution et exécution seront représentées par des concepts séparés tels que `BuildArtifact`, `BuildDistribution` et `BuildExecution` si un besoin métier réel apparaît.

---

## Décision 16 — Les invitations d'Organization précèdent le Membership

### Contexte

Un User doit pouvoir rejoindre une Organization avec un Role attribué sans être considéré comme membre avant son acceptation.

### Décision

L'`OrganizationInvitation` est une entité métier nominative distincte de l'`OrganizationMember`.

Son acceptation crée atomiquement un OrganizationMember ou réactive le Membership `Removed` déjà associé à la même paire User–Organization, puis termine définitivement l'invitation.

### Justification

Cette séparation évite de représenter un accès non accepté comme une appartenance active et permet de tracer explicitement les refus, annulations et expirations.

### Conséquences

Les Invitations n'accordent aucune Permission avant leur acceptation.

Une invitation ne peut être réutilisée et une seule invitation Pending existe par paire Organization–User.

---

# 14. Glossaire

## Objectif

Ce glossaire définit le vocabulaire métier officiel de SignalLab.

Chaque concept possède un nom canonique et une définition unique.

Ces termes constituent le langage commun utilisé dans le Product, l'UX, le Domain Model, les Specs, l'API et le code.

Lorsqu'une formulation secondaire entre en conflit avec ce glossaire, le terme défini ici prévaut.

---

## Conventions terminologiques

### Noms canoniques

Les noms des concepts métier sont écrits en `PascalCase` dans le Domain Model, les contrats techniques et le code.

Dans l'interface ou la prose, ils peuvent être affichés avec des espaces afin de rester lisibles.

Exemples :

- `FormTemplate` devient **Form Template** dans l'interface ;
- `CampaignForm` devient **Campaign Form** ;
- `MAPProfile` devient **MAP Profile**.

Cette différence de présentation ne crée jamais deux concepts distincts.

### Singulier

Le nom canonique d'une entité est toujours défini au singulier.

Le pluriel désigne uniquement une collection de ces entités.

### Rôle et entité

Un rôle contextuel ne doit pas être confondu avec une entité persistante.

Par exemple, **Participant** décrit le rôle d'un `User` prenant part à une Campaign, tandis que `ParticipantProfile` et `CampaignParticipation` sont des entités métier.

### Termes UX

Certains termes sont autorisés comme libellés UX ou raccourcis rédactionnels sans devenir des types métier.

Leur relation avec le concept canonique est explicitement indiquée dans ce glossaire.

---

# Identité et collaboration

## User

Identité globale et authentifiée d'une personne dans SignalLab.

Un `User` existe indépendamment des Organizations auxquelles il appartient et des Campaigns auxquelles il participe.

---

## Participant

Rôle contextuel d'un `User` prenant part ou souhaitant prendre part à une Campaign.

`Participant` n'est pas une entité métier persistante.

L'identité métier correspondante est portée par `ParticipantProfile` et la relation à une Campaign par `CampaignParticipation`.

---

## ParticipantProfile

Entité représentant un `User` dans son rôle de Participant.

Elle centralise les informations personnelles et démographiques utilisées lors des études.

Le MAPProfile appartient directement au User et ne fait pas partie du ParticipantProfile.

Elle n'appartient à aucune Organization.

---

## Organization

Espace collaboratif isolé regroupant des membres, des ressources de recherche partagées et plusieurs Projects.

Une Organization constitue la principale frontière d'isolation des données dans SignalLab.

---

## OrganizationMember

Entité matérialisant l'appartenance historique unique d'un `User` à une `Organization`.

Elle porte son Role actif et possède le cycle `Active ⇄ Removed`. Un Membership retiré peut être réactivé par une nouvelle OrganizationInvitation sans changer d'identité.

---

## OrganizationInvitation

Invitation nominative et temporaire permettant à un User existant de rejoindre une Organization avec un Role déterminé.

Elle ne confère aucun accès avant son acceptation et crée atomiquement un OrganizationMember, ou réactive le Membership `Removed` existant pour la même paire User–Organization, lorsqu'elle est acceptée.

---

## Role

Ensemble cohérent de `Permissions` attribuable à un `OrganizationMember` au sein d'une Organization.

Dans le MVP, chaque OrganizationMember possède exactement un Role actif parmi les trois Roles système `Member`, `Administrator` et `Owner`.

---

## Member

Role système MVP permettant de travailler sur les ressources de recherche de l'Organization sans administrer ses membres ni sa gouvernance.

---

## Administrator

Role système MVP disposant des capacités du `Member` et de la gestion courante de l'Organization et de ses membres, à l'exclusion des opérations réservées aux Owners.

---

## Owner

Role système MVP disposant de toutes les capacités d'administration et portant la gouvernance de l'Organization.

Une Organization conserve toujours au moins un Owner actif.

---

## Permission

Autorisation fonctionnelle prédéfinie par SignalLab.

Une Permission est référencée par un `Role` et n'est jamais attribuée directement à un User.

---

## Permission Catalogue

Catalogue global des `Permissions` définies et maintenues par SignalLab.

Les Organizations utilisent ce catalogue mais ne peuvent pas créer ou modifier ses Permissions.

---

## Project

Espace de travail appartenant à une `Organization` et regroupant les ressources opérationnelles liées à un ensemble cohérent d'études.

Un Project possède notamment ses Builds et ses Campaigns.

---

## ProjectMember

Entité post-MVP représentant l'accès d'un `OrganizationMember` à un Project spécifique.

Elle n'est pas implémentée dans le MVP, où tous les accès aux Projects sont hérités de l'Organization.

---

## ProjectRole

Rôle post-MVP attribuable à un `ProjectMember` dans le périmètre d'un seul Project.

Il reste distinct d'un `Role` d'Organization.

---

# Ressources de recherche

## Build

Référence métier identifiant une version testable précise du produit, prototype ou expérience étudiée dans une ou plusieurs Campaigns.

Dans le MVP, un Build contient uniquement son identité, sa plateforme, sa variante éventuelle, sa provenance et ses métadonnées descriptives.

Il ne représente ni un fichier binaire, ni un exécutable, ni un déploiement hébergé par SignalLab.

---

## BuildProvenanceType

Type indiquant comment la version référencée par un `Build` peut être localisée ou identifiée dans le MVP.

Les valeurs canoniques sont :

* `External`, lorsque `ProvenanceValue` contient une URL ;
* `Manual`, lorsque `ProvenanceValue` contient une description libre.

SignalLab ne gère pas les droits d'accès ou la disponibilité de la ressource externe.

---

## Measure

Concept analytique stable représentant un aspect du produit que l'Organization souhaite comprendre et suivre dans le temps.

Une Measure ne collecte aucune donnée directement.

Les Questions produisent des Responses et leurs QuestionMeasureBindings permettent de conserver une continuité analytique entre plusieurs questionnaires et Campaigns.

---

## MeasureOutcomeDirection

Direction d'interprétation produit d'une Measure : `HigherIsFavorable`, `HigherIsUnfavorable` ou `Neutral`.

Elle ne décrit pas la direction d'une Question particulière.

---

## MeasureScore

Score harmonisé sur `0–100` représentant la quantité réelle du concept mesuré.

Il est calculé d'abord par Submission puis agrégé sur une population.

---

## OutcomeScore

Projection optionnelle orientant un MeasureScore vers une lecture favorable lorsque la Measure n'est pas neutre.

Il ne remplace jamais le MeasureScore.

---

## FormTemplate

Modèle de questionnaire réutilisable appartenant à une Organization.

Un FormTemplate sert à préparer des Campaign Forms mais n'est jamais utilisé directement pour collecter des Responses.

---

## FormTemplateVersion

Version publiée et immuable d'un `FormTemplate`.

Elle représente exactement la structure du Form Template à l'instant de sa publication et peut servir de source à plusieurs Campaign Forms.

---

## Section

Groupe ordonné de `Questions` à l'intérieur d'un `FormTemplate` ou d'une `FormTemplateVersion`.

Une Section structure la progression du questionnaire mais ne possède pas de signification analytique propre.

---

## QuestionType

Type structurel d'une Question déterminant sa configuration, la validation de sa Response et sa famille analytique.

Le MVP prend en charge `Scale`, `Number`, `Boolean`, `SingleChoice`, `MultipleChoice`, `ShortText` et `LongText`.

---

## Question

Unité de collecte définie dans un `FormTemplate`.

Une Question précise notamment son libellé, son type, sa configuration, ses règles de validation et ses QuestionMeasureBindings éventuels.

Elle n'est jamais référencée directement par une Response de Campaign.

---

## QuestionMeasureBinding

Relation analytique explicite entre une Question et une Measure.

Elle porte un mode de contribution, une direction et un poids relatif.

Une Question peut posséder plusieurs bindings vers des Measures différentes.

---

## QuestionMeasureContributionMode

Mode `Scored` ou `Contextual` indiquant si la Response participe au score quantitatif de la Measure ou enrichit uniquement son interprétation.

---

## QuestionMeasureContributionDirection

Direction `Aligned` ou `Inverted` indiquant comment une valeur élevée de la Question contribue à la Measure associée.

---

## BindingWeight

Poids relatif strictement positif d'un QuestionMeasureBinding scoré parmi les contributions de la même Measure.

Les poids sont normalisés automatiquement lors du calcul.

---

# Campaigns et collecte

## Campaign

Étude de recherche distincte réalisée dans un `Project` sur un Build donné et à l'aide d'un Campaign Form propre.

Une Campaign organise son contexte, ses règles de participation, ses Campaign Participations et les données collectées.

---

## CampaignForm

Questionnaire propre à une `Campaign`, produit par copie complète d'une `FormTemplateVersion`.

Le Campaign Form est indépendant de sa source, reste modifiable tant que la Campaign est `Draft` et devient définitivement immuable lors de son activation.

---

## CampaignFormSection

Copie d'une Section appartenant exclusivement à un `CampaignForm`.

Elle possède sa propre identité et ne reste jamais synchronisée avec la Section source.

---

## CampaignFormQuestion

Question telle qu'elle existe dans une Campaign précise.

Elle appartient à un `CampaignForm`, possède sa propre identité, conserve ses CampaignFormQuestionMeasureBindings historiques et constitue la référence directe utilisée par les `Responses`.

---

## CampaignFormQuestionMeasureBinding

Copie indépendante et historiquement figée d'un `QuestionMeasureBinding` dans une Campaign précise.

Elle porte la Measure, le mode, la direction et le poids effectivement utilisés pour interpréter les Responses de cette Campaign.

---

## CampaignContext

Informations décrivant les conditions dans lesquelles l'expérience étudiée est réalisée.

Il peut notamment préciser le mode de participation, l'environnement, la plateforme, la durée attendue et les instructions de l'étude.

Les informations participant-facing et nécessaires à l'interprétation historique deviennent immuables lors de l'activation de la Campaign.

---

## CampaignConsentDefinition

Définition figée des conditions qu'un Participant doit accepter lorsque `CampaignParticipationSettings.RequireExplicitConsent = true`.

Le setting constitue l'unique source de vérité sur l'obligation de consentement. La définition constitue la source du ConsentSnapshot historique lorsqu'un consentement explicite est effectivement requis.

---

## CampaignParticipationSettings

Ensemble des règles générales d'accès et de participation propres à une Campaign.

Ces paramètres définissent notamment le mode d'accès, le consentement requis et le nombre maximal de Submissions autorisées.

---

## CampaignInvitation

Invitation nominative permettant à un Participant identifié de rejoindre une Campaign.

Elle constitue un mécanisme de recrutement et non la Participation elle-même.

---

## CampaignAccessLink

Lien `Active` ou `Disabled` permettant à un User de demander à rejoindre une Campaign selon les règles d'accès configurées et une expiration éventuelle.

L'ouverture du lien ne constitue pas une `CampaignParticipation` et le MVP ne limite pas son nombre global d'utilisations.

---

## CampaignApplication

Candidature soumise par un Participant afin de rejoindre une Campaign lorsque son admission nécessite une validation préalable.

Une candidature acceptée peut créer une `CampaignParticipation`.

---

## CampaignAnnouncement

Publication rendant une Campaign visible auprès de Participants potentiels.

Elle constitue un canal de recrutement et non une Participation.

---

## RecruitmentSource

Valeur décrivant le mécanisme ayant conduit à la création d'une `CampaignParticipation`.

Elle peut notamment désigner une invitation, un lien d'accès, une candidature ou une annonce.

---

## CampaignParticipation

Relation métier entre un `ParticipantProfile` et une `Campaign`.

Elle représente l'engagement, l'état d'avancement et le contexte de participation du Participant, indépendamment de toute Submission.

---

## Participation

Libellé UX et raccourci rédactionnel désignant une `CampaignParticipation`.

`Participation` ne constitue pas un type métier distinct dans le Domain Model ou le code.

---

## DemographicSnapshot

Value Object immuable appartenant à une `CampaignParticipation`.

Il conserve les informations démographiques nécessaires à l'étude telles qu'elles existaient au moment de l'acceptation, sans inclure le profil MAP.

---

## ConsentSnapshot

Value Object immuable et facultatif appartenant à une `CampaignParticipation`.

Il existe uniquement lorsqu'un consentement explicite est requis et conserve le contenu ou la version des conditions acceptées. La date d'acceptation canonique est portée par `CampaignParticipation.AcceptedAt`.

---

## Submission

Envoi complet regroupant les `Responses` produites dans le cadre d'une `CampaignParticipation`.

Dans le MVP, une Campaign Participation possède au maximum une Submission.

Une fois soumise, elle devient immuable avec ses Responses.

---

## Response

Réponse individuelle apportée à une `CampaignFormQuestion` dans une `Submission`.

Les Responses constituent la source de vérité des valeurs collectées.

---

## Results

Espace fonctionnel de consultation en lecture seule des Campaign Participations, Submissions et Responses collectées.

`Results` est un concept produit et UX, pas une entité métier persistante.

---

# Analyse et recherche

## Analytics

Capacité du système regroupant les calculs, agrégations, filtres, segmentations, statistiques et visualisations produits à partir des données métier.

Les Analytics sont déterministes, versionnés, recalculables et ne constituent jamais une entité persistante ni une source de vérité.

---

## Analysis

Travail temporaire d'exploration réalisé à partir des données collectées et des Analytics.

Une Analysis peut être enregistrée sous la forme d'une `SavedAnalysis`.

---

## SavedAnalysis

Configuration persistée d'une Analysis.

Elle conserve son Scope, ses sujets, groupes, filtres, métriques et visualisations, mais ne stocke jamais une copie des données ni des résultats calculés.

---

## AnalysisScope

Périmètre de données d'une Analysis, composé d'un Project et d'une ou plusieurs Campaigns de ce Project.

---

## AnalysisGroup

Sous-population nommée d'un AnalysisScope définie par un ensemble de filtres comparables aux autres groupes de la même Analysis.

---

## SubmissionMeasureScore

Score d'une Measure calculé pour une Submission à partir des contributions scorées, orientées et pondérées disponibles.

Il constitue l'unité utilisée avant toute agrégation de population.

---

## WeightCoverage

Rapport entre le poids des contributions répondues et le poids attendu pour une Measure dans une Submission.

Il permet d'exposer la complétude du score sans imputer les valeurs manquantes.

---

## EffectSize

Grandeur descriptive exprimant l'ampleur d'une différence entre groupes, notamment par `Hedges g` pour les scores quantitatifs.

Elle ne constitue pas une preuve causale.

---

## AnalyticalEngineVersion

Version des règles déterministes utilisées pour normaliser, agréger et calculer les statistiques d'une Analysis.

Elle est exposée pour assurer la traçabilité méthodologique sans persister les résultats calculés.

---

## ResearchBoard

Espace de travail post-MVP permettant d'organiser des Saved Analyses, des observations, des Notes et d'autres ressources de recherche.

Un Research Board organise ces éléments sans devenir propriétaire de leurs données.

---

## Note

Observation, hypothèse, interprétation ou décision rédigée dans un `ResearchBoard`.

Une Note appartient à un seul Research Board.

---

# MAP

## MAPModelVersion

Version publiée et immuable de la définition du modèle motivationnel MAP maintenu par SignalLab.

Elle contient les dimensions, les items et les règles de calcul.

Une seule version est active pour la création de nouveaux MAPAssessments à un instant donné.

---

## MAPAssessment

Questionnaire personnel complété explicitement par un `User` afin de calculer ou mettre à jour son MAPProfile.

Il appartient au User, référence une MAPModelVersion immuable et reste distinct des CampaignForms.

Une Organization n'accède jamais à ses réponses brutes.

---

## MAPProfile

Profil motivationnel multidimensionnel actuel d'un `User`, calculé à partir d'un MAPAssessment soumis et d'une MAPModelVersion.

Il appartient directement au User et reste privé par défaut.

Les Analytics utilisent uniquement sa valeur actuelle lorsqu'il est partagé, compatible et autorisé dans leur AnalysisScope.

Il n'est jamais copié dans une CampaignParticipation, une Submission, une Response ou une SavedAnalysis.

---

# Termes non canoniques ou ambigus

| Terme à éviter                        | Terme canonique ou usage attendu                                                  |
|---------------------------------------|-----------------------------------------------------------------------------------|
| `OrganizationMembership`              | `OrganizationMember`                                                              |
| `Invitation` sans contexte            | `OrganizationInvitation` ou `CampaignInvitation` selon le contexte                |
| `OrganizationRole`                    | `Role`                                                                            |
| `CampaignParticipant`                 | `Participant`, `ParticipantProfile` ou `CampaignParticipation` selon le contexte  |
| `Participant` comme type persistant   | `ParticipantProfile` ou `CampaignParticipation`                                   |
| `Participation` comme type du Domain  | `CampaignParticipation`                                                           |
| `Answer`                              | `Response`                                                                        |
| `Survey`                              | `FormTemplate` ou `CampaignForm` selon le contexte                                |
| `Form` sans contexte                  | `FormTemplate` ou `CampaignForm`                                                  |
| `Result` comme donnée persistée       | `Response`, `Submission` ou `Results` selon le contexte                           |
| `Analytics` comme entité              | `SavedAnalysis` lorsqu'une configuration persistée est visée                      |
| `Question.MeasureId`                  | `QuestionMeasureBinding`                                                          |
| `Score global de formulaire`          | Scores par `Measure` ou résultats par Question selon le besoin                    |
| `MAP Profile` comme nom de type       | `MAPProfile` dans le Domain et le code                                            |

Les variantes avec espaces restent autorisées comme libellés d'interface, sans créer de nouveaux concepts métier.
