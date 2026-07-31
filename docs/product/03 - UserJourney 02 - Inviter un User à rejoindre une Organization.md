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

SignalLab lui demande de définir les paramètres de l'`Invitation` :

- Rôle attribué au `User`.
- Durée de validité de l'`Invitation` :
  - 24 heures
  - 7 jours *(par défaut)*
  - 30 jours

Le membre valide la création de l'`Invitation`.

SignalLab crée une `Invitation` nominative liée :

- au `User` destinataire ;
- à l'`Organization` ;
- au rôle attribué ;
- au membre ayant créé l'`Invitation`.

Le destinataire reçoit une notification dans SignalLab.

L'`Invitation` apparaît dans la liste des invitations en attente de l'`Organization`.

Le membre peut copier le lien unique associé à cette `Invitation` afin de le transmettre au destinataire par le canal de son choix.

Le parcours est terminé.

---

## Parcours alternatifs

### Aucun `User` trouvé

La recherche ne retourne aucun compte.

SignalLab informe le membre que le destinataire doit disposer d'un compte SignalLab avant de pouvoir être invité.

---

### Invitation déjà en attente

Une `Invitation` valide existe déjà pour ce `User` et cette `Organization`.

SignalLab empêche la création d'une nouvelle `Invitation` et permet de consulter ou copier le lien de l'`Invitation` existante.

---

### Le `User` est déjà membre

Le `User` appartient déjà à l'`Organization`.

SignalLab l'indique au membre et aucune `Invitation` n'est créée.

---

### Création annulée

Le membre abandonne la création avant sa validation.

Aucune `Invitation` n'est créée.

---

### Invitation annulée

Le membre annule une `Invitation` en attente.

L'`Invitation` devient immédiatement inutilisable et disparaît des actions en attente du destinataire.

---

## Cas d'échec

### Permissions insuffisantes

Le membre ne possède pas les permissions nécessaires pour inviter de nouveaux membres.

SignalLab refuse l'action.

---

### Erreur technique

Une erreur empêche la création de l'`Invitation`.

SignalLab informe le membre de l'échec et aucune `Invitation` n'est créée.

---

## Résultat

Une `Invitation` nominative est créée et reste valide jusqu'à son acceptation, son refus, son expiration ou son annulation.

---

## Concepts métier impliqués

- `User`
- `Organization`
- `OrganizationMember`
- `OrganizationRole`
- `Invitation`