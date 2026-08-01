# Index des Architecture Decision Records

**Projet :** SignalLab
**Document :** Index des Architecture Decision Records
**Version :** 1.0
**Statut :** Validé
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# 1. Objectif

Ce document référence les Architecture Decision Records de SignalLab.

Une ADR conserve une décision architecturale structurante ainsi que :

* son contexte ;
* la décision retenue ;
* ses conséquences ;
* les alternatives considérées ;
* son statut ;
* ses références.

Les ADR expliquent pourquoi une décision a été prise.

Les documents opérationnels de `docs/architecture` décrivent comment cette décision doit être appliquée dans le repository.

---

# 2. Relation avec la documentation opérationnelle

```text
SoftwareArchitecture
    ↓
Architecture Decision Records
    ↓
Documentation opérationnelle
    ↓
Code et tests d’architecture
```

Les ADR conservent le contexte historique.

Les documents opérationnels représentent les règles actuellement applicables.

Une ADR n’est pas réécrite pour masquer une ancienne décision.

Lorsqu’une décision change :

1. une nouvelle ADR est créée ;
2. l’ancienne ADR passe au statut `Superseded` ;
3. les deux ADR se référencent ;
4. la documentation opérationnelle est mise à jour ;
5. les tests d’architecture sont adaptés.

---

# 3. Statuts

## Proposed

La décision est proposée mais pas encore validée.

Elle ne constitue pas une règle applicable.

## Accepted

La décision est validée et applicable.

## Accepted — Not Implemented

La décision est validée comme architecture cible, mais son implémentation appartient à une Spec ultérieure.

## Deprecated

La décision reste historiquement documentée mais ne doit plus être utilisée pour les nouveaux développements.

## Superseded

La décision a été remplacée par une autre ADR.

L’ADR de remplacement doit être indiquée explicitement.

## Rejected

La proposition a été examinée puis refusée.

Elle peut être conservée lorsqu’elle documente une alternative importante susceptible de réapparaître.

---

# 4. Structure d’une ADR

Chaque ADR contient au minimum :

```text
Status
Context
Decision
Consequences
Alternatives Considered
References
```

Le contenu explicatif est rédigé en français.

Les noms officiels de technologies et les termes techniques canoniques restent en anglais.

---

# 5. Nommage

Convention des fichiers :

```text
000 - PascalCase.md
```

Règles :

* préfixe numérique sur trois chiffres ;
* espace, tiret, espace ;
* nom anglais en PascalCase ;
* nom court ;
* groupe nominal privilégié ;
* absence d’articles et prépositions inutiles ;
* aucune renumérotation d’une ADR publiée.

Exemples :

```text
001 - ModularMonolith.md
002 - CleanArchitectureVerticalSlices.md
010 - MinimalAPIs.md
```

---

# 6. Registre des ADR

|  ID | Fichier                                              | Décision                                              | Statut                     |
| --: | ---------------------------------------------------- | ----------------------------------------------------- | -------------------------- |
| 001 | `001 - ModularMonolith.md`                           | Adopter un Modular Monolith                           | Accepted                   |
| 002 | `002 - CleanArchitectureVerticalSlices.md`           | Combiner Clean Architecture et Vertical Slices        | Accepted                   |
| 003 | `003 - NextJsFrontend.md`                            | Utiliser Next.js App Router pour le Frontend          | Accepted                   |
| 004 | `004 - AspNetCoreBackend.md`                         | Utiliser ASP.NET Core pour le Backend                 | Accepted                   |
| 005 | `005 - RestJsonOpenAPI.md`                           | Utiliser REST, JSON et OpenAPI                        | Accepted                   |
| 006 | `006 - PostgreSqlEntityFrameworkCore.md`             | Utiliser PostgreSQL et Entity Framework Core          | Accepted — Not Implemented |
| 007 | `007 - TanStackQueryServerState.md`                  | Utiliser TanStack Query pour le Server State          | Accepted                   |
| 008 | `008 - ClerkAuthentication.md`                       | Déléguer l’authentification à Clerk                   | Accepted — Not Implemented |
| 009 | `009 - DeferredRealtimeDistributedInfrastructure.md` | Différer le temps réel et l’infrastructure distribuée | Accepted                   |
| 010 | `010 - MinimalAPIs.md`                               | Utiliser les Minimal APIs pour la Presentation HTTP   | Accepted                   |

---

# 7. Propriété des décisions

Une ADR structurante possède une Spec propriétaire.

Pour les ADR initiales :

```text
SL-013 — Figer l’architecture de la solution
```

L’implémentation d’une décision peut appartenir à une autre Spec.

Exemples :

```text
PostgreSQL
→ décision documentée par SL-013
→ implémentation appartenant à SL-016

Entity Framework Core
→ décision documentée par SL-013
→ implémentation appartenant à SL-017

Clerk
→ décision documentée par SL-013
→ implémentation appartenant à la Milestone Identity
```

Une ADR au statut `Accepted — Not Implemented` n’autorise pas une implémentation anticipée hors de sa Spec propriétaire.

---

# 8. Création d’une nouvelle ADR

Une nouvelle ADR est requise lorsqu’une décision :

* modifie durablement la structure de la solution ;
* introduit une technologie structurante ;
* change la direction des dépendances ;
* modifie le style de communication entre systèmes ;
* introduit une infrastructure distribuée ;
* remplace une convention fondamentale ;
* affecte plusieurs modules ou Milestones ;
* nécessite de conserver les alternatives et compromis étudiés.

Une ADR n’est généralement pas nécessaire pour :

* une petite décision locale ;
* un détail d’implémentation réversible ;
* une convention déjà couverte par un document opérationnel ;
* une décision propre à une seule méthode ou un seul composant.

---

# 9. Validation

Une ADR passe au statut `Accepted` lorsque :

* son contexte est explicite ;
* la décision est formulée sans ambiguïté ;
* les conséquences sont reconnues ;
* les alternatives importantes sont documentées ;
* la décision est cohérente avec les documents produit et techniques ;
* sa Spec propriétaire est validée.

Lorsque la décision est vérifiable automatiquement, un test d’architecture doit être ajouté ou mis à jour.

---

# 10. Références

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `002 - FrontendConventions.md`
* `003 - APIConventions.md`
* `004 - PersistenceConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `M - Milestones.md`
