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