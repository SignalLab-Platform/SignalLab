# ADR 008 — Clerk Authentication

**Status :** Accepted — Not Implemented
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire de la décision :** SL-013 — Figer l’architecture de la solution
**Spec propriétaire de l’implémentation :** SL-023 — Implémenter l’authentification

---

# Context

L’authentification constitue un domaine technique sensible.

Elle implique notamment :

* création de compte ;
* connexion ;
* gestion sécurisée des mots de passe ;
* récupération de compte ;
* gestion de session ;
* émission et renouvellement de tokens ;
* protection contre des attaques courantes ;
* intégration avec Next.js ;
* validation des tokens dans ASP.NET Core.

Développer et maintenir un système complet d’authentification interne augmenterait fortement :

* le risque de sécurité ;
* le temps de développement ;
* le coût de maintenance ;
* la responsabilité opérationnelle.

SignalLab possède néanmoins son propre modèle métier de `User`, de Membership, de Role et de Permission.

Le fournisseur d’identité ne doit pas remplacer ces concepts.

---

# Decision

SignalLab délègue l’authentification à **Clerk**.

Clerk est responsable de :

* création des comptes ;
* connexion ;
* déconnexion ;
* gestion sécurisée des credentials ;
* récupération de compte ;
* gestion de session ;
* émission des tokens d’identité.

Le Frontend utilise le SDK Clerk pour les parcours d’authentification.

Le Backend ASP.NET Core valide les JWT avant toute opération protégée.

SignalLab ne persiste aucun `PasswordHash`.

---

# Standards

L’architecture privilégie les standards suivants :

```text
OAuth 2.0
OpenID Connect
JWT
```

Clerk constitue une implémentation de ces standards.

L’architecture doit limiter le couplage au fournisseur afin de conserver une possibilité de migration future.

---

# Authentication and Authorization

L’authentification répond à :

> Qui est l’utilisateur ?

L’autorisation répond à :

> Que peut-il faire ?

Clerk fournit l’identité authentifiée.

SignalLab contrôle l’autorisation métier dans le Backend.

Clerk ne devient pas propriétaire :

* des Roles SignalLab ;
* du catalogue de Permissions ;
* des Memberships ;
* de l’isolation des Organizations ;
* des règles métier.

---

# SignalLab User

Le compte Clerk ne remplace pas le `User` métier SignalLab.

SignalLab conserve un modèle propre permettant notamment de gérer :

* profil ;
* Organizations ;
* Memberships ;
* Roles ;
* Permissions ;
* préférences ;
* fuseau ;
* langue ;
* relations historiques.

Le lien entre Clerk et SignalLab utilise l’identifiant externe fourni par Clerk.

Exemple conceptuel :

```text
Clerk User ID
↓
ExternalIdentityId
↓
SignalLab User
```

Aucun credential Clerk n’est persisté dans le Domain.

---

# Backend Validation

Pour une route protégée, ASP.NET Core doit :

1. recevoir le JWT ;
2. vérifier sa signature ;
3. vérifier son émetteur ;
4. vérifier son audience lorsque configurée ;
5. vérifier son expiration ;
6. extraire l’identité externe ;
7. résoudre le `User` SignalLab ;
8. exécuter les contrôles de Permission et de multi-tenancy.

Un token valide prouve une identité.

Il ne prouve pas l’autorisation d’accéder à une ressource métier.

---

# Frontend Responsibilities

Le Frontend peut :

* afficher les parcours de connexion ;
* récupérer la session ;
* transmettre le token selon l’intégration officielle ;
* masquer certaines actions pour améliorer l’UX ;
* afficher l’identité courante.

Le Frontend ne garantit pas :

* une Permission ;
* l’accès à une Organization ;
* l’accès à un Project ;
* la validité métier d’une opération.

Les contrôles restent systématiquement exécutés côté Backend.

---

# Multi-Tenancy

Chaque `Organization` constitue un espace isolé.

La connaissance d’un identifiant ne suffit jamais à obtenir un accès.

Après authentification, chaque cas d’usage protégé vérifie :

* `User` courant ;
* Membership ;
* état du Membership ;
* Role ou Permissions ;
* appartenance de la ressource à l’Organization.

Un token Clerk valide ne contourne aucune de ces vérifications.

---

# Account Lifecycle

Clerk gère le cycle technique du compte d’identité.

SignalLab gère le cycle métier du `User`.

Ces cycles ne doivent pas être confondus.

Une suspension, suppression ou modification de compte Clerk peut nécessiter une synchronisation avec SignalLab.

La stratégie exacte de synchronisation sera définie dans SL-023 et SL-024.

Aucune règle de synchronisation supplémentaire n’est imposée par SL-013 au-delà de la séparation des responsabilités.

---

# Secrets and Configuration

Les clés Clerk sont fournies par configuration d’environnement.

Aucun secret réel n’est commité.

Les environnements cibles sont :

```text
Development
Staging
Production
```

La validation des variables au démarrage appartient à SL-018.

Les clés publiques et secrètes doivent rester séparées selon leur usage Frontend ou Backend.

---

# Consequences

## Positive

### Réduction du risque

SignalLab ne développe pas directement la gestion sécurisée des mots de passe et sessions.

### Développement accéléré

Les parcours standards d’authentification utilisent un fournisseur spécialisé.

### Intégration Next.js

Clerk fournit une intégration adaptée au framework Frontend retenu.

### Tokens standards

Le Backend peut valider les JWT sans déléguer l’autorisation métier.

### Séparation claire

L’identité externe et le `User` métier conservent des responsabilités distinctes.

### Migration possible

L’usage de standards et d’un identifiant externe limite le couplage du Domain.

## Negative

### Dépendance fournisseur

Les parcours d’authentification et certaines configurations dépendent de Clerk.

### Coût externe

Le coût peut évoluer avec le nombre d’utilisateurs et les fonctionnalités utilisées.

### Disponibilité externe

Une indisponibilité du fournisseur peut empêcher de nouvelles authentifications ou certains renouvellements de session.

### Synchronisation nécessaire

SignalLab doit maintenir le lien entre l’identité Clerk et le `User` métier.

### Deux modèles d’identité

Les développeurs doivent distinguer clairement le compte externe du `User` SignalLab.

### Configuration multiple

Frontend et Backend nécessitent des configurations Clerk cohérentes selon l’environnement.

---

# Alternatives Considered

## Authentification interne ASP.NET Core Identity

### Rejected for MVP

Cette option fournirait un contrôle complet.

Elle est rejetée car elle imposerait à SignalLab de gérer directement :

* PasswordHash ;
* récupération de compte ;
* sécurité des sessions ;
* emails de validation ;
* protections contre les attaques ;
* maintenance continue.

## Auth0

### Not Selected

Auth0 offre des capacités similaires et repose sur les mêmes standards.

Clerk est retenu pour son intégration Frontend et la décision déjà validée dans la Software Architecture.

## Supabase Auth

### Not Selected

Supabase Auth pourrait fournir l’identité et d’autres services Backend.

Cette approche risquerait d’encourager un accès direct aux données et une architecture concurrente à l’API ASP.NET Core.

## Firebase Authentication

### Not Selected

Firebase Authentication fournit une solution mature.

Il n’est pas retenu car la stack globale et le modèle relationnel reposent sur ASP.NET Core et PostgreSQL.

## Keycloak

### Deferred

Keycloak permettrait un hébergement et un contrôle plus directs.

Il augmenterait toutefois la charge d’exploitation, de mise à jour et de sécurisation.

Il pourra être reconsidéré si l’indépendance fournisseur ou l’auto-hébergement deviennent prioritaires.

---

# Implementation Rules

* Utiliser Clerk pour l’authentification.
* Utiliser les standards OAuth 2.0, OpenID Connect et JWT.
* Utiliser le SDK Clerk dans Next.js.
* Valider les JWT dans ASP.NET Core.
* Ne jamais stocker de `PasswordHash` dans SignalLab.
* Conserver un `User` métier distinct.
* Lier les deux identités par un identifiant externe.
* Contrôler toutes les Permissions côté Backend.
* Vérifier l’isolation des Organizations côté Backend.
* Ne jamais considérer un token valide comme une autorisation métier.
* Ne pas manipuler manuellement les tokens dans les composants métier.
* Ne commiter aucun secret Clerk.
* Toute modification du fournisseur canonique nécessite une nouvelle ADR.

---

# References

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `002 - FrontendConventions.md`
* `003 - APIConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `M - Milestones.md`
* `003 - NextJsFrontend.md`
* `004 - AspNetCoreBackend.md`
