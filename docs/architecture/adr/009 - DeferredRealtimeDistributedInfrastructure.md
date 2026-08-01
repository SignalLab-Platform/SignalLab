# ADR 009 — Infrastructure temps réel et distribuée différée

**Status :** Accepted
**Date :** 2026-08-01
**Decision Owners :** SignalLab Architecture
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# Context

SignalLab possède une vision long terme comprenant des capacités collaboratives et potentiellement temps réel :

* notifications immédiates ;
* présence collaborative ;
* invalidation entre plusieurs clients actifs ;
* ResearchBoards partagés ;
* collaboration synchrone ;
* calculs analytiques longs ;
* exports ;
* traitements planifiés ;
* montée en charge horizontale.

Ces capacités pourraient à terme justifier des technologies comme :

```text
SignalR
Redis
Hangfire
Message Broker
Event Bus distribué
Microservices
Background Workers
Stockage objet
```

Le MVP se concentre cependant sur :

* la collecte ;
* la traçabilité historique ;
* les Campaigns ;
* les Submissions et Responses ;
* les Results ;
* les Analyses recalculables ;
* le moteur analytique déterministe ;
* la structure multi-tenant ;
* les parcours synchrones principaux.

Aucun besoin actuellement validé ne nécessite :

* une connexion persistante ;
* une communication interservices ;
* une transaction distribuée ;
* une cohérence éventuelle ;
* un cache distribué ;
* un traitement persistant survivant à un redémarrage ;
* un déploiement indépendant par domaine.

Introduire ces capacités pendant le MVP augmenterait la complexité de développement, de test, de sécurité, d’observabilité et d’exploitation sans valeur immédiate démontrée.

---

# Decision

SignalLab diffère l’introduction de l’infrastructure temps réel et distribuée.

Le MVP n’installe pas et n’exploite pas :

```text
SignalR
Redis
Hangfire
Message Broker
Event Bus distribué
Microservices
Transactions distribuées
Background Workers spécialisés
Stockage objet
```

Les parcours MVP reposent sur :

```text
HTTP
REST
JSON
TanStack Query
PostgreSQL
Requêtes synchrones bornées et annulables
Rafraîchissements explicites ou automatiques raisonnables
```

Le Backend reste un Modular Monolith stateless.

Les nouvelles technologies ne sont introduites qu’après identification d’un besoin réel, définition de critères mesurables et validation d’une Spec et d’une ADR dédiées.

---

# Realtime Strategy

SignalR constitue la technologie privilégiée si une future capacité nécessite réellement une connexion persistante avec le Backend ASP.NET Core.

Cas potentiels :

* notification immédiate ;
* présence collaborative ;
* édition partagée ;
* ResearchBoard partagé ;
* état modifié simultanément par plusieurs clients ;
* invalidation immédiate d’une projection ouverte.

SignalR ne sera pas utilisé comme source de vérité.

Le modèle futur attendu est :

```text
Modification métier
↓
Transaction PostgreSQL validée
↓
Événement SignalR informant d’un changement
↓
TanStack Query invalide la Query concernée
↓
REST retourne la projection canonique
```

SignalR ne remplacera pas :

* REST ;
* PostgreSQL ;
* TanStack Query ;
* les autorisations Backend ;
* les cas d’usage Application ;
* les contrats HTTP.

Les payloads temps réel devront rester minimaux.

Ils signaleront prioritairement qu’une ressource a changé plutôt que de transporter une seconde représentation complète susceptible de diverger.

---

# Realtime Authorization

Toute future connexion SignalR sera authentifiée côté Backend.

L’accès aux groupes devra vérifier :

* le `User` courant ;
* son Membership ;
* ses Permissions ;
* l’Organization concernée ;
* le Project ou ResearchBoard concerné ;
* l’état courant de la ressource.

Exemples de groupes futurs :

```text
Organization:{organizationId}
Project:{projectId}
ResearchBoard:{researchBoardId}
```

La connaissance d’un identifiant de groupe ne suffira jamais à rejoindre ce groupe.

Une Permission retirée devra empêcher les futurs accès et être prise en compte lors des reconnexions.

---

# Reconnection and Consistency

Une connexion temps réel peut être interrompue.

Le système ne doit donc jamais dépendre d’un événement SignalR unique pour garantir un état métier.

Après une reconnexion :

1. le client rétablit son identité ;
2. le Backend réévalue ses autorisations ;
3. le client invalide les Queries concernées ;
4. l’état canonique est rechargé par REST.

Un événement manqué ne doit pas produire une corruption durable de l’état Frontend.

---

# Redis

Redis n’est pas introduit dans le MVP.

Il pourrait être évalué pour :

* Backplane SignalR lors d’une montée en charge multi-instance ;
* cache distribué mesuré ;
* Rate Limiting distribué ;
* coordination temporaire entre instances ;
* données éphémères à forte fréquence.

Redis ne sera pas utilisé comme source de vérité métier.

PostgreSQL restera la source persistante canonique.

L’ajout de Redis devra définir :

* les données cachées ;
* leur durée de vie ;
* leur invalidation ;
* leur comportement en cas d’indisponibilité ;
* la protection des données sensibles ;
* le gain de performance mesuré.

Un cache ne doit pas être ajouté pour masquer une requête PostgreSQL mal conçue sans analyse préalable.

---

# Background Jobs

Le MVP exécute les traitements analytiques dans la requête HTTP tant qu’ils respectent des budgets mesurés.

Chaque calcul doit être :

* déterministe ;
* borné ;
* annulable ;
* journalisé ;
* limité par Scope ;
* limité par cardinalité ;
* compatible avec les budgets de réponse définis.

Un système de Background Jobs pourra être introduit si le produit nécessite :

* une Analysis dépassant durablement les budgets synchrones ;
* un export long ;
* un traitement planifié ;
* un calcul devant survivre à un redémarrage ;
* un recalcul massif ;
* un effet externe nécessitant des retries persistants.

La nécessité doit être démontrée par des mesures ou par un besoin fonctionnel explicite.

---

# Persistent Job Model

Si un système de jobs est introduit ultérieurement, un Job persistant devra posséder un état explicite.

Exemple conceptuel :

```text
Pending
Running
Succeeded
Failed
Cancelled
```

Il devra notamment définir :

* propriétaire ;
* Organization ;
* type d’opération ;
* paramètres ;
* progression lorsque disponible ;
* date de création ;
* date de démarrage ;
* date de fin ;
* erreur sûre ;
* politique de retry ;
* politique de rétention ;
* annulation.

L’outil technique ne devra pas devenir le Domain Model.

Les concepts métier nécessaires devront être modélisés indépendamment de Hangfire ou d’un autre fournisseur.

---

# Analytical Jobs

L’introduction future de jobs analytiques ne modifiera pas la nature des Analyses.

Une `SavedAnalysis` restera une configuration.

Le résultat analytique restera une projection recalculable.

```text
SavedAnalysis Configuration
+
Persisted Research Data
+
AnalyticalEngineVersion
↓
Calculated Projection
```

Un Job pourra orchestrer le calcul, mais son résultat ne deviendra pas automatiquement une nouvelle source de vérité.

Toute stratégie de cache ou de matérialisation devra préserver :

* la version du moteur ;
* les données sources ;
* la configuration ;
* la traçabilité ;
* la possibilité de recalcul.

---

# Message Broker and Event Bus

Aucun Message Broker ni Event Bus distribué n’est utilisé dans le MVP.

Les modules du Modular Monolith communiquent dans le même processus.

Les effets internes peuvent être coordonnés explicitement par Application.

Des Domain Events internes peuvent être ajoutés lorsque leur valeur métier est démontrée, sans imposer une infrastructure distribuée.

Un Event Bus distribué pourra être envisagé si :

* un service indépendant est réellement extrait ;
* plusieurs consommateurs autonomes existent ;
* une livraison asynchrone persistante est nécessaire ;
* la cohérence éventuelle est acceptée ;
* l’idempotence et les retries sont définis.

Son introduction nécessitera de documenter :

* garanties de livraison ;
* ordre ;
* duplication ;
* idempotence ;
* Dead Letter Queue ;
* schéma des messages ;
* versioning ;
* observabilité ;
* sécurité.

---

# Microservices

SignalLab ne découpe pas son Backend en Microservices pendant le MVP.

Une extraction ne pourra être envisagée qu’en présence de critères concrets tels que :

* charge indépendante clairement mesurée ;
* cycle de déploiement indépendant ;
* ownership par équipe distincte ;
* frontière métier stable ;
* exigences de sécurité ou d’isolation spécifiques ;
* besoin de résilience indépendante ;
* valeur supérieure au coût opérationnel.

Une simple taille croissante du repository ne constitue pas un motif suffisant.

Avant extraction, le module devra déjà posséder :

* une responsabilité claire ;
* des contrats explicites ;
* une propriété de données identifiable ;
* peu de dépendances transversales ;
* des tests ;
* une observabilité suffisante.

---

# Distributed Transactions

Le MVP n’utilise pas de transaction distribuée.

Les opérations nécessitant une atomicité restent dans :

```text
Un processus Backend
+
Une base PostgreSQL
+
Une transaction locale
```

Si des services externes ou Microservices sont introduits, la cohérence devra être repensée explicitement.

Des stratégies futures pourraient inclure :

* Outbox Pattern ;
* idempotence ;
* Saga ;
* compensation ;
* retries persistants.

Aucune de ces stratégies n’est introduite avant l’existence d’une opération qui la nécessite.

---

# File and Object Storage

Le MVP ne stocke aucun artefact binaire de Build.

Le `Build` reste une référence métier.

Il ne représente pas :

* un fichier hébergé ;
* un repository ;
* un commit ;
* un pipeline ;
* une distribution ;
* une exécution.

Un stockage objet pourra être introduit si un concept distinct apparaît, par exemple :

```text
BuildArtifact
Attachment
Export
GeneratedReport
```

Cette évolution devra définir :

* ownership ;
* autorisations ;
* durée de conservation ;
* antivirus ;
* taille maximale ;
* chiffrement ;
* URL signées ;
* suppression ;
* audit ;
* coût.

Le stockage objet restera distinct de PostgreSQL et du `Build` métier.

---

# Stateless Backend

Le Backend doit rester stateless entre les requêtes.

L’état métier durable est stocké dans PostgreSQL.

Le processus peut conserver des objets temporaires liés à une requête, mais ne doit pas supposer que :

* la prochaine requête arrive sur la même instance ;
* une donnée en mémoire survivra à un redémarrage ;
* une instance unique existera toujours.

Ce principe prépare une montée en charge future sans imposer immédiatement plusieurs instances.

---

# Observability Prerequisites

Une infrastructure distribuée ne sera pas introduite sans observabilité suffisante.

Avant l’ajout d’une capacité distribuée, le système devra pouvoir suivre :

* identifiant de corrélation ;
* requête initiale ;
* Job ou message ;
* service producteur ;
* service consommateur ;
* retries ;
* latence ;
* erreurs ;
* résultat final.

Les logs structurés et traces OpenTelemetry constituent les fondations prévues.

---

# Failure Strategy

Chaque future dépendance distribuée devra définir son comportement en cas d’échec.

Questions obligatoires :

* L’opération métier est-elle déjà commitée ?
* Peut-elle être rejouée ?
* Est-elle idempotente ?
* Combien de retries sont autorisés ?
* Où l’échec définitif est-il enregistré ?
* Que voit l’utilisateur ?
* Comment l’opération est-elle annulée ?
* Comment une incohérence est-elle réparée ?
* Quelles données sensibles peuvent être journalisées ?

Une technologie ne doit pas être ajoutée sans réponse explicite à ces questions.

---

# Consequences

## Positive

### Architecture MVP plus simple

Le système conserve peu de composants opérationnels.

### Développement accéléré

Le développeur peut se concentrer sur les domaines et parcours principaux.

### Transactions simples

Les opérations métier restent protégées par PostgreSQL sans coordination distribuée.

### Débogage facilité

Les requêtes, erreurs et logs appartiennent principalement à un seul processus.

### Coût d’hébergement réduit

Aucun service Redis, Broker ou Worker spécialisé n’est requis initialement.

### Décisions guidées par les mesures

Les technologies futures répondent à des problèmes observés plutôt qu’à des hypothèses.

### Évolution conservée

Le Backend stateless, les contrats REST et les modules explicites permettent une évolution ultérieure.

## Negative

### Absence de mise à jour instantanée

Les clients utilisent des rafraîchissements TanStack Query plutôt que des événements immédiats.

### Limite des requêtes synchrones

Les calculs analytiques doivent rester dans des budgets compatibles avec HTTP.

### Pas de traitement résilient long

Une requête interrompue ne dispose pas encore d’un Job persistant permettant de reprendre son traitement.

### Déploiement Backend commun

Tous les modules sont déployés ensemble.

### Scalabilité non spécialisée

Une charge spécifique ne peut pas encore être isolée dans un service dédié.

Ces limitations sont acceptées pendant le MVP.

---

# Alternatives Considered

## Installer SignalR dès la fondation

### Rejected

Cette option préparerait les notifications futures.

Elle est rejetée car aucune Feature MVP ne nécessite actuellement une connexion persistante.

Elle imposerait prématurément :

* sécurité des Hubs ;
* groupes ;
* reconnexion ;
* tests ;
* observabilité ;
* éventuel Backplane.

## Ajouter Redis immédiatement

### Rejected

Cette option préparerait le cache et la montée en charge.

Elle est rejetée faute de besoin de cache distribué mesuré et parce qu’elle ajouterait une nouvelle source d’indisponibilité.

## Ajouter Hangfire immédiatement

### Rejected

Cette option préparerait les calculs longs.

Elle est rejetée tant que les budgets analytiques n’ont pas démontré la nécessité de jobs persistants.

## Utiliser un Event Bus interne dès le départ

### Rejected

Cette option réduirait le couplage apparent.

Elle est rejetée car elle pourrait masquer les dépendances réelles et rendre l’exécution plus difficile à suivre sans besoin concret.

## Découper immédiatement en Microservices

### Rejected

Cette option renforcerait l’isolation technique et le déploiement indépendant.

Elle est rejetée en raison du coût opérationnel, des transactions distribuées et de l’absence de critères d’extraction.

## Stocker les artefacts de Build

### Rejected for MVP

SignalLab identifie ce qui est testé sans devenir une plateforme de stockage ou distribution de Builds.

---

# Implementation Rules

* Ne pas installer SignalR pendant le MVP sans Spec dédiée.
* Utiliser REST et TanStack Query pour les parcours actuels.
* Conserver PostgreSQL comme source de vérité.
* Conserver le Backend stateless.
* Ne pas installer Redis sans besoin mesuré.
* Ne pas installer Hangfire ou un autre Job Runner sans besoin fonctionnel ou budget dépassé.
* Ne pas introduire de Message Broker ou Event Bus distribué.
* Ne pas créer de Microservices sans critères d’extraction explicites.
* Ne pas utiliser de transaction distribuée.
* Ne pas stocker d’artefact binaire de Build.
* Conserver les calculs synchrones bornés et annulables.
* Faire notifier les changements futurs par SignalR puis recharger l’état canonique par REST.
* Exiger une ADR et une Milestone dédiées pour toute capacité distribuée.
* Ajouter l’observabilité, la sécurité et la gestion des échecs avec la capacité concernée.

---

# Supersession Criteria

Cette ADR devra être complétée ou remplacée lorsqu’une capacité temps réel ou distribuée est approuvée.

La nouvelle ADR devra indiquer :

* le besoin précis ;
* les mesures disponibles ;
* la technologie choisie ;
* les alternatives ;
* les responsabilités ;
* les contrats ;
* la sécurité ;
* l’observabilité ;
* le modèle d’échec ;
* le coût opérationnel ;
* les limites.

---

# References

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `002 - FrontendConventions.md`
* `003 - APIConventions.md`
* `004 - PersistenceConventions.md`
* `005 - TestingStrategy.md`
* `06 - SoftwareArchitecture.md`
* `00 - ProductFoundation.md`
* `02 - DomainModel.md`
* `M - Milestones.md`
* `001 - ModularMonolith.md`
* `007 - TanStackQueryServerState.md`
