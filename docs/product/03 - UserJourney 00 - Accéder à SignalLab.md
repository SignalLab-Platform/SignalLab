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
- `CampaignParticipant`