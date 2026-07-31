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
- Une `Campaign` en statut `Draft` peut être supprimée.
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