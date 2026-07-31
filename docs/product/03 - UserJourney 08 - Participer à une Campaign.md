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

> Note de conception : certaines `Campaigns` pourront nécessiter un consentement explicite (RGPD, confidentialité, NDA, enregistrement, etc.). Cette capacité sera détaillée dans les spécifications fonctionnelles et n'est pas modélisée dans le Domain Model actuel.

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
- Un `User` ne peut participer qu'une seule fois à une même `Campaign`, sauf si celle-ci autorise explicitement plusieurs participations.
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
- `CampaignParticipant`
- `CampaignStatus`