# M01 — Platform Foundation

**Version :** 3.0
**Statut :** ✅ Validée
**Dernière mise à jour :** 30/09/2026
**Milestone suivante :** M02

---

> **Décision de conception**
>
> Cette Milestone construit l'ensemble des fondations techniques et architecturales de SignalLab.
>
> Aucun domaine métier n'est encore implémenté.
>
> Son objectif est de figer toutes les décisions structurelles de l'application afin que les Milestones suivantes n'aient plus qu'à ajouter des fonctionnalités métier sans remettre en question l'architecture.

---

# Objectif

Construire la plateforme technique de SignalLab.

À la fin de cette Milestone, le projet dispose :

- d'une architecture logicielle documentée ;
- d'une architecture frontend figée ;
- d'un Application Shell permanent ;
- d'une architecture de navigation complète ;
- d'une infrastructure de développement reproductible ;
- d'une première version déployée.

---

# Valeur produit

Cette Milestone ne livre aucune fonctionnalité métier.

Elle garantit :

- une architecture stable ;
- une expérience de développement cohérente ;
- une interface prête à accueillir les futurs domaines métier ;
- une base de code évolutive.

---

# Résultat attendu

À la fin de cette Milestone :

- le dépôt est structuré ;
- le Backend démarre ;
- le Frontend démarre ;
- l'Application Shell est opérationnel ;
- la navigation fonctionne ;
- PostgreSQL est configuré ;
- Entity Framework est configuré ;
- Docker fonctionne ;
- la CI fonctionne ;
- une première version est déployée.

---

# Hors scope

Cette Milestone ne contient aucune logique métier.

Sont exclus :

- authentification ;
- Users ;
- Organizations ;
- Projects ;
- Builds ;
- Campaigns ;
- Participation ;
- Results ;
- MAP ;
- Analyses.

---

# Technologies concernées

## Backend

- ASP.NET Core
- Entity Framework Core
- PostgreSQL

## Frontend

- React
- TypeScript
- Vite
- Tailwind CSS
- TanStack Router
- TanStack Query

## Infrastructure

- Docker
- Docker Compose
- GitHub
- GitHub Actions

---

# Architecture concernée

Cette Milestone définit définitivement :

- l'architecture Backend ;
- l'architecture Frontend ;
- l'Application Shell ;
- la Navigation ;
- les conventions ;
- l'infrastructure.

---

# Principes

- L'architecture précède les fonctionnalités.
- Le Shell de l'application est permanent.
- Les domaines métier remplissent le Shell mais ne le modifient jamais.
- La navigation est indépendante du métier.
- Les modèles Explorer, Workspace et Document sont définis avant leur première utilisation.

---

# Scénario d'acceptation

Un nouveau développeur doit pouvoir :

1. Cloner le dépôt.
2. Lancer Docker.
3. Démarrer Backend et Frontend.
4. Accéder à l'application.
5. Naviguer dans l'Application Shell.
6. Comprendre l'architecture uniquement grâce à la documentation.

---

# Specs

---

## SL-010 — Initialiser le dépôt

### Description

Créer la structure officielle du dépôt SignalLab.

### Objectif produit

Disposer d'une base commune pour l'ensemble du développement.

### Objectif technique

Définir l'organisation générale du repository.

### Critères d'acceptation

- Structure validée.
- README créé.
- Licence ajoutée.
- .gitignore configuré.

### Definition of Done

- Repository opérationnel.
- Documentation initiale présente.

---

## SL-011 — Initialiser le Backend

### Description

Créer la solution ASP.NET Core.

### Objectif technique

Préparer la plateforme backend.

### Critères d'acceptation

- Compilation.
- Swagger.
- Lancement local.

### Definition of Done

- Backend opérationnel.

---

## SL-012 — Initialiser le Frontend

### Description

Créer l'application React.

### Objectif technique

Préparer la plateforme frontend.

### Critères d'acceptation

- React fonctionne.
- Tailwind configuré.
- Routing disponible.

### Definition of Done

- Frontend opérationnel.

---

## SL-013 — Définir l'architecture de la solution

### Description

Figer officiellement l'architecture logicielle de SignalLab.

### Cette Spec documente notamment

- Backend
- Frontend
- Vertical Slice
- DTO
- API
- Persistence
- Tests
- ADR
- Nommage
- Arborescence

### Objectif produit

Éviter les décisions implicites.

### Objectif technique

Stabiliser définitivement la structure du projet.

### Principes impliqués

- Architecture documentée.
- Responsabilités explicites.
- Décisions justifiées.
- ADR.

### Critères d'acceptation

- Backend défini.
- Frontend défini.
- DTO définis.
- API définies.
- Persistence définie.
- Tests définis.
- ADR créés.

### Definition of Done

- Documentation validée.

---

## SL-014 — Construire l'Application Shell

### Description

Construire le squelette permanent de SignalLab.

Aucune fonctionnalité métier n'est encore intégrée.

Le Shell est constitué de trois régions permanentes :

- Header
- Sidebar
- Content

### Objectif produit

Garantir une structure d'application stable pendant toute la durée du projet.

### Objectif technique

Créer les composants racines de l'interface.

### Principes impliqués

- Header permanent.
- Sidebar permanente.
- Content permanent.
- Desktop First.
- Adaptation limitée aux dimensions des fenêtres desktop.
- Navigateurs mobiles et interactions tactiles non supportés.

### Critères d'acceptation

Le Shell affiche correctement :

- Header vide.
- Sidebar vide.
- Content vide.

Les trois régions restent présentes quelle que soit la route prise en charge par l'application desktop.

Les overlays futurs pourront être affichés sans modifier cette structure.

Sur un appareil ou un viewport mobile non supporté, le Shell n'est pas affiché et une page informative indique que SignalLab nécessite un navigateur desktop.

### Definition of Done

- AppShell implémenté.
- Layout global fonctionnel.
- Documentation mise à jour.

---

## SL-015 — Implémenter l'architecture de navigation

### Description

Construire le moteur de navigation de SignalLab.

Cette Spec ne crée aucun contenu métier.

Elle met uniquement en place les mécanismes permettant aux futures Milestones d'enregistrer leurs contextes.

### Cette Spec comprend

- Context Navigation
- Local Navigation
- Navigation State
- Context Stack
- Routing
- Layout switching

Elle introduit officiellement les trois modèles de visualisation :

- Explorer
- Workspace
- Document

sans implémenter aucun de leurs contenus.

### Objectif produit

Garantir une navigation cohérente dans toute l'application.

### Objectif technique

Découpler complètement la navigation des domaines métier.

### Principes impliqués

- Context Navigation indépendante.
- Local Navigation indépendante.
- Les Explorers partagent la même structure.
- Les Workspaces partagent la même structure.
- Les Documents partagent la même structure.

### Critères d'acceptation

La navigation entre contextes est opérationnelle.

Le changement de contexte remplace le contenu sans reconstruire le Shell.

Le changement de vue locale ne modifie jamais le contexte.

Les états de navigation sont persistables.

### Definition of Done

- Infrastructure de navigation implémentée.
- Documentation mise à jour.

---

## SL-016 — Configurer PostgreSQL

### Description

Configurer la base de données principale de SignalLab.

Cette Spec établit l'ensemble des conventions de stockage qui seront utilisées par les futurs domaines métier.

Aucune table métier n'est encore créée.

### Objectif produit

Disposer d'une base de données stable dès le début du projet.

### Objectif technique

Mettre en place PostgreSQL et préparer l'intégration avec Entity Framework.

### Principes impliqués

- Une unique base de données.
- Une seule source de vérité.
- Convention avant configuration.

### Critères d'acceptation

- PostgreSQL démarre correctement.
- Connexion locale fonctionnelle.
- Connexion Docker fonctionnelle.
- Chaîne de connexion configurable.

### Definition of Done

- Base accessible.
- Documentation mise à jour.

---

## SL-017 — Configurer Entity Framework Core

### Description

Configurer Entity Framework Core comme couche de persistance.

Cette Spec définit les conventions générales :

- DbContext
- Migrations
- Configurations
- Naming conventions

Aucun domaine métier n'est encore implémenté.

### Objectif produit

Préparer une persistance robuste et évolutive.

### Objectif technique

Uniformiser l'ensemble de l'accès aux données.

### Principes impliqués

- Fluent API privilégiée.
- Une configuration par entité.
- Migrations versionnées.

### Critères d'acceptation

- DbContext créé.
- Première migration générable.
- Migration applicable.

### Definition of Done

- Persistance prête.

---

## SL-018 — Configurer les environnements

### Description

Mettre en place les différents environnements d'exécution.

Cette Spec définit notamment :

- Development
- Staging
- Production

ainsi que les conventions de configuration.

### Objectif produit

Garantir un comportement reproductible entre les environnements.

### Objectif technique

Séparer clairement les configurations.

### Principes impliqués

- Configuration externalisée.
- Aucun secret dans le code.
- Variables d'environnement.

### Critères d'acceptation

- Les environnements sont configurés.
- Les paramètres sont injectés correctement.
- Les secrets sont externalisés.

### Definition of Done

- Configuration validée.

---

## SL-019 — Configurer Docker

### Description

Créer l'infrastructure Docker du projet.

Cette Spec permet de lancer localement l'ensemble de la plateforme.

### Objectif produit

Faciliter l'installation du projet.

### Objectif technique

Standardiser l'environnement de développement.

### Principes impliqués

- Infrastructure reproductible.
- Un simple lancement.
- Aucun prérequis spécifique à une machine.

### Critères d'acceptation

Le projet démarre avec Docker Compose.

Backend et PostgreSQL communiquent correctement.

### Definition of Done

- Docker opérationnel.

---

## SL-020 — Mettre en place la CI/CD

### Description

Créer la première pipeline GitHub Actions.

Cette pipeline garantit la qualité minimale avant chaque fusion.

### Objectif produit

Réduire les erreurs d'intégration.

### Objectif technique

Automatiser les contrôles essentiels.

### La pipeline exécute notamment

- Build Backend
- Build Frontend
- Tests
- Lint
- Vérifications de qualité

### Critères d'acceptation

Chaque Pull Request lance automatiquement la pipeline.

Une erreur bloque la fusion.

### Definition of Done

- Pipeline opérationnelle.

---

## SL-021 — Réaliser le premier déploiement

### Description

Publier la première version de SignalLab.

Cette version ne contient encore aucune fonctionnalité métier.

Son objectif est uniquement de valider toute la chaîne de déploiement.

### Objectif produit

Valider la capacité de livraison continue.

### Objectif technique

Tester la chaîne complète :

Repository

↓

CI

↓

Build

↓

Publication

↓

Application accessible

### Critères d'acceptation

L'application est accessible publiquement.

Le Backend répond.

Le Frontend est servi correctement.

### Definition of Done

- Premier déploiement validé.

---

## SL-022 — Finaliser la documentation développeur

### Description

Compléter toute la documentation technique nécessaire au développement des prochaines Milestones.

Cette Spec clôt officiellement les fondations de SignalLab.

### La documentation couvre notamment

- Architecture
- Conventions
- Workflow Git
- Branching
- Pull Requests
- ADR
- Structure des Features
- Documentation produit
- Documentation technique

### Objectif produit

Permettre à un nouveau développeur de contribuer rapidement.

### Objectif technique

Faire de la documentation la référence officielle du projet.

### Principes impliqués

- Documentation First.
- Une seule source de vérité.
- Toute décision importante est documentée.

### Critères d'acceptation

Toute l'architecture est documentée.

Le workflow de développement est documenté.

Les conventions sont documentées.

### Definition of Done

- Documentation validée.

---

# Fin de Milestone

À la fin de M01, SignalLab dispose de :

- une architecture backend figée ;
- une architecture frontend figée ;
- un Application Shell permanent ;
- une architecture de navigation complète ;
- une infrastructure de persistance prête ;
- une chaîne CI/CD fonctionnelle ;
- une première version déployée ;
- une documentation technique complète.

Aucun domaine métier n'a encore été implémenté.

Les Milestones suivantes pourront se concentrer exclusivement sur les fonctionnalités métier en s'appuyant sur ces fondations.