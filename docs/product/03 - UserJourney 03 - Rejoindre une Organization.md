# User Journey 03 — Rejoindre une `Organization`

## Objectif

Permettre à un `User` disposant d'un compte SignalLab de rejoindre une `Organization` existante à la suite d'une `Invitation` nominative.

---

## Acteurs

- `User` invité
- Membre de l'`Organization` disposant des permissions nécessaires pour inviter de nouveaux membres

---

## Préconditions

- Le `User` possède déjà un compte SignalLab.
- Une `Invitation` nominative valide a été créée pour ce `User`.
- L'`Invitation` précise l'`Organization` concernée, le rôle attribué et sa date d'expiration.

---

## Déclencheur

Le `User` ouvre l'`Invitation` depuis le centre de notifications ou via le lien unique associé à cette `Invitation`.

---

## Parcours principal

Le `User` ouvre une `Invitation` à rejoindre une `Organization`.

Si le `User` n'est pas authentifié, SignalLab lui demande de s'authentifier avant de poursuivre.

Une fois authentifié, SignalLab le redirige automatiquement vers l'`Invitation` initialement ouverte.

SignalLab vérifie que l'`Invitation` est valide et qu'elle est bien destinée au `User` authentifié.

SignalLab présente les informations essentielles de l'`Organization`, ainsi que le rôle qui sera attribué au `User`.

Le `User` accepte l'`Invitation`.

SignalLab crée son appartenance à l'`Organization` avec le rôle défini dans l'`Invitation`.

L'`Invitation` est alors marquée comme acceptée et ne nécessite plus aucune action.

L'`Organization` apparaît dans la liste des `Organizations` accessibles depuis la page d'accueil.

SignalLab ouvre ensuite la page de l'`Organization`.

Le parcours est terminé.

---

## Parcours alternatifs

### Invitation refusée

Le `User` refuse l'`Invitation`.

SignalLab marque l'`Invitation` comme refusée et la retire des actions en attente.

Le `User` ne rejoint pas l'`Organization`.

---

### Invitation annulée

L'`Invitation` a été annulée avant son acceptation.

SignalLab informe le `User` que cette `Invitation` n'est plus valide.

---

### Invitation expirée

Le `User` ouvre une `Invitation` dont la durée de validité est dépassée.

SignalLab informe le `User` que l'`Invitation` a expiré et qu'une nouvelle devra être créée.

---

### Le `User` est déjà membre

Le `User` appartient déjà à cette `Organization`.

SignalLab l'en informe et lui permet d'accéder directement à celle-ci.

---

## Cas d'échec

### Compte authentifié incorrect

Le lien est ouvert par un `User` différent de celui auquel l'`Invitation` est destinée.

SignalLab refuse l'accès à l'`Invitation` et informe le `User` que celle-ci ne lui est pas destinée.

---

### Erreur technique

Une erreur empêche l'ajout du `User` à l'`Organization`.

SignalLab informe le `User` de l'échec sans invalider l'`Invitation`, afin qu'il puisse réessayer ultérieurement.

---

## Résultat

Le `User` devient membre de l'`Organization` avec le rôle défini dans l'`Invitation`.

L'`Organization` est désormais accessible depuis sa page d'accueil et le `User` est redirigé vers sa page.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `OrganizationRole`
- `Invitation`