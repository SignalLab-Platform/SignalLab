# Technical Design

**Projet :** SignalLab  
**Version :** 1.0  
**Statut :** Validé  
**Auteur :** Jimmy Fromonot

---

# Table des matières

1. Introduction
2. Vision technique
3. Principes architecturaux
4. Vue d'ensemble du système
5. Architecture Frontend
6. Architecture Backend
7. Communication
8. Authentification et autorisation
9. Persistence
10. Temps réel
11. Observabilité
12. Gestion des erreurs
13. Sécurité
14. Tests
15. Documentation de l'API
16. Déploiement
17. Intégration continue
18. Performance et évolutivité
19. Traitements en arrière-plan
20. Technologies non retenues
21. Évolutions futures
22. Décisions techniques
23. Conclusion

---

# 1. Introduction

## 1.1 Objectif du document

Ce document décrit l'architecture technique de SignalLab ainsi que les décisions de conception retenues pour son développement.

Il constitue la référence technique du projet et a pour objectif de :

- décrire l'architecture globale de l'application ;
- justifier les choix technologiques ;
- définir les responsabilités des différentes couches du système ;
- servir de support aux futurs développements ;
- faciliter la maintenance et l'évolution du projet.

Ce document ne décrit pas les fonctionnalités métier en détail.

Les comportements fonctionnels, les parcours utilisateurs et les modèles métier sont documentés séparément au travers des documents de spécifications fonctionnelles, des User Journeys et du Domain Model.

---

## 1.2 Présentation de SignalLab

SignalLab est une plateforme SaaS de **Game User Research** destinée à transformer les données de playtests en connaissances comparables dans le temps.

Le produit structure notamment :

- les Organizations et Projects de recherche ;
- les Builds étudiés ;
- les Campaigns et leurs questionnaires historiques ;
- les CampaignParticipations, Submissions et Responses ;
- les Measures assurant la continuité analytique ;
- les SavedAnalysis et les segmentations MAP.

Le MVP met volontairement l'accent sur la collecte, la traçabilité et un moteur analytique déterministe puissant.

La collaboration temps réel, les ResearchBoards et l'organisation collective du raisonnement appartiennent à la vision long terme et ne sont pas implémentés sans Milestone dédiée.

---

## 1.3 Philosophie générale

L'objectif principal de SignalLab est de construire une base technique durable.

Chaque décision d'architecture répond à un problème concret identifié.

Aucune technologie n'est intégrée uniquement parce qu'elle est populaire ou largement utilisée.

Le projet privilégie :

- la simplicité ;
- la maintenabilité ;
- la séparation des responsabilités ;
- les standards ouverts ;
- la capacité d'évolution.

Les optimisations prématurées et les architectures inutilement complexes sont volontairement évitées.

---

# 2. Vision technique

SignalLab est conçu comme un Modular Monolith.

L'application est composée d'un frontend Next.js et d'une API ASP.NET Core partageant une architecture clairement séparée entre les responsabilités métier et les responsabilités techniques.

L'objectif est de permettre une évolution progressive du produit sans remettre en cause les fondations du système.

L'architecture doit permettre :

- l'ajout de nouvelles fonctionnalités avec un impact limité sur le reste du projet ;
- la séparation stricte entre les règles métier et les technologies utilisées ;
- l'intégration de fonctionnalités collaboratives en temps réel ;
- une maintenance facilitée ;
- une évolution vers une plateforme SaaS professionnelle.

Le projet ne cherche pas à anticiper prématurément des problématiques de très grande échelle.

Les composants distribués, les microservices ou les infrastructures complexes ne seront introduits que lorsqu'un besoin réel apparaîtra.

---

# 3. Principes architecturaux

## 3.1 Les besoins déterminent les technologies

Chaque technologie intégrée au projet répond à un besoin identifié.

Les choix techniques sont motivés par la résolution de problèmes concrets et non par leur popularité.

Une technologie qui ne répond pas à un besoin actuel n'est pas intégrée au MVP.

Cette philosophie permet de conserver une architecture simple tout en restant ouverte aux évolutions futures.

---

## 3.2 Séparation des responsabilités

Le projet applique le principe de séparation des responsabilités.

Chaque couche possède un rôle clairement identifié.

Les responsabilités principales sont réparties entre :

- la communication avec le monde extérieur ;
- l'orchestration des cas d'usage ;
- les règles métier ;
- les détails techniques.

Cette séparation réduit le couplage entre les composants et facilite leur évolution indépendante.

---

## 3.3 Le métier est indépendant des technologies

Les règles métier de SignalLab ne doivent jamais dépendre directement :

- d'ASP.NET Core ;
- de PostgreSQL ;
- d'Entity Framework Core ;
- de Clerk ;
- de SignalR ;
- de Next.js.

Le domaine représente les concepts métier propres à SignalLab.

Les technologies utilisées pour stocker, transmettre ou afficher ces concepts sont considérées comme des détails d'implémentation.

---

## 3.4 Les standards avant les fournisseurs

Le projet privilégie les standards largement adoptés.

Les principaux standards utilisés sont notamment :

- HTTP ;
- HTTPS ;
- REST ;
- JSON ;
- OAuth 2.0 ;
- OpenID Connect ;
- JWT ;
- SQL.

Les produits sélectionnés (Clerk, PostgreSQL, Entity Framework Core...) sont des implémentations répondant à ces standards.

L'objectif est de limiter les dépendances fortes envers un fournisseur particulier.

---

## 3.5 Simplicité et évolution progressive

Le MVP constitue une première étape.

Les choix techniques sont volontairement adaptés aux besoins actuels du projet.

Des technologies comme Redis, Hangfire ou une architecture distribuée pourront être introduites ultérieurement si leur valeur devient démontrée.

À l'inverse, elles ne sont pas intégrées par anticipation.

---

# 4. Vue d'ensemble du système

## 4.1 Architecture globale

Le système est composé des principaux éléments suivants :

- un frontend développé avec Next.js ;
- une API ASP.NET Core ;
- une base de données PostgreSQL ;
- un fournisseur d'identité Clerk.

Le frontend constitue l'interface utilisateur.

L'API constitue l'autorité métier du système.

La base de données représente la source de vérité des données.

Clerk assure uniquement l'authentification des utilisateurs.

---

## 4.2 Flux principal

Le déroulement classique d'une opération est le suivant :

Navigateur

↓

Next.js

↓

API REST ASP.NET Core

↓

Application Layer

↓

Domain

↓

Infrastructure

↓

Entity Framework Core

↓

PostgreSQL

Les projections analytiques suivent un flux de lecture complémentaire :

Analysis Request

↓

Application Query / Analytical Engine

↓

PostgreSQL + Domain Metadata

↓

Calculated Projection

Les capacités temps réel futures pourront ajouter SignalR lorsque les notifications ou ResearchBoards seront effectivement implémentés.

---

## 4.3 Source de vérité

Le backend constitue l'autorité de toutes les données métier.

Le frontend ne fait que représenter l'état du système.

Aucune règle métier importante ne doit dépendre exclusivement du frontend.

Toutes les validations importantes sont réalisées côté serveur.

---

# 5. Architecture générale

SignalLab adopte les principes de la Clean Architecture.

L'objectif est d'isoler les règles métier des détails techniques afin de permettre au système d'évoluer sans remettre en cause son cœur fonctionnel.

L'application est organisée autour de quatre responsabilités principales :

- Presentation
- Application
- Domain
- Infrastructure

Les dépendances suivent toujours la même direction :

Presentation
        ↓
Application
        ↓
Domain

L'Infrastructure fournit les implémentations techniques nécessaires au fonctionnement de ces couches, sans que celles-ci ne dépendent directement de ses technologies.

Cette organisation permet notamment de remplacer un framework, une base de données ou un fournisseur d'identité avec un impact limité sur les règles métier.

Les sections suivantes détaillent le rôle de chacune de ces couches.

---

# 6. Architecture Frontend

## 6.1 React

Le frontend est développé avec React afin de construire l'interface utilisateur sous forme de composants réutilisables.

Cette approche permet :

- une séparation claire des responsabilités de l'interface ;
- une forte réutilisabilité des composants ;
- une maintenance simplifiée ;
- une meilleure évolutivité de l'application.

Les composants React sont responsables uniquement de la représentation de l'état de l'application et des interactions utilisateur.

Ils ne contiennent pas les règles métier qui doivent être garanties par le serveur.

---

## 6.2 Next.js

Next.js constitue le framework principal du frontend.

Il apporte notamment :

- le système de routing ;
- les layouts ;
- l'organisation du projet ;
- les optimisations de rendu ;
- l'intégration avec Clerk ;
- la gestion des Server Components lorsque cela est pertinent.

SignalLab utilise Next.js comme framework d'application et non comme backend métier.

L'ensemble des règles métier reste implémenté dans l'API ASP.NET Core.

---

## 6.3 TypeScript

TypeScript est utilisé afin de sécuriser le développement du frontend.

Son utilisation permet :

- une meilleure documentation du code ;
- la détection précoce de nombreuses erreurs ;
- des refactorings plus sûrs ;
- une meilleure cohérence des échanges avec l'API.

Les modèles TypeScript représentent les contrats utilisés par le frontend.

Ils ne constituent pas le modèle métier du système.

---

## 6.4 Gestion du Server State

SignalLab utilise TanStack Query pour gérer le Server State.

Le Server State représente les données dont la source de vérité se situe sur le serveur.

TanStack Query prend notamment en charge :

- le cache des requêtes HTTP ;
- la déduplication des appels identiques ;
- la gestion de la fraîcheur des données ;
- l'invalidation après une mutation ;
- le refetch automatique lorsque cela est nécessaire.

Les données purement liées à l'interface (état d'une fenêtre, élément sélectionné, etc.) restent du Client State et ne relèvent pas de TanStack Query.

---

## 6.5 Tailwind CSS

Tailwind CSS est utilisé pour construire l'interface graphique.

Son approche utilitaire permet :

- une grande rapidité de développement ;
- une forte cohérence visuelle ;
- la réduction du CSS spécifique au projet ;
- une personnalisation simple du Design System.

Les composants réutilisables restent privilégiés lorsque plusieurs ensembles de classes Tailwind sont utilisés de manière répétée.

---

## 6.6 shadcn/ui

SignalLab utilise shadcn/ui comme base de composants d'interface.

Cette bibliothèque fournit notamment :

- Buttons ;
- Dialogs ;
- Inputs ;
- Selects ;
- Tooltips ;
- Tabs ;
- Popovers ;
- Command Palette.

Contrairement à une bibliothèque de composants classique, les composants sont intégrés directement au projet et peuvent être adaptés librement aux besoins de SignalLab.

shadcn/ui constitue une base technique et non l'identité graphique définitive du produit.

---

# 7. Architecture Backend

## 7.1 ASP.NET Core

Le backend est développé avec ASP.NET Core.

Il constitue le point d'entrée unique des opérations métier de SignalLab.

Il est responsable :

- de l'exposition des API REST ;
- de l'authentification des utilisateurs ;
- de l'autorisation des opérations ;
- de l'exécution des cas d'usage ;
- de la communication temps réel via SignalR ;
- de la journalisation ;
- de la gestion centralisée des erreurs.

Toutes les modifications de données transitent par cette API.

---

## 7.2 C#

Le backend est entièrement développé en C#.

Ce langage apporte :

- un typage statique robuste ;
- un excellent support de la programmation asynchrone ;
- une bonne intégration avec ASP.NET Core ;
- un écosystème mature ;
- des outils de développement performants.

Le modèle métier est entièrement exprimé au travers de classes et d'objets C#.

---

## 7.3 Clean Architecture

Le backend suit les principes de la Clean Architecture.

L'objectif principal est de séparer les règles métier des détails techniques.

Les dépendances doivent toujours pointer vers le cœur métier.

Cette organisation facilite :

- les tests ;
- les évolutions techniques ;
- la maintenance ;
- le remplacement de technologies.

---

## 7.4 Presentation Layer

La Presentation Layer constitue la frontière entre le système et le monde extérieur.

Elle contient notamment :

- les Controllers ;
- les Endpoints HTTP ;
- les Hubs SignalR ;
- les Middlewares ;
- la validation technique des requêtes ;
- la sérialisation des réponses.

Cette couche ne contient aucune règle métier.

Son rôle est uniquement de traduire une requête technique en intention métier.

---

## 7.5 Application Layer

L'Application Layer orchestre les cas d'usage.

Elle coordonne les différentes opérations nécessaires à l'exécution d'une fonctionnalité.

Par exemple :

- charger les données nécessaires ;
- vérifier les autorisations ;
- appeler le Domain ;
- demander la persistence ;
- notifier les clients ;
- retourner le résultat.

Elle ne définit pas les règles métier elles-mêmes.

Elle orchestre leur exécution.

Les principaux composants de cette couche sont :

- Application Services ;
- Commands ;
- Queries ;
- DTO ;
- Interfaces de Repository ;
- Interfaces des services externes.

---

## 7.6 Domain

Le Domain représente le cœur de SignalLab.

Il contient :

- les entités métier ;
- les Value Objects ;
- les comportements métier ;
- les invariants ;
- les Domain Events lorsque leur utilisation est justifiée.

Cette couche ne connaît pas :

- ASP.NET Core ;
- PostgreSQL ;
- Entity Framework Core ;
- Clerk ;
- SignalR ;
- React ;
- Next.js.

Le Domain exprime uniquement les règles propres à SignalLab.

Le Domain Model constitue la documentation de référence décrivant précisément les concepts métier et leurs relations.

---

## 7.7 Infrastructure

L'Infrastructure regroupe l'ensemble des détails techniques permettant au système de fonctionner.

Elle contient notamment :

- Entity Framework Core ;
- le DbContext ;
- les Repositories ;
- les Migrations ;
- les services techniques ;
- les intégrations avec les services externes.

Son rôle est de fournir les implémentations nécessaires aux interfaces définies par les couches internes.

Les règles métier ne doivent jamais dépendre directement de cette couche.

---

# 8. Communication

## 8.1 HTTP

Le frontend communique avec le backend exclusivement via HTTP sécurisé par HTTPS.

Toutes les opérations métier transitent par cette interface.

Le backend constitue l'autorité du système.

---

## 8.2 REST

SignalLab adopte une architecture REST.

Les ressources principales de l'application sont représentées par des endpoints dédiés.

Exemples :
http
GET /projects

GET /projects/{id}

POST /projects

PATCH /projects/{id}

DELETE /projects/{id}

Cette approche correspond naturellement au modèle métier de SignalLab fondé sur des ressources relativement stables.

---

## 8.3 JSON

Les échanges entre le frontend et le backend utilisent JSON.

Les contrats d'échange sont représentés par des DTO.

Les entités du Domain ne sont jamais exposées directement.

Cette séparation permet :

- de stabiliser les contrats ;
- d'éviter l'exposition accidentelle de données internes ;
- d'adapter les réponses aux besoins du client.

---

## 8.4 Validation

La validation est répartie selon deux responsabilités.

La validation technique concerne notamment :

- les champs obligatoires ;
- les formats ;
- les tailles maximales ;
- les types de données.

La validation métier appartient au Domain.

Par exemple :

- un utilisateur ne peut pas modifier un projet auquel il n'appartient pas ;
- un projet archivé ne peut plus accepter certaines opérations.

Cette séparation garantit que les règles métier restent indépendantes des technologies utilisées.

---

# 9. Authentification et autorisation

## 9.1 Principes

SignalLab ne développe pas son propre système d'authentification.

L'authentification est déléguée à un Identity Provider spécialisé afin de réduire les risques de sécurité et le coût de maintenance d'un domaine particulièrement sensible.

Le projet s'appuie sur les standards suivants :

- OAuth 2.0 ;
- OpenID Connect ;
- JWT.

Le fournisseur actuellement retenu est Clerk.

Le choix de Clerk répond principalement :

- à son excellente intégration avec Next.js ;
- à sa simplicité de mise en œuvre ;
- à une offre gratuite adaptée au MVP ;
- à son respect des standards d'authentification modernes.

L'architecture reste indépendante du fournisseur utilisé afin de permettre une éventuelle migration future si les besoins du projet évoluent.

---

## 9.2 Authentification

Clerk est responsable de :

- la création des comptes ;
- la connexion des utilisateurs ;
- la gestion sécurisée des mots de passe ;
- les méthodes de récupération de compte ;
- l'émission des tokens d'identité.

Le frontend utilise le SDK Clerk afin de gérer les parcours d'authentification.

Le backend valide les JWT avant toute opération protégée.

---

## 9.3 Utilisateur métier

L'utilisateur authentifié par Clerk ne remplace pas le modèle utilisateur de SignalLab.

SignalLab conserve son propre modèle métier permettant notamment de gérer :

- les Organizations ;
- les Memberships ;
- les rôles ;
- les permissions ;
- les préférences utilisateur.

Le lien entre les deux est assuré grâce à l'identifiant externe fourni par Clerk.

---

## 9.4 Autorisation

L'authentification répond à la question :

> Qui est l'utilisateur ?

L'autorisation répond à la question :

> Que peut-il faire ?

Toutes les autorisations sont contrôlées par le backend.

Le frontend peut masquer certaines fonctionnalités afin d'améliorer l'expérience utilisateur, mais ne constitue jamais une barrière de sécurité.

Les permissions métier sont définies dans le Domain Model.

---

## 9.5 Isolation des Organizations

SignalLab est conçu comme une plateforme multi-tenant.

Chaque Organization constitue un espace isolé.

Aucune donnée appartenant à une Organization ne doit être accessible depuis une autre.

Cette isolation est systématiquement vérifiée côté serveur.

La connaissance d'un identifiant de ressource ne doit jamais permettre d'y accéder sans disposer des autorisations appropriées.

---

# 10. Persistence

## 10.1 PostgreSQL

SignalLab utilise PostgreSQL comme base de données relationnelle principale.

Ce choix est motivé par :

- sa maturité ;
- sa fiabilité ;
- son respect des standards SQL ;
- ses performances ;
- son écosystème ;
- sa portabilité.

Le modèle métier de SignalLab est naturellement relationnel.

Les entités et leurs relations sont décrites dans le Domain Model.

---

## 10.2 Entity Framework Core

Entity Framework Core est utilisé comme Object-Relational Mapper.

Il est responsable :

- du mapping entre les objets C# et PostgreSQL ;
- de la génération des requêtes SQL ;
- du suivi des modifications ;
- des migrations ;
- de la persistence des données.

Les règles métier ne sont jamais implémentées dans Entity Framework Core.

Le Domain modifie les objets.

Entity Framework Core observe ces modifications puis les persiste.

---

## 10.3 DbContext

Le DbContext représente une unité de travail avec la base de données.

Dans le cadre d'une requête HTTP classique, sa durée de vie correspond généralement à celle de la requête.

Il est responsable :

- du chargement des entités ;
- du suivi des modifications ;
- de la persistence des changements.

Les lectures ne nécessitant aucune modification peuvent utiliser AsNoTracking lorsque cela est pertinent.

---

## 10.4 Transactions

Les opérations nécessitant plusieurs modifications cohérentes sont exécutées au sein d'une transaction.

Une transaction garantit que l'ensemble des modifications est appliqué ou annulé de manière atomique.

Cette approche évite la création d'états intermédiaires incohérents.

---

## 10.5 Contraintes de données

Les contraintes structurelles importantes sont garanties par PostgreSQL.

Cela comprend notamment :

- les Primary Keys ;
- les Foreign Keys ;
- les contraintes d'unicité ;
- les colonnes obligatoires ;
- les index.

Les règles métier restent implémentées dans le Domain.

---

## 10.6 Migrations

Les évolutions du schéma de base de données sont gérées par les Migrations Entity Framework Core.

Chaque évolution du schéma doit être :

- versionnée ;
- reproductible ;
- revue ;
- testée avant déploiement.

---

## 10.7 Données métier structurées

Les Campaigns, Forms, bindings analytiques, Participations, Submissions, Responses, profils MAP et SavedAnalysis sont des structures relationnelles persistées dans PostgreSQL.

Les résultats analytiques ne sont jamais persistés comme source de vérité. Seule la configuration d'une SavedAnalysis est stockée.

Le MVP ne prévoit aucun stockage de fichiers binaires de Build, de pièces jointes ou d'exports.

Si de futurs besoins apparaissent, un stockage objet sera introduit comme responsabilité distincte du modèle relationnel et du Build métier.

---

# Architecture du moteur analytique

## Responsabilité

Le moteur analytique transforme les données métier persistées en projections recalculables.

Il implémente notamment :

- les familles analytiques des QuestionTypes ;
- la normalisation structurelle sur `0–100` ;
- les directions et poids historiques des bindings ;
- le calcul d'un score de Measure par Submission ;
- les agrégations, couvertures et valeurs manquantes ;
- les groupes et filtres ;
- Spearman, Hedges g et les intervalles d'incertitude ;
- les projections nécessaires aux visualisations ;
- la résolution dynamique des profils MAP partagés.

Le moteur est déterministe. Une même version, les mêmes données et la même configuration produisent les mêmes résultats.

## Position dans l'architecture

Le moteur analytique appartient à l'Application et au Domain selon la nature de la responsabilité :

- les invariants, formules et Value Objects méthodologiques sont indépendants des technologies ;
- l'Application compose le Scope, les Permissions et les requêtes ;
- l'Infrastructure exécute les lectures PostgreSQL optimisées ;
- la Presentation sérialise uniquement les projections nécessaires au client.

Aucune formule métier n'est implémentée exclusivement dans React ou SQL.

## AnalyticalEngineVersion

Chaque ensemble de règles calculatoires possède une version explicite.

La version est incluse dans les réponses analytiques et les diagnostics afin de garantir la traçabilité.

Une évolution de formule doit :

- être documentée ;
- disposer de jeux de données de référence ;
- préserver la capacité à expliquer la méthode utilisée ;
- ne jamais réécrire les Responses sources.

Les résultats passés ne sont pas snapshotés dans le MVP. Une SavedAnalysis rouverte est recalculée avec la version active du moteur, clairement affichée.

## Exécution

Le MVP utilise des requêtes HTTP synchrones tant que les budgets de calcul mesurés sont respectés.

Chaque calcul doit prendre en charge :

- un `CancellationToken` ;
- des limites de Scope et de cardinalité ;
- une pagination ou un streaming logique des preuves individuelles ;
- des requêtes SQL agrégées lorsque cela préserve les règles métier ;
- une journalisation de la durée et du volume traité.

Les calculs statistiques nécessitant les scores individuels chargent uniquement les colonnes nécessaires et utilisent des structures en mémoire bornées.

Si les volumes réels dépassent les budgets, un système de jobs persistants pourra être introduit ultérieurement sans modifier les contrats de SavedAnalysis.

## Frontend analytique

L'Analysis Workspace est une Functional Overlay React rendue dans le Shell Next.js.

Le contexte sous-jacent initialise le Scope sans devenir propriétaire de l'Analysis.

TanStack Query gère :

- l'annulation et la déduplication des requêtes ;
- la fraîcheur des projections ;
- l'invalidation après une nouvelle Submission ou un changement de partage MAP ;
- la conservation temporaire des résultats pendant les changements de vue.

Les données calculées restent du Server State et ne sont jamais reconstruites comme source de vérité côté client.

## Tests

Le moteur utilise des jeux de données de référence couvrant :

- chaque QuestionType ;
- les bindings Aligned et Inverted ;
- les poids ;
- les données manquantes ;
- les comparaisons et corrélations ;
- les intervalles ;
- la confidentialité MAP.

Les résultats numériques sont comparés avec des tolérances explicitement définies.

---

# 11. Temps réel — capacité post-MVP

## 11.1 Statut

Le MVP n'installe ni n'exploite SignalR.

Les parcours MVP reposent sur HTTP, TanStack Query et des rafraîchissements explicites ou automatiques raisonnables.

SignalR reste le choix technique privilégié lorsque des fonctionnalités justifient réellement des connexions persistantes, notamment :

- notifications en temps réel ;
- présence collaborative ;
- ResearchBoards partagés ;
- invalidation immédiate entre plusieurs clients actifs.

## 11.2 Principes futurs

SignalR ne remplacera jamais REST ni PostgreSQL.

Les événements informeront les clients qu'un état a changé ; TanStack Query récupérera ensuite la projection canonique.

L'accès à chaque groupe Organization, Project ou ResearchBoard sera autorisé côté serveur.

L'introduction de SignalR nécessitera une ADR et une Milestone dédiées couvrant sécurité, reconnexion, observabilité et montée en charge.

---

# 12. Observabilité

## 12.1 Objectifs

L'observabilité permet de comprendre le comportement du système en développement comme en production.

Elle répond principalement aux questions suivantes :

- Que s'est-il passé ?
- Pourquoi cela s'est-il produit ?
- Où le problème s'est-il produit ?

SignalLab adopte une approche fondée sur les standards afin de conserver une indépendance vis-à-vis des outils de visualisation utilisés.

---

## 12.2 Journalisation

SignalLab utilise Serilog comme système de journalisation.

Les logs sont structurés afin de permettre leur exploitation par des outils d'analyse.

Les informations enregistrées peuvent notamment inclure :

- le nom du cas d'usage ;
- l'utilisateur concerné lorsque cela est approprié ;
- l'Organization concernée ;
- le résultat de l'opération ;
- la durée d'exécution ;
- l'identifiant de corrélation.

Les informations sensibles ne doivent jamais être enregistrées dans les logs.

Cela inclut notamment :

- les mots de passe ;
- les tokens d'authentification ;
- les secrets ;
- les données confidentielles des utilisateurs.

---

## 12.3 Traces distribuées

SignalLab utilise OpenTelemetry afin de produire des traces standardisées.

Les traces permettent de suivre le parcours complet d'une requête au travers des différentes couches de l'application.

Une requête peut ainsi être suivie depuis :

- l'API ASP.NET Core ;
- l'Application Layer ;
- le Domain ;
- Entity Framework Core ;
- PostgreSQL ;
- SignalR.

Cette approche facilite l'identification des problèmes de performance ainsi que leur diagnostic.

---

## 12.4 Corrélation

Chaque opération importante doit pouvoir être retrouvée grâce à un identifiant de corrélation.

Cet identifiant permet de relier :

- les logs ;
- les traces ;
- les éventuelles erreurs.

L'objectif est de faciliter l'analyse d'un problème sans exposer de données sensibles.

---

# 13. Gestion des erreurs

## 13.1 Principes

La gestion des erreurs est centralisée.

Les erreurs sont classées selon leur nature afin de fournir une réponse adaptée au client.

Les principales catégories sont :

- erreurs de validation ;
- erreurs d'authentification ;
- erreurs d'autorisation ;
- ressources inexistantes ;
- conflits métier ;
- erreurs techniques inattendues.

---

## 13.2 Réponses HTTP

Les erreurs sont retournées sous une forme structurée.

Lorsque cela est pertinent, SignalLab s'appuie sur le standard HTTP Problem Details.

Le client reçoit uniquement les informations nécessaires à la compréhension de l'erreur.

Les détails techniques restent enregistrés côté serveur.

---

## 13.3 Exceptions

Les exceptions inattendues ne doivent jamais être exposées directement aux utilisateurs.

Les éléments suivants ne doivent notamment jamais apparaître dans les réponses HTTP :

- stack traces ;
- requêtes SQL ;
- chemins internes ;
- secrets ;
- informations d'infrastructure.

Ces informations sont uniquement conservées dans les systèmes de journalisation.

---

# 14. Sécurité

## 14.1 Principes

La sécurité constitue une préoccupation permanente du projet.

Sans viser les contraintes d'une très grande plateforme dès le MVP, SignalLab adopte dès le départ plusieurs principes fondamentaux :

- authentification déléguée à un Identity Provider spécialisé ;
- communications exclusivement en HTTPS ;
- vérification systématique des autorisations côté serveur ;
- isolation stricte des Organizations ;
- stockage sécurisé des secrets ;
- journalisation sans données sensibles.

---

## 14.2 Secrets

Aucun secret ne doit être présent dans le dépôt Git.

Cela comprend notamment :

- les clés Clerk ;
- les chaînes de connexion ;
- les clés d'API ;
- les secrets de signature ;
- les tokens d'accès.

Les secrets sont fournis par l'environnement d'exécution.

---

## 14.3 Protection des données

Les données manipulées par SignalLab peuvent représenter des informations sensibles appartenant à ses utilisateurs.

Le système applique donc les principes suivants :

- minimisation des données collectées ;
- chiffrement des communications via HTTPS ;
- chiffrement des données au repos assuré par l'hébergeur ;
- sauvegardes protégées ;
- traçabilité des opérations importantes ;
- séparation stricte des données entre Organizations.

Ces principes constituent les fondations de la stratégie de sécurité du projet.

---

# 15. Tests

## 15.1 Objectifs

Les tests ont pour objectif de garantir la stabilité du système tout au long de son évolution.

Ils permettent notamment de vérifier :

- les règles métier ;
- les cas d'usage ;
- les échanges avec la base de données ;
- les principaux parcours utilisateurs.

---

## 15.2 Tests unitaires

Les tests unitaires concernent principalement le Domain.

Ils permettent de vérifier les règles métier indépendamment des technologies utilisées.

Ces tests doivent être :

- rapides ;
- déterministes ;
- indépendants de la base de données.

---

## 15.3 Tests d'intégration

Les tests d'intégration vérifient le bon fonctionnement des interactions entre plusieurs composants.

Ils concernent notamment :

- ASP.NET Core ;
- Entity Framework Core ;
- PostgreSQL ;
- l'authentification ;
- les contrats HTTP.

---

## 15.4 Tests Frontend

Le frontend est testé selon la valeur métier des fonctionnalités développées.

Les principaux types de tests comprennent :

- tests unitaires ;
- tests de composants ;
- tests d'intégration ;
- tests des principaux parcours utilisateur.

L'objectif n'est pas d'atteindre un taux arbitraire de couverture mais de sécuriser les comportements les plus importants.

---

# 16. Documentation de l'API

L'API expose une documentation OpenAPI.

Cette documentation permet :

- de documenter les endpoints ;
- de tester les requêtes pendant le développement ;
- de faciliter la compréhension des contrats HTTP.

Le frontend étant développé conjointement avec le backend par un unique développeur, les modèles TypeScript sont actuellement maintenus manuellement.

La génération automatique des contrats à partir d'OpenAPI pourra être réévaluée si plusieurs clients indépendants ou plusieurs développeurs interviennent sur le projet.

---

# 17. Déploiement

## 17.1 Docker

SignalLab utilise Docker afin de garantir un environnement reproductible.

Les principaux services sont conteneurisés :

- frontend ;
- backend ;
- PostgreSQL.

Cette approche permet d'obtenir un environnement cohérent entre :

- le développement ;
- les tests ;
- la production.

---

## 17.2 Environnements

Le projet distingue actuellement deux environnements :

- Development ;
- Production.

Un environnement Staging pourra être introduit lorsque le projet accueillera des utilisateurs réels ou nécessitera une validation préalable avant mise en production.

---

## 17.3 Hébergement

Le fournisseur d'hébergement n'est pas encore définitivement retenu.

Le choix devra notamment prendre en compte :

- le coût ;
- la simplicité d'exploitation ;
- le support de Docker ;
- la disponibilité d'une base PostgreSQL ;
- la gestion sécurisée des secrets.

L'architecture ne dépend volontairement d'aucun fournisseur spécifique.

---

# 18. Intégration continue

SignalLab utilise GitHub Actions afin d'automatiser les principales étapes de validation.

La pipeline comprend notamment :

- la restauration des dépendances ;
- la compilation ;
- l'exécution des tests ;
- la construction des images Docker.

L'objectif est de garantir qu'une modification ne dégrade pas l'état du projet avant son déploiement.

---

# 19. Performance et évolutivité

Le MVP privilégie une architecture simple et robuste.

Les optimisations sont réalisées lorsqu'un besoin mesuré apparaît.

Les principales pratiques retenues sont :

- pagination des collections importantes ;
- requêtes SQL optimisées ;
- limitation des requêtes inutiles ;
- mesure des performances avant optimisation.

Le backend reste stateless afin de faciliter une éventuelle montée en charge future.

Des technologies telles que Redis pourront être introduites si les besoins évoluent.

---

# 20. Traitements en arrière-plan

Le MVP n'introduit pas immédiatement de système spécialisé de Background Jobs.

Les calculs analytiques sont exécutés dans la requête HTTP uniquement tant que les limites de Scope, les budgets de temps et l'annulation garantissent une expérience acceptable.

Cette décision doit être validée par des mesures sur les jeux de données de référence de M09, et non supposée définitivement.

Une solution telle que Hangfire pourra être introduite si le produit nécessite :

- des Analyses dépassant les budgets synchrones ;
- des exports longs ;
- des traitements planifiés ;
- des calculs persistants devant survivre à un redémarrage ;
- des recalculs massifs déclenchés hors requête.

L'ajout d'un job ne modifiera pas le Domain Model : les résultats resteront des projections recalculables et les SavedAnalysis conserveront uniquement leur configuration.

---

# 21. Technologies non retenues

Certaines technologies ont volontairement été écartées de la première version du projet.

Il s'agit notamment de :

- GraphQL ;
- une architecture Microservices ;
- Redis ;
- Hangfire ;
- un système de stockage de fichiers binaires ;
- une stratégie explicite de versioning de l'API.

Ces choix ne traduisent pas une incapacité technique mais une volonté de conserver une architecture adaptée aux besoins actuels.

Chaque technologie pourra être réévaluée lorsqu'un besoin concret apparaîtra.

---

# 22. Décisions techniques

## Frontend

- React
- Next.js
- TypeScript
- TanStack Query
- Tailwind CSS
- shadcn/ui

## Backend

- ASP.NET Core
- C#
- Clean Architecture
- Entity Framework Core
- PostgreSQL

## Communication

- HTTP
- REST
- JSON
- SignalR retenu pour les capacités temps réel post-MVP, non implémenté dans le MVP

## Analytics

- moteur déterministe côté Backend
- AnalyticalEngineVersion explicite
- SavedAnalysis comme configuration uniquement
- aucun résultat calculé persisté
- exécution synchrone bornée et annulable pour le MVP

## Authentification

- Clerk
- OAuth 2.0
- OpenID Connect
- JWT

## Observabilité

- Serilog
- OpenTelemetry

## Déploiement

- Docker
- GitHub Actions

---

# 23. Conclusion

SignalLab adopte une architecture moderne, modulaire et volontairement pragmatique.

Les choix réalisés privilégient la simplicité, la maintenabilité et les standards ouverts plutôt que l'introduction prématurée de technologies complexes.

L'application est construite autour d'une séparation claire entre :

- la présentation ;
- les cas d'usage ;
- les règles métier ;
- les détails techniques.

Cette architecture constitue une base solide pour le développement du MVP tout en conservant la capacité d'évoluer progressivement vers une plateforme SaaS collaborative plus ambitieuse.

Les évolutions futures seront guidées par les besoins réels du produit plutôt que par l'adoption systématique de nouvelles technologies.