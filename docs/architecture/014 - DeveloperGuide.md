# SignalLab — Guide développeur

**Projet :** SignalLab
**Document :** Guide développeur de la Platform Foundation
**Version :** 1.0
**Statut :** Validé
**Spec propriétaire :** SL-022 — Finaliser la documentation développeur

---

# 1. Objectif

Ce document constitue le point d'entrée technique pour développer SignalLab.

Il doit permettre à un nouveau développeur de :

- préparer son environnement ;
- configurer SignalLab sans secret versionné ;
- lancer la plateforme localement ;
- exécuter les validations ;
- comprendre l'organisation générale de la solution ;
- identifier les conventions applicables ;
- suivre le workflow Git du repository ;
- distinguer les capacités actuellement implémentées des capacités futures.

Ce document ne remplace pas les documents spécialisés de `docs/architecture`.

Il fournit le parcours principal et renvoie vers eux lorsqu'un sujet nécessite davantage de détail.

---

# 2. État de la plateforme

À la fin de M01 — Platform Foundation, SignalLab dispose de :

- un Frontend Next.js ;
- une API ASP.NET Core ;
- un Application Shell desktop ;
- une architecture de navigation ;
- PostgreSQL ;
- Entity Framework Core ;
- trois environnements applicatifs ;
- des images Docker ;
- une stack Docker Compose locale ;
- une CI GitHub Actions ;
- un environnement de staging distant.

La plateforme ne contient encore aucun domaine métier.

Les Milestones suivantes ajoutent progressivement les domaines sans remettre en cause les fondations établies par M01.

---

# 3. Prérequis

Le développement local nécessite :

```text
Git
.NET SDK 10
Node.js 24
npm
Docker Desktop
```

Le repository contient un `global.json` définissant la version .NET de référence :

```text
10.0.302
```

La stratégie `rollForward` permet l'utilisation d'un Feature Band .NET 10 compatible plus récent.

La version Node.js actuellement utilisée par le projet est :

```text
24.18.0
```

Le Frontend utilise actuellement notamment :

```text
Next.js 16.3.7
React 19.2.4
```

Docker fournit PostgreSQL pour le développement local.

L'image PostgreSQL actuellement utilisée est :

```text
postgres:18.6
```

Bash est uniquement nécessaire pour les scripts du repository écrits en shell.

---

# 4. Cloner le repository

Cloner le repository puis se placer à sa racine :

```powershell
git clone <repository-url> SignalLab
cd SignalLab
```

Toutes les commandes de ce document sont exécutées depuis la racine du repository sauf indication contraire.

La racine contient notamment :

```text
SignalLab/
├── apps/
│   ├── api/
│   └── web/
├── docs/
│   ├── architecture/
│   ├── milestones/
│   └── product/
├── scripts/
├── compose.yaml
├── global.json
├── .env.example
└── README.md
```

---

# 5. Configuration locale

## 5.1 Fichier `.env`

Docker Compose utilise un fichier `.env` local situé à la racine du repository.

Créer ce fichier depuis l'exemple versionné :

```powershell
Copy-Item .env.example .env
```

Le fichier contient les paramètres locaux PostgreSQL :

```dotenv
POSTGRES_DB=signallab
POSTGRES_USER=signallab
POSTGRES_PASSWORD=<mot-de-passe-local>
POSTGRES_PORT=5432
```

Remplacer uniquement la valeur du mot de passe par une valeur locale.

Le fichier `.env` est ignoré par Git.

Il ne doit jamais être commit.

Les valeurs de `.env` servent à l'environnement Docker local et ne sont pas utilisées comme mécanisme de secrets pour Staging ou Production.

---

## 5.2 Configuration native de l'API

Lorsque l'API est exécutée directement avec `dotnet run`, la chaîne PostgreSQL Development est stockée avec .NET User Secrets.

Le projet concerné est :

```text
apps/api/src/SignalLab.Api/SignalLab.Api.csproj
```

La clé attendue est :

```text
ConnectionStrings:Postgres
```

La valeur possède la forme suivante :

```text
Host=localhost;Port=5432;Database=signallab;Username=signallab;Password=<mot-de-passe-local>
```

Elle doit utiliser le même mot de passe que le service PostgreSQL local.

Configurer le secret avec :

```powershell
dotnet user-secrets set `
  "ConnectionStrings:Postgres" `
  "Host=localhost;Port=5432;Database=signallab;Username=signallab;Password=<mot-de-passe-local>" `
  --project apps/api/src/SignalLab.Api/SignalLab.Api.csproj
```

Ne jamais utiliser une credential de Staging ou Production pour cette configuration.

Ne jamais copier un secret réel dans un fichier versionné.

La gestion détaillée des environnements et secrets est documentée dans :

```text
010 - Environments.md
```

---

# 6. Démarrage recommandé — Docker Compose

Docker Compose constitue le moyen le plus simple de lancer toute la Platform Foundation.

Il démarre :

```text
PostgreSQL
API ASP.NET Core
Frontend Next.js
```

## 6.1 Premier démarrage

Après création du fichier `.env` :

```powershell
docker compose up --build -d
```

Cette commande :

- construit les images applicatives ;
- crée le réseau Docker ;
- crée le volume PostgreSQL si nécessaire ;
- démarre PostgreSQL ;
- attend son health check ;
- démarre l'API ;
- démarre le Frontend.

---

## 6.2 Vérifier les services

```powershell
docker compose ps
```

Les trois services doivent être présents :

```text
postgres
api
web
```

PostgreSQL doit apparaître comme :

```text
healthy
```

L'API est exposée localement sur :

```text
http://localhost:8080
```

Le Frontend est exposé sur :

```text
http://localhost:3000
```

---

## 6.3 Vérifier l'API

```powershell
Invoke-WebRequest `
  http://localhost:8080/health `
  -UseBasicParsing
```

Résultat attendu :

```text
StatusCode : 200
Content    : Healthy
```

---

## 6.4 Vérifier le Frontend

```powershell
Invoke-WebRequest `
  http://localhost:3000/home `
  -UseBasicParsing
```

Résultat attendu :

```text
StatusCode : 200
```

Le navigateur peut ensuite ouvrir :

```text
http://localhost:3000/home
```

Le Shell SignalLab doit être visible.

---

## 6.5 Arrêter la stack

```powershell
docker compose down
```

Cette commande supprime les conteneurs et le réseau mais conserve le volume PostgreSQL.

Les données locales persistent donc entre deux démarrages.

La suppression du volume doit toujours être explicite.

Les détails sont documentés dans :

```text
011 - Docker.md
```

---

# 7. Développement hors Docker

Les applications peuvent également être exécutées directement sur la machine.

Cette méthode est utile pendant le développement quotidien.

---

## 7.1 PostgreSQL

Démarrer uniquement PostgreSQL avec Docker :

```powershell
docker compose up -d postgres
```

Vérifier :

```powershell
docker compose ps
```

Le service `postgres` doit être `healthy`.

---

## 7.2 Backend

Restaurer les dépendances :

```powershell
dotnet restore apps/api/SignalLab.Api.sln
```

Lancer l'API avec le profil HTTPS Development :

```powershell
dotnet run `
  --project apps/api/src/SignalLab.Api/SignalLab.Api.csproj `
  --launch-profile https
```

L'API Development écoute notamment sur :

```text
https://localhost:7281
```

Vérifier :

```powershell
Invoke-WebRequest `
  https://localhost:7281/health `
  -UseBasicParsing
```

Le certificat HTTPS de développement peut nécessiter d'être approuvé localement avec :

```powershell
dotnet dev-certs https --trust
```

---

## 7.3 Frontend

Installer les dépendances exactement à partir du lockfile :

```powershell
npm --prefix apps/web ci
```

Lancer le serveur de développement :

```powershell
npm --prefix apps/web run dev
```

Le Frontend est disponible sur :

```text
http://localhost:3000
```

---

# 8. Validation locale

Une modification doit être validée au niveau correspondant avant d'être proposée en Pull Request.

---

## 8.1 Backend

Depuis la racine :

```powershell
dotnet restore apps/api/SignalLab.Api.sln

dotnet build `
  apps/api/SignalLab.Api.sln `
  --configuration Release `
  --no-restore

dotnet test `
  apps/api/SignalLab.Api.sln `
  --configuration Release `
  --no-build `
  --no-restore
```

---

## 8.2 Frontend

Installer les dépendances :

```powershell
npm --prefix apps/web ci
```

Puis exécuter :

```powershell
npm --prefix apps/web run lint
npm --prefix apps/web run typecheck
npm --prefix apps/web test
npm --prefix apps/web run build
```

Un changement de dépendance destiné au runtime doit également être vérifié avec :

```powershell
npm --prefix apps/web audit --omit=dev
```

---

## 8.3 Docker

Valider la configuration Compose :

```powershell
docker compose config --quiet
```

Construire et lancer l'ensemble :

```powershell
docker compose up --build -d
```

Puis vérifier :

```powershell
docker compose ps
```

Après validation :

```powershell
docker compose down
```

---

## 8.4 CI

La CI reproduit les contrôles principaux sur GitHub.

Elle possède deux jobs indépendants :

```text
Backend
Frontend
```

Chaque job :

- restaure ou installe ses dépendances ;
- compile l'application ;
- exécute ses validations ;
- construit son image Docker.

Les détails sont documentés dans :

```text
012 - CI-CD.md
```

---

# 9. Workflow Git

Le développement suit une branche par Spec.

Une nouvelle Spec commence depuis un `main` à jour :

```powershell
git switch main
git pull
git switch -c sl-###-description-courte
```

Exemple :

```text
sl-022-developer-documentation
```

Le travail de la Spec est réalisé sur cette branche.

Avant la Pull Request :

```powershell
git status
```

Les validations locales pertinentes sont exécutées.

La branche est ensuite poussée :

```powershell
git push -u origin sl-###-description-courte
```

Une Pull Request est ouverte vers :

```text
main
```

`main` est protégé.

La fusion nécessite la réussite des checks :

```text
Backend
Frontend
```

Une erreur dans l'un de ces checks bloque la fusion.

Après fusion :

```powershell
git switch main
git pull
git branch -d sl-###-description-courte
git push origin --delete sl-###-description-courte
```

Une Spec ne doit pas être développée directement sur `main`.

Une Spec doit rester limitée à son périmètre propriétaire.

Une capacité appartenant à une Milestone future ne doit pas être implémentée par anticipation.

---

# 10. Organisation de l'architecture

SignalLab est développé comme un Modular Monolith.

Le système possède actuellement :

```text
Browser
    ↓
Next.js
    ↓
ASP.NET Core API
    ↓
Application
    ↓
Domain
    ↑
Infrastructure
    ↓
Entity Framework Core
    ↓
PostgreSQL
```

Le Backend constitue l'autorité métier.

Le Frontend représente l'état du système et transmet les intentions de l'utilisateur.

Le Domain ne dépend d'aucune technologie de présentation, de persistence ou d'infrastructure.

La Clean Architecture définit la direction des dépendances.

Les Vertical Slices organisent les cas d'usage à l'intérieur des modules métier.

La référence opérationnelle principale est :

```text
000 - SolutionArchitecture.md
```

La référence conceptuelle technique est :

```text
docs/product/06 - SoftwareArchitecture.md
```

---

# 11. Conventions

Les règles détaillées ne sont pas dupliquées dans ce guide.

Elles sont définies par les documents suivants.

## Backend

```text
001 - BackendConventions.md
```

Ce document définit notamment :

- les responsabilités des projets .NET ;
- les Vertical Slices ;
- les Commands et Queries ;
- les Minimal APIs ;
- les contrats ;
- le nommage ;
- la validation ;
- les erreurs ;
- la Dependency Injection.

## Frontend

```text
002 - FrontendConventions.md
```

Ce document définit notamment :

- l'App Router ;
- l'organisation des Features ;
- les Server et Client Components ;
- TanStack Query ;
- les contrats API ;
- les composants ;
- TypeScript ;
- le styling ;
- l'accessibilité ;
- les tests Frontend.

## API

```text
003 - APIConventions.md
```

Ce document définit notamment :

- les routes ;
- les méthodes HTTP ;
- les contrats JSON ;
- les statuts HTTP ;
- Problem Details ;
- pagination, filtrage, tri et recherche ;
- OpenAPI ;
- les conventions Minimal APIs.

## Persistence

```text
004 - PersistenceConventions.md
```

Ce document définit notamment :

- les responsabilités de persistence ;
- le DbContext ;
- Fluent API ;
- les migrations ;
- les transactions ;
- les contraintes ;
- les index ;
- les conventions de lecture et d'écriture.

## Tests

```text
005 - TestingStrategy.md
```

Ce document définit les niveaux de tests, leur responsabilité et les validations attendues selon le type de changement.

---

# 12. Architecture Decision Records

Les décisions architecturales structurantes sont conservées dans :

```text
docs/architecture/adr/
```

L'index est :

```text
docs/architecture/adr/000 - ADRIndex.md
```

Une ADR décrit pourquoi une décision structurante a été prise.

La documentation opérationnelle décrit comment cette décision s'applique actuellement au repository.

Une nouvelle ADR est nécessaire lorsqu'une évolution modifie durablement notamment :

- la structure de la solution ;
- une technologie structurante ;
- la direction des dépendances ;
- le style de communication ;
- l'infrastructure distribuée ;
- une convention architecturale fondamentale.

Une ADR existante n'est pas réécrite pour masquer une ancienne décision.

Lorsqu'une décision est remplacée, une nouvelle ADR est créée et l'ancienne est marquée `Superseded`.

---

# 13. Documentation de la Platform Foundation

Les documents produits pendant M01 sont :

```text
000 - SolutionArchitecture.md
001 - BackendConventions.md
002 - FrontendConventions.md
003 - APIConventions.md
004 - PersistenceConventions.md
005 - TestingStrategy.md
006 - ApplicationShell.md
007 - NavigationArchitecture.md
008 - PostgreSQL.md
009 - Entity Framework Core.md
010 - Environments.md
011 - Docker.md
012 - CI-CD.md
013 - FirstDeployment.md
014 - DeveloperGuide.md
```

Ils constituent la documentation opérationnelle de la Platform Foundation.

Les documents de `docs/product` restent les références produit et conceptuelles.

Les documents de `docs/milestones` définissent le périmètre, l'ordre et les critères d'acceptation des Specs.

---

# 14. Environnements

SignalLab reconnaît exactement trois environnements :

```text
Development
Staging
Production
```

Tout autre nom d'environnement est rejeté au démarrage de l'API.

## Development

Development est utilisé pour le développement local.

Les secrets de l'API exécutée nativement sont stockés avec .NET User Secrets.

Docker Compose utilise le fichier `.env` local.

## Staging

Le staging permet de valider une version distante avant la production.

Les secrets sont injectés par l'environnement d'hébergement et ne sont jamais versionnés.

Le staging actuel est accessible à :

```text
https://staging.signallab.dev
https://api.staging.signallab.dev
```

Le Frontend de staging émet :

```text
X-Robots-Tag: noindex, nofollow
```

afin de ne pas être indexé.

## Production

Production est un environnement reconnu par l'application mais le MVP complet n'est pas encore déployé en production à la fin de M01.

L'infrastructure de production finale appartient à une Milestone ultérieure.

Les détails sont documentés dans :

```text
010 - Environments.md
013 - FirstDeployment.md
```

---

# 15. État actuel et capacités futures

La documentation distingue trois notions :

```text
Implémenté maintenant
Architecture retenue mais non implémentée
Post-MVP
```

Une technologie présente dans la Software Architecture ou dans une ADR n'est pas nécessairement déjà implémentée.

Le statut des documents opérationnels et des Specs détermine l'état réellement applicable.

---

## 15.1 Implémenté à la fin de M01

La Platform Foundation comprend :

- Next.js ;
- React ;
- TypeScript ;
- Tailwind CSS ;
- shadcn/ui ;
- TanStack Query ;
- Application Shell ;
- architecture de navigation ;
- ASP.NET Core ;
- Clean Architecture ;
- Vertical Slices comme convention d'organisation ;
- Minimal APIs ;
- OpenAPI ;
- PostgreSQL ;
- Entity Framework Core ;
- migrations EF Core ;
- Development, Staging et Production comme environnements reconnus ;
- Docker ;
- Docker Compose ;
- GitHub Actions ;
- staging distant.

---

## 15.2 Retenu mais non encore implémenté

Certaines décisions appartiennent au MVP mais nécessitent une Milestone ultérieure.

C'est notamment le cas de :

```text
Clerk
```

L'authentification appartient à M02.

Elle ne doit pas être introduite dans M01.

---

## 15.3 Capacités différées

Les capacités suivantes ne sont pas introduites sans besoin fonctionnel explicite et Spec propriétaire :

- SignalR ;
- collaboration temps réel ;
- ResearchBoards ;
- Redis ;
- Microservices ;
- Event Bus distribué ;
- système spécialisé de Background Jobs ;
- stockage binaire de Builds ;
- versioning explicite de l'API.

Leur présence dans la vision technique n'autorise pas leur implémentation anticipée.

---

# 16. Staging

Le premier staging est actuellement hébergé sur Render.

Render est un choix d'infrastructure et non une dépendance architecturale du produit.

Les standards et responsabilités de SignalLab restent indépendants du fournisseur.

Les services de staging suivent :

```text
main
```

Le Frontend est disponible sur :

```text
https://staging.signallab.dev
```

L'API de santé est disponible sur :

```text
https://api.staging.signallab.dev/health
```

Le staging possède sa propre configuration PostgreSQL et ses propres secrets.

Aucun secret de staging ne doit être copié dans le repository ou dans la configuration Development.

Les détails du premier déploiement sont documentés dans :

```text
013 - FirstDeployment.md
```

---

# 17. Règles de sécurité minimales

Un développeur ne doit jamais commit :

- un mot de passe ;
- une chaîne de connexion réelle ;
- une URL contenant des credentials ;
- une clé API ;
- un token ;
- un secret d'authentification ;
- un fichier `.env` local.

Les secrets Development appartiennent à la machine du développeur.

Les secrets Staging et Production appartiennent à la configuration de leur environnement respectif.

En cas d'exposition d'une credential, celle-ci doit être renouvelée.

---

# 18. Où chercher une information

Pour comprendre le produit :

```text
docs/product/
```

Pour comprendre le périmètre et l'ordre d'implémentation :

```text
docs/milestones/
```

Pour comprendre les règles techniques actuelles :

```text
docs/architecture/
```

Pour comprendre pourquoi une décision architecturale existe :

```text
docs/architecture/adr/
```

Pour une règle métier :

```text
02 - DomainModel.md
```

Pour une intention UX ou de navigation :

```text
04 - ApplicationBlueprint.md
05 - UXArchitecture.md
006 - ApplicationShell.md
007 - NavigationArchitecture.md
```

Pour une règle technique générale :

```text
000 - SolutionArchitecture.md
```

Pour une convention de code :

```text
001 - BackendConventions.md
002 - FrontendConventions.md
003 - APIConventions.md
004 - PersistenceConventions.md
005 - TestingStrategy.md
```

Pour l'infrastructure locale et distante :

```text
008 - PostgreSQL.md
009 - Entity Framework Core.md
010 - Environments.md
011 - Docker.md
012 - CI-CD.md
013 - FirstDeployment.md
```

---

# 19. Parcours minimal d'un nouveau développeur

Depuis un clone neuf, le parcours minimal est :

```text
Installer les prérequis
        ↓
Cloner SignalLab
        ↓
Créer .env depuis .env.example
        ↓
Définir un mot de passe PostgreSQL local
        ↓
docker compose up --build -d
        ↓
docker compose ps
        ↓
PostgreSQL healthy
        ↓
GET http://localhost:8080/health
        ↓
200 Healthy
        ↓
GET http://localhost:3000/home
        ↓
200
        ↓
Shell SignalLab visible
```

Pour travailler ensuite hors Docker :

```text
Configurer ConnectionStrings:Postgres dans User Secrets
        ↓
docker compose up -d postgres
        ↓
dotnet run
        ↓
npm run dev
```

Pour proposer une modification :

```text
Créer une branche de Spec
        ↓
Développer
        ↓
Valider localement
        ↓
Push
        ↓
Pull Request vers main
        ↓
Backend + Frontend verts
        ↓
Merge
        ↓
Synchroniser main
        ↓
Supprimer la branche de Spec
```

---

# 20. Références

- `000 - SolutionArchitecture.md`
- `001 - BackendConventions.md`
- `002 - FrontendConventions.md`
- `003 - APIConventions.md`
- `004 - PersistenceConventions.md`
- `005 - TestingStrategy.md`
- `006 - ApplicationShell.md`
- `007 - NavigationArchitecture.md`
- `008 - PostgreSQL.md`
- `009 - Entity Framework Core.md`
- `010 - Environments.md`
- `011 - Docker.md`
- `012 - CI-CD.md`
- `013 - FirstDeployment.md`
- `docs/architecture/adr/000 - ADRIndex.md`
- `docs/product/06 - SoftwareArchitecture.md`
- `docs/milestones/M - Milestones.md`