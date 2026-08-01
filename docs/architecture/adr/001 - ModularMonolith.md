# ADR 001 — Modular Monolith

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# Context

SignalLab doit couvrir plusieurs capacités métier fortement liées :

```text
Identity
Organizations
Projects
Builds
Measures
Forms
Campaigns
Participation
Results
Analytics
MAP
```

Ces capacités partagent :

* une nomenclature métier commune ;
* des invariants transversaux ;
* une hiérarchie Organization → Project → ressources ;
* des transactions pouvant traverser plusieurs concepts ;
* une même équipe de développement ;
* un même cycle de livraison ;
* une même base de données relationnelle ;
* une charge initiale inconnue mais compatible avec un déploiement simple.

Le MVP doit rester réalisable par un développeur unique dans un délai contraint.

L’architecture doit néanmoins maintenir des frontières suffisamment claires pour empêcher la création d’un monolithe désorganisé.

Les Microservices ajouteraient immédiatement :

* communication réseau interne ;
* déploiements multiples ;
* découverte et configuration de services ;
* authentification interservices ;
* observabilité distribuée ;
* gestion de messages ;
* cohérence éventuelle ;
* transactions distribuées ou compensations ;
* complexité opérationnelle.

Aucun besoin fonctionnel ou de charge ne justifie actuellement ce coût.

---

# Decision

SignalLab adopte un **Modular Monolith**.

Le Backend possède :

* un seul processus ASP.NET Core ;
* une seule solution .NET ;
* un seul déploiement Backend ;
* une seule base PostgreSQL ;
* plusieurs modules métier séparés logiquement ;
* aucune communication réseau entre modules internes.

Les modules sont organisés dans les projets Clean Architecture existants :

```text
SignalLab.Domain
SignalLab.Application
SignalLab.Infrastructure
SignalLab.Api
```

Chaque projet organise ses types par capacité métier.

Exemple cible :

```text
SignalLab.Domain/
├── Organizations/
├── Projects/
├── Campaigns/
└── ...

SignalLab.Application/
├── Organizations/
├── Projects/
├── Campaigns/
└── ...
```

SignalLab ne crée pas un projet .NET distinct par module tant qu’un besoin réel d’isolation d’assembly n’est pas démontré.

Les frontières modulaires reposent initialement sur :

* les dossiers ;
* les namespaces ;
* la propriété explicite des types ;
* les cas d’usage Application ;
* les interfaces entre couches ;
* les conventions documentées ;
* les tests d’architecture.

Un module ne doit pas modifier directement les données internes d’un autre module en contournant ses invariants et ses cas d’usage.

---

# Consequences

## Positive

### Déploiement simple

Le Backend reste une seule application à construire, configurer, tester et déployer.

### Transactions relationnelles simples

Les opérations nécessitant une cohérence atomique peuvent utiliser une transaction PostgreSQL locale.

### Développement rapide

Les fonctionnalités peuvent être développées sans infrastructure réseau interne ni contrats interservices.

### Refactoring transversal possible

Les frontières peuvent évoluer pendant le MVP sans migration de protocoles réseau ou coordination de déploiements distribués.

### Observabilité centralisée

Les logs, erreurs et traces appartiennent à un seul processus Backend.

### Coût opérationnel limité

Aucun orchestrateur ou système de messagerie distribué n’est nécessaire pour le MVP.

### Extraction future possible

Une capacité devenue suffisamment autonome peut être extraite ultérieurement si :

* ses frontières sont stables ;
* sa charge le justifie ;
* son cycle de déploiement devient indépendant ;
* son extraction apporte une valeur mesurable.

## Negative

### Frontières non imposées par le réseau

Le code peut techniquement contourner les modules si les conventions ne sont pas respectées.

Cette faiblesse doit être limitée par :

* la documentation ;
* les revues ;
* les tests d’architecture ;
* des namespaces cohérents ;
* la propriété claire des données.

### Déploiement commun

Une modification Backend entraîne le déploiement de l’application complète.

### Ressources partagées

Une opération coûteuse peut affecter le même processus que les autres capacités.

Les calculs analytiques devront donc être :

* annulables ;
* bornés ;
* mesurés ;
* optimisés selon des budgets explicites.

### Base partagée

Les modules partagent le même schéma PostgreSQL.

Les accès directs non contrôlés doivent être évités afin de préserver la propriété logique des données.

---

# Alternatives Considered

## Microservices immédiats

### Rejected

Cette option fournirait une isolation technique forte et des déploiements indépendants.

Elle est rejetée pour le MVP en raison de :

* sa complexité ;
* son coût opérationnel ;
* l’absence d’équipe distribuée ;
* l’absence de besoins de scalabilité indépendants démontrés ;
* la difficulté accrue des transactions métier ;
* la vitesse de développement réduite.

## Monolithe organisé uniquement par couches techniques

### Rejected

Exemple :

```text
Controllers
Services
Repositories
Models
```

Cette organisation réduirait la visibilité des capacités métier et favoriserait les services transversaux trop larges.

SignalLab combine les couches Clean Architecture avec une organisation modulaire et des Vertical Slices.

## Projet .NET par module

### Deferred

Cette option renforcerait l’isolation par assembly.

Elle est différée car elle multiplierait les projets et références avant que les frontières réelles des modules aient été éprouvées.

Elle pourra être reconsidérée si :

* les dépendances deviennent difficiles à contrôler ;
* plusieurs équipes possèdent des modules distincts ;
* certains modules nécessitent un cycle de compilation indépendant ;
* les tests d’architecture par namespace deviennent insuffisants.

## Modular Monolith avec base distincte par module

### Rejected for MVP

Cette option renforcerait la propriété des données mais compliquerait les transactions et migrations.

Une base PostgreSQL unique est plus adaptée au MVP.

---

# Implementation Rules

* Un seul Backend ASP.NET Core.
* Une seule base PostgreSQL.
* Aucun appel HTTP interne entre modules.
* Aucun Event Bus distribué.
* Aucun module créé avant sa Spec propriétaire.
* Aucun projet .NET par module sans nouvelle décision.
* Les modules utilisent les noms canoniques du Domain Model.
* Les dépendances entre modules restent explicites.
* Les transactions transversales restent locales au monolithe.
* Une extraction en service séparé nécessite une nouvelle ADR.

---

# References

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `004 - PersistenceConventions.md`
* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `M - Milestones.md`
* `009 - DeferredRealtimeDistributedInfrastructure.md`
