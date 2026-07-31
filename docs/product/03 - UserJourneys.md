# User Journey 00 — Accéder à SignalLab

## Objectif

Permettre au `User` d'accéder rapidement aux espaces de travail et aux études auxquels il participe, ou d'initier une nouvelle activité.

---

## Acteurs

- `User`

---

## Préconditions

- Le `User` possède un compte SignalLab.

---

## Déclencheur

Le `User` se connecte à SignalLab.

---

## Parcours principal

Le `User` s'authentifie dans SignalLab.

Après son authentification, SignalLab affiche la page d'accueil personnelle du `User`.

Depuis cette page, le `User` peut à tout moment :

- créer une nouvelle `Organization` ;
- rejoindre une `Organization` existante ;
- rejoindre une `Campaign`.

Le `User` consulte ensuite les ressources auxquelles il a accès.

S'il souhaite poursuivre une activité de recherche, il sélectionne l'une de ses `Organizations`.

S'il souhaite poursuivre une participation, il sélectionne l'une des `Campaigns` auxquelles il participe.

La sélection d'une ressource établit le contexte de travail correspondant et permet au `User` d'accéder aux fonctionnalités associées.

Pendant toute sa navigation dans SignalLab, le `User` peut consulter le centre de notifications afin de prendre connaissance des invitations et des événements récents le concernant.

Le parcours est terminé.

---

## Parcours alternatifs

### Le `User` n'appartient à aucune `Organization`

Le `User` peut créer une nouvelle `Organization` ou rejoindre une `Organization` existante.

---

### Le `User` ne participe à aucune `Campaign`

Le `User` peut rejoindre une nouvelle `Campaign` à l'aide d'un lien, d'un code ou d'une invitation.

---

## Cas d'échec

### Une ressource n'est plus accessible

Une `Organization` ou une `Campaign` n'est plus accessible au `User` (suppression, perte d'accès, révocation, etc.).

SignalLab informe le `User` et met à jour les ressources affichées.

---

### Erreur technique

Une erreur empêche l'authentification ou le chargement de la page d'accueil.

SignalLab informe le `User`.

---

## Résultat

Le `User` accède au contexte de travail correspondant à son objectif ou initie une nouvelle activité depuis la page d'accueil.

---

## Règles métier

- Seuls les `Users` authentifiés peuvent accéder à SignalLab.
- La page d'accueil présente uniquement les ressources accessibles au `User`.
- La page d'accueil constitue le point d'entrée vers les `Organizations` et les `Campaigns` du `User`.
- La page d'accueil ne constitue pas un processus d'onboarding.
- Le contexte de travail est établi par la sélection d'une `Organization` ou d'une `Campaign`.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `Campaign`
- `CampaignParticipation`

---

# User Journey 01 — Créer une `Organization`

## Objectif

Permettre à un `User` de créer une nouvelle `Organization` afin de disposer d'un espace de travail dédié à la gestion de ses projets de recherche et de ses collaborateurs.

---

## Acteurs

- `User`

---

## Préconditions

- Le `User` est authentifié dans SignalLab.

---

## Déclencheur

Le `User` sélectionne l'action **Créer une `Organization`** depuis sa page d'accueil.

---

## Parcours principal

Le `User` démarre la création d'une nouvelle `Organization`.

SignalLab lui demande uniquement les informations minimales nécessaires à sa création :

- **Nom** *(obligatoire)*
- **Pays** *(obligatoire)*

Après validation, SignalLab crée la nouvelle `Organization`.

Le `User` devient automatiquement le premier membre de cette `Organization` avec le rôle de `Owner`.

SignalLab ouvre ensuite la page de l'`Organization` nouvellement créée.

Si cette `Organization` ne contient encore aucun `Project`, la page affiche un état vide invitant le `User` à créer son premier `Project`.

Les informations complémentaires de l'`Organization`, telles que sa description, son logo ou son site web, pourront être renseignées ultérieurement depuis les paramètres de l'`Organization`.

Le parcours de création est alors terminé.

---

## Parcours alternatifs

### Création annulée

Le `User` abandonne la création avant sa validation.

Aucune `Organization` n'est créée et le `User` revient à sa page d'accueil.

---

### Informations invalides

Une ou plusieurs informations sont invalides ou incomplètes.

SignalLab indique les champs concernés afin que le `User` puisse les corriger avant de poursuivre.

---

## Cas d'échec

Une erreur technique empêche la création de l'`Organization`.

SignalLab informe le `User` de l'échec et l'invite à réessayer.

---

## Résultat

Une nouvelle `Organization` est créée.

Le `User` en est le `Owner` et accède immédiatement à son espace de travail.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `Role`

---

# User Journey 02 — Inviter un `User` à rejoindre une `Organization`

## Objectif

Permettre à un membre autorisé d'inviter un `User` existant à rejoindre une `Organization` en lui attribuant un rôle et une durée de validité pour son invitation.

---

## Acteurs

- Membre de l'`Organization` disposant des permissions nécessaires
- `User` invité

---

## Préconditions

- Le membre est authentifié dans SignalLab.
- Le membre appartient à l'`Organization`.
- Le membre dispose des permissions nécessaires pour inviter de nouveaux membres.
- Le `User` invité possède déjà un compte SignalLab.

---

## Déclencheur

Le membre sélectionne l'action **Inviter un membre** depuis la page de l'`Organization`.

---

## Parcours principal

Le membre recherche un `User` existant dans SignalLab.

Il sélectionne le compte correspondant.

SignalLab lui demande de définir les paramètres de l'`OrganizationInvitation` :

- Rôle attribué au `User`.
- Durée de validité de l'`OrganizationInvitation` :
  - 24 heures
  - 7 jours *(par défaut)*
  - 30 jours

Le membre valide la création de l'`OrganizationInvitation`.

SignalLab crée une `OrganizationInvitation` nominative liée :

- au `User` destinataire ;
- à l'`Organization` ;
- au rôle attribué ;
- au membre ayant créé l'`OrganizationInvitation`.

Le destinataire reçoit une notification dans SignalLab.

L'`OrganizationInvitation` apparaît dans la liste des invitations en attente de l'`Organization`.

Le membre peut copier le lien unique associé à cette `OrganizationInvitation` afin de le transmettre au destinataire par le canal de son choix.

Le parcours est terminé.

---

## Parcours alternatifs

### Aucun `User` trouvé

La recherche ne retourne aucun compte.

SignalLab informe le membre que le destinataire doit disposer d'un compte SignalLab avant de pouvoir être invité.

---

### Invitation déjà en attente

Une `OrganizationInvitation` valide existe déjà pour ce `User` et cette `Organization`.

SignalLab empêche la création d'une nouvelle `OrganizationInvitation` et permet de consulter ou copier le lien de l'`OrganizationInvitation` existante.

---

### Le `User` possède déjà un Membership actif

Le `User` possède déjà un `OrganizationMember` en statut `Active` dans l'`Organization`.

SignalLab l'indique au membre et aucune `OrganizationInvitation` n'est créée.

---

### Le `User` possède un Membership retiré

Le `User` possède un `OrganizationMember` historique en statut `Removed`.

SignalLab autorise la création d'une nouvelle `OrganizationInvitation`. Son acceptation réactivera le même Membership avec le rôle attribué par cette nouvelle invitation.

---

### Création annulée

Le membre abandonne la création avant sa validation.

Aucune `OrganizationInvitation` n'est créée.

---

### Invitation annulée

Le membre annule une `OrganizationInvitation` en attente.

L'`OrganizationInvitation` devient immédiatement inutilisable et disparaît des actions en attente du destinataire.

---

## Cas d'échec

### Permissions insuffisantes

Le membre ne possède pas les permissions nécessaires pour inviter de nouveaux membres.

SignalLab refuse l'action.

---

### Erreur technique

Une erreur empêche la création de l'`OrganizationInvitation`.

SignalLab informe le membre de l'échec et aucune `OrganizationInvitation` n'est créée.

---

## Résultat

Une `OrganizationInvitation` nominative est créée et reste valide jusqu'à son acceptation, son refus, son expiration ou son annulation.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `Role`
- `OrganizationInvitation`

---

# User Journey 03 — Rejoindre une `Organization`

## Objectif

Permettre à un `User` disposant d'un compte SignalLab de rejoindre une `Organization` existante à la suite d'une `OrganizationInvitation` nominative.

---

## Acteurs

- `User` invité
- Membre de l'`Organization` disposant des permissions nécessaires pour inviter de nouveaux membres

---

## Préconditions

- Le `User` possède déjà un compte SignalLab.
- Une `OrganizationInvitation` nominative valide a été créée pour ce `User`.
- L'`OrganizationInvitation` précise l'`Organization` concernée, le rôle attribué et sa date d'expiration.

---

## Déclencheur

Le `User` ouvre l'`OrganizationInvitation` depuis le centre de notifications ou via le lien unique associé à cette `OrganizationInvitation`.

---

## Parcours principal

Le `User` ouvre une `OrganizationInvitation` à rejoindre une `Organization`.

Si le `User` n'est pas authentifié, SignalLab lui demande de s'authentifier avant de poursuivre.

Une fois authentifié, SignalLab le redirige automatiquement vers l'`OrganizationInvitation` initialement ouverte.

SignalLab vérifie que l'`OrganizationInvitation` est valide et qu'elle est bien destinée au `User` authentifié.

SignalLab présente les informations essentielles de l'`Organization`, ainsi que le rôle qui sera attribué au `User`.

Le `User` accepte l'`OrganizationInvitation`.

Si aucun Membership historique n'existe, SignalLab crée son `OrganizationMember` avec le rôle défini dans l'`OrganizationInvitation`.

Si un OrganizationMember en statut `Removed` existe déjà, SignalLab réactive cette même entité et lui applique le rôle défini dans la nouvelle invitation.

L'`OrganizationInvitation` est alors marquée comme acceptée et ne nécessite plus aucune action.

L'`Organization` apparaît dans la liste des `Organizations` accessibles depuis la page d'accueil.

SignalLab ouvre ensuite la page de l'`Organization`.

Le parcours est terminé.

---

## Parcours alternatifs

### Invitation refusée

Le `User` refuse l'`OrganizationInvitation`.

SignalLab marque l'`OrganizationInvitation` comme refusée et la retire des actions en attente.

Le `User` ne rejoint pas l'`Organization`.

---

### Invitation annulée

L'`OrganizationInvitation` a été annulée avant son acceptation.

SignalLab informe le `User` que cette `OrganizationInvitation` n'est plus valide.

---

### Invitation expirée

Le `User` ouvre une `OrganizationInvitation` dont la durée de validité est dépassée.

SignalLab informe le `User` que l'`OrganizationInvitation` a expiré et qu'une nouvelle devra être créée.

---

### Le `User` possède déjà un Membership actif

Le `User` possède déjà un `OrganizationMember` en statut `Active` dans cette `Organization`.

SignalLab l'en informe et lui permet d'accéder directement à celle-ci.

---

## Cas d'échec

### Compte authentifié incorrect

Le lien est ouvert par un `User` différent de celui auquel l'`OrganizationInvitation` est destinée.

SignalLab refuse l'accès à l'`OrganizationInvitation` et informe le `User` que celle-ci ne lui est pas destinée.

---

### Erreur technique

Une erreur empêche l'ajout du `User` à l'`Organization`.

SignalLab informe le `User` de l'échec sans invalider l'`OrganizationInvitation`, afin qu'il puisse réessayer ultérieurement.

---

## Résultat

Le `User` devient membre actif de l'`Organization` avec le rôle défini dans l'`OrganizationInvitation`, soit par création de son Membership, soit par réactivation de son OrganizationMember historique.

L'`Organization` est désormais accessible depuis sa page d'accueil et le `User` est redirigé vers sa page.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `Role`
- `OrganizationInvitation`

---

# User Journey 04 — Créer un `Project`

## Objectif

Permettre à un membre autorisé d'une `Organization` de créer un nouveau `Project` afin d'y organiser une ou plusieurs `Campaigns` ainsi que les ressources associées.

---

## Acteurs

- Membre de l'`Organization` disposant des permissions nécessaires

---

## Préconditions

- Le membre est authentifié dans SignalLab.
- Le membre appartient à l'`Organization`.
- Le membre dispose des permissions nécessaires pour créer un `Project`.

---

## Déclencheur

Le membre sélectionne l'action **Créer un `Project`** depuis la page de l'`Organization`.

---

## Parcours principal

SignalLab demande au membre de renseigner le nom du `Project`.

Le membre saisit le nom du `Project`.

Le membre valide la création.

SignalLab crée le `Project` au sein de l'`Organization` courante.

SignalLab ouvre ensuite la page du nouveau `Project`.

Le `Project` étant initialement vide, sa page présente les différentes sections permettant de créer les ressources du projet ainsi qu'une première `Campaign`.

Le parcours est terminé.

---

## Parcours alternatifs

### Création annulée

Le membre abandonne la création avant sa validation.

Aucun `Project` n'est créé.

---

### Nom invalide ou manquant

Le nom saisi est invalide ou absent.

SignalLab informe le membre du problème et empêche la création tant que celui-ci n'est pas corrigé.

---

## Cas d'échec

### Permissions insuffisantes

Le membre ne possède pas les permissions nécessaires pour créer un `Project`.

SignalLab refuse l'action.

---

### Erreur technique

Une erreur empêche la création du `Project`.

SignalLab informe le membre de l'échec et aucun `Project` n'est créé.

---

## Résultat

Un nouveau `Project` vide est créé dans l'`Organization`.

Le membre est redirigé vers la page du `Project`, depuis laquelle il peut commencer à créer les ressources nécessaires ainsi que ses futures `Campaigns`.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `Project`
- `Campaign`

---

# User Journey 05 — Créer une `Campaign`

## Objectif

Permettre à un membre autorisé d'un `Project` de créer une nouvelle `Campaign` afin de préparer une étude et de la configurer progressivement avant son activation.

---

## Acteurs

- Membre du `Project` disposant des permissions nécessaires

---

## Préconditions

- Le membre est authentifié dans SignalLab.
- Le membre appartient au `Project`.
- Le membre dispose des permissions nécessaires pour créer une `Campaign`.
- Le `Project` existe.

---

## Déclencheur

Le membre sélectionne l'action **Créer une `Campaign`** depuis la page du `Project`.

---

## Parcours principal

SignalLab demande au membre de renseigner les informations initiales de la `Campaign` :

- Nom obligatoire
- Description facultative

Le membre valide la création.

SignalLab crée la `Campaign` dans le `Project` courant.

La `Campaign` est créée avec le statut `Draft`, quel que soit son niveau de configuration.

SignalLab ouvre ensuite la page de la nouvelle `Campaign`.

SignalLab met en évidence les éléments restant à configurer avant que la `Campaign` puisse être activée.

Le membre peut compléter progressivement la configuration de la `Campaign`.

Le parcours est terminé.

---

## Parcours alternatifs

### Création annulée

Le membre abandonne la création avant sa validation.

Aucune `Campaign` n'est créée.

---

### Nom invalide ou manquant

Le nom saisi est absent ou invalide.

SignalLab informe le membre du problème et empêche la création tant que celui-ci n'est pas corrigé.

---

## Cas d'échec

### Permissions insuffisantes

Le membre ne possède pas les permissions nécessaires pour créer une `Campaign` dans ce `Project`.

SignalLab refuse l'action.

---

### `Project` indisponible

Le `Project` n'existe plus ou n'est plus accessible au membre.

SignalLab empêche la création de la `Campaign`.

---

### Erreur technique

Une erreur empêche la création de la `Campaign`.

SignalLab informe le membre de l'échec et aucune `Campaign` n'est créée.

---

## Résultat

Une nouvelle `Campaign` est créée dans le `Project` avec le statut `Draft`.

Le membre est redirigé vers la page de la `Campaign`, depuis laquelle il peut poursuivre sa configuration.

La `Campaign` ne peut pas passer au statut `Active` tant que ses prérequis d'activation ne sont pas satisfaits.

---

## Règles métier

- Une `Campaign` appartient à un seul `Project`.
- Une `Campaign` nouvellement créée possède toujours le statut `Draft`.
- Une `Campaign` peut être créée même si sa configuration est incomplète.
- Aucun `Build` ni autre élément de configuration n'est requis au moment de la création.
- Les prérequis sont évalués dynamiquement à partir de la configuration de la `Campaign`.
- Les prérequis sont vérifiés au moment du passage du statut `Draft` au statut `Active`.
- Une `Campaign` en statut `Draft` peut être archivée sans suppression physique.
- Le cycle de vie d'une `Campaign` est :

Draft
├──→ Active
└──→ Scheduled → Active
Active → Completed

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `Project`
- `Campaign`
- `CampaignStatus`

---

# User Journey 06 — Configurer une `Campaign`

## Objectif

Permettre à un membre autorisé d'un `Project` de compléter progressivement la configuration d'une `Campaign` afin qu'elle satisfasse les conditions nécessaires à son lancement.

---

## Acteurs

- Membre du `Project` disposant des permissions nécessaires

---

## Préconditions

- Le membre est authentifié dans SignalLab.
- Le membre appartient au `Project`.
- Le membre dispose des permissions nécessaires pour modifier la `Campaign`.
- La `Campaign` existe et possède le statut `Draft`.

---

## Déclencheur

Le membre ouvre une `Campaign` en statut `Draft`.

---

## Parcours principal

Le membre consulte la configuration actuelle de la `Campaign`.

SignalLab présente les différentes ressources et paramètres pouvant être configurés.

Le membre ajoute, modifie ou supprime les éléments de configuration de la `Campaign`.

Après chaque modification, SignalLab enregistre les changements.

SignalLab réévalue les prérequis nécessaires au lancement de la `Campaign`.

Le membre peut interrompre sa configuration à tout moment puis la reprendre ultérieurement.

Le membre poursuit la configuration jusqu'à ce qu'il considère sa `Campaign` prête à être lancée.

Le parcours est terminé.

---

## Parcours alternatifs

### Configuration interrompue

Le membre quitte la `Campaign` avant d'avoir terminé sa configuration.

Les modifications déjà enregistrées sont conservées.

Le membre pourra reprendre la configuration ultérieurement.

---

### Configuration incomplète

Le membre laisse volontairement certains éléments non configurés.

La `Campaign` demeure en statut `Draft`.

SignalLab continue d'indiquer les prérequis restant à satisfaire avant le lancement.

---

## Cas d'échec

### Permissions insuffisantes

Le membre ne possède pas les permissions nécessaires pour modifier cette `Campaign`.

SignalLab refuse les modifications.

---

### `Campaign` indisponible

La `Campaign` n'existe plus ou n'est plus accessible.

SignalLab informe le membre que la configuration ne peut pas être poursuivie.

---

### La `Campaign` n'est plus en statut `Draft`

La `Campaign` a changé de statut pendant la session de configuration.

SignalLab empêche les modifications qui ne sont plus autorisées et informe le membre des restrictions liées au nouveau statut.

---

### Erreur technique

Une erreur empêche l'enregistrement d'une modification.

SignalLab informe le membre de l'échec et les modifications concernées ne sont pas enregistrées.

---

## Résultat

La `Campaign` possède une configuration mise à jour.

SignalLab indique en permanence les prérequis satisfaits et ceux restant à compléter avant que la `Campaign` puisse être lancée.

Le membre peut décider de poursuivre sa configuration ou d'engager le processus de lancement.

---

## Règles métier

- Une `Campaign` ne peut être configurée que lorsqu'elle possède le statut `Draft`.
- La configuration d'une `Campaign` est un processus progressif pouvant être interrompu et repris à tout moment.
- Les modifications sont enregistrées au fur et à mesure de leur réalisation.
- Les prérequis dde lancement sont évalués dynamiquement après chaque modification.
- Une `Campaign` peut rester durablement en statut `Draft`, même si sa configuration est incomplète.
- La satisfaction de tous les prérequis n'entraîne jamais le lancement automatique de la `Campaign`.
- Le lancement d'une `Campaign` constitue un User Journey distinct.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `Project`
- `Campaign`
- `CampaignStatus`

---

# User Journey 07 — Lancer une `Campaign`

## Objectif

Permettre à un membre autorisé d'un `Project` de lancer une `Campaign` une fois sa configuration terminée, soit immédiatement, soit à une date planifiée.

---

## Acteurs

- Membre du `Project` disposant des permissions nécessaires.

---

## Préconditions

- Le membre est authentifié dans SignalLab.
- Le membre appartient au `Project`.
- Le membre dispose des permissions nécessaires pour lancer une `Campaign`.
- La `Campaign` existe et possède le statut `Draft`.

---

## Déclencheur

Le membre sélectionne l'action **Lancer la `Campaign`** depuis la page de la `Campaign`.

---

## Parcours principal

Le membre demande le lancement de la `Campaign`.

SignalLab vérifie que tous les prérequis nécessaires au lancement sont satisfaits.

Le membre choisit le mode de lancement :

- Lancement immédiat.
- Lancement planifié.

### Lancement immédiat

SignalLab place immédiatement la `Campaign` dans le statut `Active`.

Les participants peuvent commencer leur participation conformément à la configuration de la `Campaign`.

Le parcours est terminé.

### Lancement planifié

Le membre définit la date et l'heure de lancement de la `Campaign`.

SignalLab place la `Campaign` dans le statut `Scheduled`.

À partir de ce moment :

- la configuration critique de la `Campaign` est verrouillée ;
- le recrutement des participants est autorisé ;
- les participants peuvent être invités ou rejoindre la `Campaign` selon sa configuration ;
- les participants ne peuvent pas encore commencer leur participation.

À la date programmée, SignalLab effectue une dernière vérification technique afin de garantir que les prérequis de lancement sont toujours satisfaits.

Si cette vérification est concluante, la `Campaign` passe automatiquement au statut `Active`.

Le parcours est terminé.

---

## Parcours alternatifs

### Prérequis non satisfaits

Au moins un prérequis de lancement n'est pas satisfait.

SignalLab refuse le lancement et présente au membre la liste complète des éléments bloquants.

La `Campaign` demeure en statut `Draft`.

---

### Annulation de la planification

Le membre souhaite modifier un élément critique de la `Campaign` alors que celle-ci est en statut `Scheduled`.

SignalLab informe le membre que cette modification nécessite l'annulation de la planification.

Après confirmation du membre :

- la `Campaign` repasse en statut `Draft` ;
- les éléments critiques redeviennent modifiables ;
- les participants déjà recrutés sont informés du report de la `Campaign`.

---

## Cas d'échec

### Permissions insuffisantes

Le membre ne possède pas les permissions nécessaires pour lancer cette `Campaign`.

SignalLab refuse l'action.

---

### La `Campaign` n'est plus en statut `Draft`

La `Campaign` a déjà été lancée ou son statut a changé.

SignalLab informe le membre que le lancement n'est plus possible.

---

### Erreur technique

Une erreur empêche le lancement de la `Campaign`.

SignalLab informe le membre de l'échec et le statut de la `Campaign` demeure inchangé.

---

## Résultat

La `Campaign` est :

- soit immédiatement placée dans le statut `Active` ;
- soit placée dans le statut `Scheduled` jusqu'à sa date de lancement.

Une `Campaign` planifiée passe automatiquement au statut `Active` lorsque la date programmée est atteinte.

---

## Règles métier

- Une `Campaign` ne peut être lancée que depuis le statut `Draft`.
- Tous les prérequis de lancement doivent être satisfaits avant le passage au statut `Scheduled` ou `Active`.
- Le lancement d'une `Campaign` est toujours une action explicite du membre.
- Une `Campaign` peut être lancée immédiatement ou planifiée.
- Le statut `Scheduled` représente une `Campaign` validée dont le lancement est programmé à une date ultérieure.
- Pendant le statut `Scheduled`, le recrutement des participants est autorisé.
- Pendant le statut `Scheduled`, les éléments critiques de la configuration sont verrouillés.
- Toute modification d'un élément critique nécessite l'annulation explicite de la planification.
- L'annulation de la planification replace la `Campaign` dans le statut `Draft`.
- Les participants déjà recrutés sont informés lorsqu'une planification est annulée.
- Une `Campaign` ne peut jamais revenir du statut `Active` au statut `Draft`.
- Le passage automatique du statut `Scheduled` au statut `Active` est précédé d'une dernière vérification technique des prérequis afin de détecter d'éventuelles situations exceptionnelles.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `Project`
- `Campaign`
- `CampaignStatus`
- `CampaignParticipation`

---

# User Journey 08 — Participer à une `Campaign`

## Objectif

Permettre à un `User` de participer à une `Campaign` à laquelle il est éligible.

---

## Acteurs

- `User`

---

## Préconditions

- Le `User` est authentifié dans SignalLab.
- Le `User` satisfait aux conditions d'accès définies par la `Campaign`.
- La `Campaign` est ouverte aux nouveaux participants.

---

## Déclencheur

Le `User` ouvre une invitation ou un lien lui permettant d'accéder à une `Campaign`.

---

## Parcours principal

Le `User` accède à la page de la `Campaign`.

SignalLab vérifie que le `User` est autorisé à participer.

SignalLab présente les informations de la `Campaign`.

Le `User` décide de participer.

Le `User` accepte les éventuelles conditions de participation et donne son consentement lorsque celui-ci est requis.

> Note de conception : `CampaignParticipationSettings.RequireExplicitConsent` détermine si le consentement est requis. Lorsqu'il l'est, la `CampaignConsentDefinition` figée constitue les conditions présentées et un `ConsentSnapshot` immuable est créé lors de l'acceptation. Aucun ConsentSnapshot n'est créé lorsque le consentement explicite n'est pas requis.

SignalLab enregistre la participation du `User` à la `Campaign`.

Si la `Campaign` est en statut `Scheduled`, SignalLab informe le `User` que sa participation est enregistrée et que la `Campaign` débutera à la date prévue.

Lorsque la `Campaign` devient disponible, le `User` réalise les activités définies par la `Campaign`.

Une fois celles-ci terminées, le `User` soumet sa participation.

SignalLab enregistre les résultats de la participation.

Le parcours est terminé.

---

## Parcours alternatifs

### Le `User` participe déjà à cette `Campaign`

SignalLab ouvre directement la participation existante.

---

### Le `User` renonce à participer

Le `User` décide de ne pas poursuivre la participation.

Aucune participation n'est enregistrée.

Le parcours est terminé.

---

### La `Campaign` n'accepte plus de nouveaux participants

SignalLab informe le `User` que les inscriptions sont closes.

Aucune participation n'est enregistrée.

---

## Cas d'échec

### Conditions d'accès non satisfaites

Le `User` ne satisfait pas aux conditions définies par la `Campaign`.

SignalLab refuse l'accès à la participation.

---

### La `Campaign` n'est plus disponible

La `Campaign` n'est plus accessible.

SignalLab informe le `User`.

---

### Erreur technique

Une erreur empêche l'enregistrement de la participation ou des résultats.

SignalLab informe le `User`.

---

## Résultat

Le `User` a participé à la `Campaign`.

Sa participation et les résultats associés sont enregistrés dans SignalLab.

---

## Règles métier

- Seuls les `Users` authentifiés peuvent participer à une `Campaign`.
- Dans le MVP, un `User` ne possède qu'une seule `CampaignParticipation` par `Campaign`. Les participations répétées appartiennent à une évolution future explicitement configurée.
- Une `Campaign` doit être ouverte aux nouveaux participants pour accepter une participation.
- Une participation est enregistrée uniquement après validation des conditions de participation et, lorsque nécessaire, du consentement du `User`.
- Une `Campaign` en statut `Scheduled` peut accepter de nouveaux participants, mais ceux-ci ne peuvent commencer leur participation qu'à partir de son passage en statut `Active`.
- Les résultats d'une participation sont enregistrés lorsque le `User` soumet sa participation.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `Project`
- `Campaign`
- `CampaignParticipation`
- `CampaignStatus`

---

# User Journey 09 — Consulter les résultats d'une `Campaign`

## Objectif

Permettre à un membre autorisé d'une `Organization` de consulter et d'analyser les résultats d'une `Campaign`.

---

## Acteurs

- Membre de l'`Organization` disposant des permissions nécessaires

---

## Préconditions

- Le membre est authentifié dans SignalLab.
- Le membre appartient à l'`Organization`.
- Le membre dispose des permissions nécessaires pour consulter les résultats de la `Campaign`.
- La `Campaign` existe.

---

## Déclencheur

Le membre ouvre la page de la `Campaign` et accède à la section des résultats.

---

## Parcours principal

Le membre accède aux résultats de la `Campaign`.

SignalLab affiche les informations disponibles, notamment :

- la progression de la `Campaign` ;
- les statistiques de participation ;
- les résultats collectés ;
- les données produites par les participants.

Le membre consulte et analyse les résultats de la `Campaign`.

Le membre peut consulter les résultats globaux ou explorer les données d'une participation individuelle.

Le parcours est terminé.

---

## Parcours alternatifs

### Aucun résultat disponible

Aucune donnée n'a encore été collectée.

SignalLab affiche un état vide et informe le membre qu'aucun résultat n'est disponible.

---

### La `Campaign` est toujours en cours

La `Campaign` est en statut `Active`.

SignalLab présente les résultats actuellement disponibles.

Les nouvelles données deviennent progressivement consultables à mesure que les participants soumettent leur participation.

---

## Cas d'échec

### Permissions insuffisantes

Le membre ne possède pas les permissions nécessaires pour consulter les résultats de cette `Campaign`.

SignalLab refuse l'accès.

---

### La `Campaign` n'est plus disponible

La `Campaign` n'est plus accessible.

SignalLab informe le membre.

---

### Erreur technique

Une erreur empêche l'affichage des résultats.

SignalLab informe le membre.

---

## Résultat

Le membre a consulté les résultats disponibles de la `Campaign`.

---

## Règles métier

- Les résultats d'une `Campaign` sont consultables dès que des données sont disponibles.
- Les résultats d'une `Campaign` continuent d'évoluer tant que celle-ci est en statut `Active`.
- Les résultats présentés reflètent l'état actuel de la `Campaign`.
- Un membre autorisé peut consulter les résultats globaux ainsi que les données d'une participation individuelle.
- Une `Campaign` sans donnée disponible présente un état vide.
- Les modalités d'actualisation des résultats relèvent des spécifications techniques et n'impactent pas le comportement métier.

---

## Concepts métier impliqués

- `Organization`
- `OrganizationMember`
- `Project`
- `Campaign`
- `CampaignParticipation`
- `CampaignStatus`

---

# User Journey 10 — Enregistrer et associer un `Build`

## Objectif

Permettre à un membre autorisé de référencer une version testable dans un `Project`, puis de l'associer à une `Campaign` en préparation.

## Acteurs

- `OrganizationMember` disposant des Permissions nécessaires

## Préconditions

- Le `Project` est `Active`.
- La `Campaign` éventuelle appartient au même `Project` et possède le statut `Draft`.

## Parcours principal

Le membre ouvre la tab **Builds** du `Project`.

Il crée un `Build` et renseigne son nom, sa version, sa plateforme, sa variante éventuelle et sa provenance.

Pour une provenance `External`, il fournit une URL valide.

Pour une provenance `Manual`, il décrit librement où la version peut être localisée ou comment l'identifier.

SignalLab crée le `Build` avec le statut `Active`.

Depuis la configuration d'une `Campaign Draft`, le membre sélectionne ce `Build`.

SignalLab vérifie qu'il appartient au même `Project` et qu'il est `Active`, puis met à jour la référence sans créer de copie.

## Parcours alternatifs

### Corriger une identité de Build

L'identité étant immuable, le membre crée un nouveau `Build` puis remplace la référence de la `Campaign Draft`.

### Archiver un Build

Le membre archive le `Build`. Il n'est plus sélectionnable par une nouvelle `Campaign`, mais reste visible depuis les Campaigns historiques.

## Résultat

La version testable est précisément identifiée et peut être figée lors de l'activation de la `Campaign`.

## Concepts métier impliqués

- `Project`
- `Build`
- `BuildStatus`
- `BuildProvenanceType`
- `Campaign`

---

# User Journey 11 — Créer une `Measure`

## Objectif

Permettre à une Organization de définir un concept analytique permanent et réutilisable dans ses questionnaires.

## Acteurs

- `OrganizationMember` disposant de la Permission de gérer les Measures

## Parcours principal

Le membre ouvre l'Explorer des `Measures` de l'`Organization`.

Il définit la clé canonique, le nom, la description, la catégorie et l'`OutcomeDirection` de la Measure.

SignalLab crée la `Measure` avec le statut `Active`.

La Measure devient disponible pour les `QuestionMeasureBindings` des FormTemplates de cette Organization.

## Parcours alternatifs

### Archiver une Measure

La Measure n'est plus associable à de nouvelles Questions, mais tous les bindings et résultats historiques restent interprétables.

### Redéfinir le concept

Si le concept analytique ou sa direction change, le membre crée une nouvelle Measure plutôt que de réécrire l'ancienne.

## Résultat

L'Organization possède un vocabulaire analytique stable dans le temps.

## Concepts métier impliqués

- `Organization`
- `Measure`
- `MeasureOutcomeDirection`
- `MeasureStatus`

---

# User Journey 12 — Construire et publier un `FormTemplate`

## Objectif

Créer un questionnaire réutilisable et une version publiée immuable.

## Acteurs

- `OrganizationMember` disposant des Permissions nécessaires

## Parcours principal

Le membre crée un `FormTemplate` dans l'Organization.

Il ajoute des Sections puis des Questions parmi les types pris en charge.

Pour chaque Question, il configure les validations et zéro, une ou plusieurs relations vers des Measures.

Chaque `QuestionMeasureBinding` précise explicitement :

- `Scored` ou `Contextual` ;
- `Aligned` ou `Inverted` lorsqu'il est scoré ;
- un poids strictement positif.

SignalLab ne déduit aucun de ces choix depuis le texte.

Le membre publie le FormTemplate.

SignalLab produit une `FormTemplateVersion` immuable contenant la structure et tous les bindings.

## Cas d'échec

La publication est refusée si un QuestionType, une validation ou un binding est incohérent.

## Résultat

Une version exacte et historiquement traçable du questionnaire peut être utilisée par les Campaigns.

## Concepts métier impliqués

- `FormTemplate`
- `FormTemplateVersion`
- `Section`
- `Question`
- `QuestionType`
- `QuestionMeasureBinding`
- `Measure`

---

# User Journey 13 — Générer et adapter un `CampaignForm`

## Objectif

Créer le questionnaire propre à une Campaign à partir d'une version publiée, puis l'adapter avant activation.

## Acteurs

- `OrganizationMember` disposant des Permissions nécessaires

## Préconditions

- La `Campaign` est `Draft`.
- Une `FormTemplateVersion` publiée existe dans la même Organization.

## Parcours principal

Depuis la tab **Form** de la Campaign, le membre sélectionne une FormTemplateVersion.

SignalLab copie intégralement les Sections, Questions, configurations et bindings dans un nouveau `CampaignForm` indépendant.

Le membre peut adapter cette copie, notamment modifier les Questions et leurs `CampaignFormQuestionMeasureBindings`.

Ces modifications ne changent jamais le FormTemplate ou sa version source.

Lors de l'activation, SignalLab valide puis fige définitivement le CampaignForm.

## Résultat

La Campaign possède le questionnaire historique exact qui sera présenté aux Participants et interprété par les Analytics.

## Concepts métier impliqués

- `Campaign`
- `CampaignForm`
- `CampaignFormQuestion`
- `CampaignFormQuestionMeasureBinding`
- `FormTemplateVersion`

---

# User Journey 14 — Ouvrir une `Analysis`

## Objectif

Explorer les données depuis le contexte courant sans quitter définitivement le `Project`, la `Campaign` ou les `Results` consultés.

## Acteurs

- `OrganizationMember` disposant de la Permission `ViewAnalytics`

## Déclencheur

Le membre utilise le bouton flottant **Analytics** depuis un `Project`, une `Campaign` ou ses `Results`.

## Parcours principal

SignalLab ouvre l'`Analysis Workspace` comme une surcouche au-dessus du contexte courant.

Le `AnalysisScope` est prérempli :

- avec la Campaign courante depuis Campaign ou Results ;
- avec une sélection initiale adaptée depuis Project.

Le membre peut ajouter ou retirer toute Campaign appartenant au même Project.

Il choisit des Questions ou Measures, définit des groupes et filtres, puis explore les statistiques et visualisations compatibles.

Il peut ouvrir les Responses et Submissions sources depuis tout résultat.

La fermeture de la surcouche restaure le contexte sous-jacent.

## Résultat

Une Analysis temporaire et recalculable existe sans créer de nouvelle ressource tant qu'elle n'est pas sauvegardée.

## Concepts métier impliqués

- `Analysis`
- `AnalysisScope`
- `AnalysisGroup`
- `Analytics`
- `Campaign`
- `Submission`
- `Response`

---

# User Journey 15 — Sauvegarder et rouvrir une `SavedAnalysis`

## Objectif

Conserver la configuration d'une exploration et la retrouver depuis le `Project`.

## Parcours principal

Depuis l'Analysis Workspace, le membre nomme et sauvegarde son Analysis.

SignalLab crée une `SavedAnalysis` appartenant au Project.

Elle conserve le Scope, les sujets, groupes, filtres, métriques et visualisations, sans stocker les résultats calculés.

La SavedAnalysis apparaît dans la tab **Analyses** du Project Explorer.

Lorsqu'elle est rouverte, SignalLab affiche la même surcouche et recalcule les résultats à partir des données et profils actuels.

## Résultat

La configuration analytique est durable, tandis que les preuves restent connectées à leurs sources vivantes.

## Concepts métier impliqués

- `SavedAnalysis`
- `AnalysisScope`
- `AnalyticalEngineVersion`
- `Project`

---

# User Journey 16 — Créer et partager son `MAPProfile`

## Objectif

Permettre à un User de calculer son profil motivationnel personnel et de contrôler son utilisation dans la recherche.

## Acteurs

- `User`

## Parcours principal

Le User ouvre son espace MAP depuis le Personal Hub.

SignalLab crée ou reprend un `MAPAssessment Draft` associé à la `MAPModelVersion` active.

Le User répond aux 34 items obligatoires puis soumet l'assessment.

SignalLab valide les réponses, calcule atomiquement les neuf dimensions et remplace son `MAPProfile` actuel.

Le profil reste `Private` par défaut.

Le User peut activer `SharedForResearch` ou revenir à `Private` à tout moment.

## Résultat

Le User possède un profil actuel, multidimensionnel, sans typologie exclusive et dont il contrôle le partage.

## Concepts métier impliqués

- `MAPModelVersion`
- `MAPAssessment`
- `MAPProfile`
- `MAPProfileSharing`

---

# User Journey 17 — Segmenter une `Analysis` avec MAP

## Objectif

Comparer l'expérience mesurée selon les motivations actuelles des Participants sans exposer leurs profils individuellement.

## Acteurs

- `OrganizationMember` disposant de `ViewAnalytics`

## Préconditions

- L'AnalysisScope contient des Submissions Submitted.
- Certains Users possèdent un MAPProfile partagé et compatible.

## Parcours principal

Le membre ajoute un filtre ou un groupe fondé sur une dimension MAP et une plage continue de scores.

SignalLab résout au moment du calcul les MAPProfiles actuels partagés et compatibles.

L'Analysis affiche les populations utilisables et indisponibles, puis les comparaisons de Measures, distributions, heatmaps, scatter plots ou tailles d'effet compatibles.

SignalLab ne révèle jamais si un profil individuel est absent, privé ou incompatible.

## Résultat

MAP enrichit l'interprétation des Responses sans remplacer les données de Campaign et sans créer de snapshot historique du profil.

## Concepts métier impliqués

- `MAPProfile`
- `MAPModelVersion`
- `AnalysisGroup`
- `SavedAnalysis`
- `Measure`