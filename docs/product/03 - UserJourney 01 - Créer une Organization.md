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
- `OrganizationRole`