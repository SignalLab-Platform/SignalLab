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
- `CampaignParticipant`