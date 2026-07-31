# M11 — Deployment and Operations

**Version :** 2.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M10 — MAP Profile  
**Milestone suivante :** M12 — Documentation and Portfolio

---

> Cette Milestone prépare la version fonctionnelle complète de SignalLab à une utilisation réelle.
>
> Elle valide son déploiement, son exploitation, sa sécurité opérationnelle et sa capacité de restauration.
>
> Elle n'introduit aucune nouvelle fonctionnalité métier.

---

# Objectif

Permettre à SignalLab d'être :

- déployé ;
- configuré ;
- exploité ;
- supervisé ;
- sauvegardé ;
- restauré ;
- mis à jour de manière reproductible.

---

# Valeur produit

À la fin de cette Milestone, SignalLab peut être utilisé dans un environnement de production par des Organizations et Participants réels.

---

# Hors scope

Cette Milestone ne contient pas :

- nouvelles fonctionnalités métier ;
- évolution du Domain Model ;
- nouvelles interfaces produit ;
- hébergement de Builds ;
- stockage d'artefacts binaires ;
- distribution ou exécution de Builds.

---

# Principes

- Les environnements sont reproductibles.
- Les secrets sont externalisés.
- PostgreSQL reste la source de vérité persistée.
- Les métadonnées des Builds sont stockées comme les autres données métier.
- Aucun artefact de Build n'est pris en charge dans le MVP.
- Les déploiements sont automatisés et réversibles.
- Les sauvegardes et restaurations sont testées.
- L'observabilité ne doit jamais exposer de données sensibles.

---

# Scénario d'acceptation

Une version validée est fusionnée.

↓

La CI exécute les contrôles.

↓

L'application est construite.

↓

La version est déployée en production.

↓

Les migrations sont appliquées de manière contrôlée.

↓

Le Frontend et le Backend sont accessibles.

↓

Les données existantes sont conservées.

↓

Une restauration testée permet de récupérer la plateforme en cas d'incident.

---

# Specs

---

## SL-112 — Préparer les environnements finaux

### Description

Finaliser les configurations Development, Staging et Production pour l'application complète.

Cette Spec prolonge les conventions introduites dans M01 sans les redéfinir.

### Objectif produit

Garantir un comportement cohérent jusqu'à la production.

### Objectif technique

Valider les configurations, variables d'environnement, URLs et dépendances de chaque environnement.

### Critères d'acceptation

Les trois environnements sont définis et documentés.

Aucun secret n'est présent dans le repository.

Les configurations nécessaires sont injectées correctement.

### Definition of Done

Les environnements finaux sont opérationnels.

---

## SL-113 — Déployer l'infrastructure de production

### Description

Déployer les composants nécessaires au fonctionnement du MVP :

- Frontend ;
- API ASP.NET Core ;
- PostgreSQL ;
- services techniques strictement nécessaires ;
- terminaison HTTPS.

### Objectif produit

Rendre la plateforme complète accessible.

### Objectif technique

Créer et documenter l'infrastructure de production.

### Critères d'acceptation

Tous les composants requis sont déployés.

Le Frontend communique avec l'API.

L'API communique avec PostgreSQL.

Les communications publiques utilisent HTTPS.

### Definition of Done

L'infrastructure de production est opérationnelle.

---

## SL-114 — Valider la persistance des métadonnées des Builds

### Description

Valider la création, la conservation et la récupération en production des métadonnées décrivant les Builds.

Aucun fichier ou artefact binaire n'est stocké par SignalLab dans le MVP.

### Objectif produit

Garantir que les références des Builds restent exploitables dans les Campaigns déployées.

### Objectif technique

Vérifier leur persistance PostgreSQL, leur exposition API et leur conservation entre les déploiements.

### Critères d'acceptation

Les métadonnées d'un Build peuvent être créées et récupérées en production.

Les Campaigns conservent une référence valide vers leur Build.

Les sauvegardes incluent ces métadonnées.

Aucun stockage binaire n'est provisionné pour les Builds.

### Definition of Done

La persistance des métadonnées des Builds est validée.

---

## SL-115 — Sécuriser l'exploitation

### Description

Configurer les mécanismes opérationnels nécessaires à la confidentialité, l'intégrité et la disponibilité de la plateforme.

### Cette Spec comprend notamment

- gestion des secrets ;
- HTTPS et certificats ;
- configuration sécurisée ;
- sauvegardes PostgreSQL ;
- politique de restauration ;
- limitation des accès administratifs ;
- mises à jour des dépendances critiques.

### Objectif produit

Protéger les données des Organizations et des Participants.

### Critères d'acceptation

Les secrets sont externalisés.

Les sauvegardes sont automatisées.

Une restauration est documentée.

Les accès administratifs sont limités.

### Definition of Done

L'exploitation respecte les exigences de sécurité du MVP.

---

## SL-116 — Superviser la plateforme

### Description

Mettre en place l'observabilité nécessaire à l'exploitation de SignalLab.

### Cette Spec comprend notamment

- logs structurés ;
- corrélation ;
- traces ;
- métriques essentielles ;
- health checks ;
- alertes techniques critiques.

### Objectif produit

Détecter et diagnostiquer rapidement les incidents.

### Objectif technique

Configurer l'observabilité prévue par l'architecture sans introduire de plateforme disproportionnée.

### Critères d'acceptation

L'état du Frontend, de l'API et de PostgreSQL est observable.

Les erreurs critiques peuvent être retrouvées grâce à un identifiant de corrélation.

Aucune donnée sensible n'est enregistrée dans les logs.

### Definition of Done

La supervision est opérationnelle.

---

## SL-117 — Automatiser les déploiements finaux

### Description

Étendre la CI/CD de M01 afin de déployer l'application complète de manière contrôlée.

### Cette Spec comprend notamment

- build Backend ;
- build Frontend ;
- tests ;
- lint ;
- migrations contrôlées ;
- déploiement Staging ;
- déploiement Production ;
- stratégie de retour arrière.

### Objectif produit

Réduire les risques liés aux mises en production.

### Critères d'acceptation

Une version validée peut être déployée automatiquement.

Les contrôles bloquants s'exécutent avant le déploiement.

Un échec ne laisse pas la plateforme dans un état incohérent.

Une procédure de retour arrière est disponible.

### Definition of Done

La chaîne de déploiement finale est opérationnelle.

---

## SL-118 — Tester sauvegarde, restauration et mise à jour

### Description

Valider les opérations critiques permettant de maintenir la plateforme dans le temps.

### Objectif produit

Garantir que les données peuvent être conservées et récupérées après un incident ou une mise à jour.

### Objectif technique

Tester sur un environnement représentatif :

- installation complète ;
- migration de version ;
- sauvegarde ;
- restauration ;
- retour arrière compatible.

### Critères d'acceptation

Une installation complète réussit.

Une mise à jour conserve les données.

Une sauvegarde peut être restaurée.

Le comportement après restauration est vérifié.

### Definition of Done

Les procédures opérationnelles critiques sont validées.

---

## SL-119 — Valider le MVP en production

### Description

Exécuter le scénario fonctionnel principal du MVP dans l'environnement de production.

### Parcours validé

Organization

↓

Project

↓

Build metadata

↓

Measure

↓

Form Template

↓

Campaign

↓

Campaign Form

↓

Participation

↓

Submission

↓

Results

↓

Analysis

↓

MAP filtering

### Objectif produit

Garantir que la chaîne complète fonctionne dans les conditions réelles de déploiement.

### Critères d'acceptation

Le parcours principal est exécutable de bout en bout.

Le Frontend et le Backend sont accessibles.

Les données persistent correctement.

Les contrôles d'autorisation restent effectifs.

### Definition of Done

Le MVP est validé en production.

---

## SL-120 — Finaliser Deployment and Operations

### Description

Stabiliser l'ensemble de la Milestone.

Cette Spec comprend :

- corrections ;
- revue de sécurité ;
- revue des coûts et ressources ;
- documentation opérationnelle ;
- validation finale ;
- suppression des éléments inutilisés.

### Objectif produit

Disposer d'une plateforme réellement exploitable.

### Critères d'acceptation

Toutes les Specs précédentes sont validées.

Les procédures sont documentées.

Les tests de production sont concluants.

Aucune infrastructure de Build hors scope n'est déployée.

### Definition of Done

Milestone validée.

---

# Fin de Milestone

À la fin de M11, SignalLab peut être :

- déployé ;
- exploité ;
- supervisé ;
- sauvegardé ;
- restauré ;
- mis à jour de manière sécurisée.

Le parcours fonctionnel complet du MVP est validé en production sans stockage ni hébergement d'artefacts de Build.