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
- `CampaignParticipant`
- `CampaignStatus`