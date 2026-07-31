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