# SignalLab — Docker

## 1. Objectif

SignalLab utilise Docker pour fournir un environnement de développement complet, isolé et reproductible.

La stack Docker locale regroupe :

- le frontend Next.js ;
- l'API ASP.NET Core ;
- PostgreSQL.

L'objectif est qu'un environnement SignalLab complet puisse être construit et démarré depuis le repository avec une commande unique.

---

# 2. Architecture de la stack

Docker Compose orchestre trois services :

```text
web
 |
 | HTTP
 |
api
 |
 | PostgreSQL
 |
postgres
```

Les services sont :

| Service | Technologie | Port conteneur | Port hôte |
|---|---|---:|---:|
| `web` | Next.js 16 | 3000 | 3000 |
| `api` | ASP.NET Core / .NET 10 | 8080 | 8080 |
| `postgres` | PostgreSQL 18.6 | 5432 | configurable, 5432 par défaut |

Le navigateur accède à :

```text
http://localhost:3000
```

pour le frontend et :

```text
http://localhost:8080
```

pour l'API locale conteneurisée.

---

# 3. Docker Compose

Le fichier Compose principal est :

```text
compose.yaml
```

Il décrit :

- les trois services ;
- leurs images ou builds ;
- leurs ports ;
- leurs variables d'environnement ;
- leurs dépendances de démarrage ;
- le réseau Docker ;
- le volume PostgreSQL.

Le fichier doit pouvoir être validé avec :

```powershell
docker compose config --quiet
```

Les services déclarés peuvent être inspectés avec :

```powershell
docker compose config --services
```

Résultat attendu :

```text
postgres
api
web
```

---

# 4. Images applicatives

## 4.1 API

L'image de l'API est construite depuis :

```text
apps/api/Dockerfile
```

Son contexte de build est :

```text
apps/api
```

Le Dockerfile utilise un build multi-stage.

### Build

Le stage de build utilise le SDK .NET 10.

Il :

1. copie les sources de l'API ;
2. restaure les dépendances ;
3. publie `SignalLab.Api` en configuration `Release`.

### Runtime

Le stage final utilise uniquement le runtime ASP.NET Core.

L'API :

- écoute en HTTP sur le port `8080` ;
- s'exécute avec l'utilisateur non-root `app` ;
- ne contient pas le SDK .NET ;
- ne contient aucun secret de développement.

L'image locale est nommée :

```text
signallab-api:local
```

---

# 5. Image frontend

L'image frontend est construite depuis :

```text
apps/web/Dockerfile
```

Son contexte de build est :

```text
apps/web
```

Le frontend utilise Node.js 24.18.0.

Le Dockerfile utilise trois stages.

## Dependencies

Les dépendances sont installées avec :

```text
npm ci
```

à partir de :

```text
package.json
package-lock.json
```

Le lockfile garantit une installation reproductible des dépendances.

## Build

Le build exécute :

```text
npm run build
```

Next.js est configuré avec :

```text
output: "standalone"
```

Le runtime nécessaire est ainsi extrait dans :

```text
.next/standalone
```

## Runtime

L'image finale contient uniquement les éléments nécessaires à l'exécution de Next.js :

- le runtime standalone ;
- `public` ;
- `.next/static`.

Le frontend :

- écoute sur `0.0.0.0:3000` ;
- s'exécute avec l'utilisateur non-root `node` ;
- n'embarque pas le `node_modules` complet utilisé pendant le build.

L'image locale est nommée :

```text
signallab-web:local
```

---

# 6. `.dockerignore`

L'API et le frontend disposent chacun d'un `.dockerignore`.

Ces fichiers empêchent notamment l'inclusion dans les contextes de build de sorties locales telles que :

```text
bin
obj
node_modules
.next
coverage
```

ainsi que des fichiers locaux ou sensibles non nécessaires au build.

Un build Docker reconstruit donc l'application depuis ses sources plutôt que de réutiliser des artefacts produits sur la machine hôte.

---

# 7. Réseau Docker

Compose crée un réseau explicite :

```text
signallab
```

de type :

```text
bridge
```

Les trois services appartiennent à ce réseau.

Docker fournit une résolution DNS interne basée sur le nom des services.

L'API accède donc à PostgreSQL avec :

```text
postgres:5432
```

et non :

```text
localhost:5432
```

`localhost` à l'intérieur d'un conteneur désigne ce conteneur lui-même.

Le réseau effectivement créé par Compose porte un nom préfixé par le projet, par exemple :

```text
signallab_signallab
```

---

# 8. PostgreSQL

Le service PostgreSQL utilise :

```text
postgres:18.6
```

Sa configuration locale est fournie par le fichier `.env` :

```text
POSTGRES_DB
POSTGRES_USER
POSTGRES_PASSWORD
POSTGRES_PORT
```

`.env` n'est pas versionné.

`.env.example` fournit uniquement des valeurs d'exemple non sensibles.

---

# 9. Volume PostgreSQL

PostgreSQL utilise le volume nommé :

```text
postgres_data
```

monté dans :

```text
/var/lib/postgresql
```

Ce volume constitue la persistence locale de la base.

Un arrêt normal de la stack :

```powershell
docker compose down
```

supprime les conteneurs et le réseau, mais conserve le volume.

Les données sont donc retrouvées lors du prochain :

```powershell
docker compose up -d
```

Ce comportement a été validé en créant une donnée temporaire dans PostgreSQL, en détruisant puis recréant les conteneurs et en vérifiant que cette donnée était toujours présente.

La donnée de test a ensuite été supprimée.

---

# 10. Réinitialisation explicite de PostgreSQL

La suppression du volume est volontaire et destructive.

Pour supprimer également les données locales :

```powershell
docker compose down -v
```

Le prochain démarrage initialise alors un nouveau volume PostgreSQL à partir des valeurs courantes de `.env`.

Cette commande ne doit pas être utilisée pour un arrêt normal de l'environnement.

---

# 11. Ordre de démarrage

PostgreSQL possède un healthcheck basé sur :

```text
pg_isready
```

L'API déclare :

```text
depends_on
    postgres
        condition: service_healthy
```

L'API n'est donc démarrée qu'après que PostgreSQL est considéré comme disponible par son healthcheck.

Le frontend ne dépend pas actuellement du démarrage de l'API.

Aucune dépendance artificielle n'est ajoutée tant que le frontend ne nécessite pas effectivement l'API pour démarrer.

---

# 12. Configuration de l'API dans Docker

La stack Docker locale utilise :

```text
ASPNETCORE_ENVIRONMENT=Development
```

La chaîne PostgreSQL est construite par Compose à partir des variables de `.env`.

Elle est injectée avec la clé :

```text
ConnectionStrings__Postgres
```

qui correspond dans la configuration ASP.NET Core à :

```text
ConnectionStrings:Postgres
```

La chaîne utilise :

```text
Host=postgres
Port=5432
```

car l'API et PostgreSQL communiquent sur le réseau Docker.

---

# 13. Development hors Docker et dans Docker

Deux workflows de développement restent possibles.

## Exécution locale

L'API exécutée directement sur Windows utilise :

```text
Development
+
.NET User Secrets
+
Host=localhost
```

PostgreSQL peut alors être lancé seul avec Docker.

## Stack Docker complète

L'API conteneurisée utilise :

```text
Development
+
variables injectées par Docker Compose
+
Host=postgres
```

Les User Secrets de la machine hôte ne sont pas nécessaires au conteneur.

Les deux workflows restent indépendants.

---

# 14. HTTP et HTTPS

Le conteneur API expose uniquement HTTP sur :

```text
8080
```

La redirection HTTPS ASP.NET Core est désactivée explicitement dans Compose avec :

```text
HttpsRedirection__Enabled=false
```

Hors Docker, la configuration conserve par défaut la redirection HTTPS.

Cette distinction évite d'embarquer des certificats de développement dans l'image Docker.

La terminaison HTTPS d'un futur environnement public appartient à l'infrastructure de déploiement et n'est pas configurée dans SL-019.

---

# 15. Démarrage de la stack

## Premier démarrage ou reconstruction

Depuis la racine du repository :

```powershell
docker compose up --build -d
```

Cette commande :

1. lit la configuration Docker locale ;
2. construit l'image de l'API ;
3. construit l'image du frontend ;
4. crée le réseau ;
5. crée ou réutilise le volume PostgreSQL ;
6. démarre PostgreSQL ;
7. attend son état healthy avant de démarrer l'API ;
8. démarre le frontend.

Elle constitue la commande reproductible principale de SignalLab pour l'environnement Docker de développement.

---

# 16. Démarrage quotidien

Lorsque les images sont déjà construites :

```powershell
docker compose up -d
```

suffit à recréer et démarrer les services.

Pour arrêter :

```powershell
docker compose down
```

Lorsque le code ou les dépendances nécessitent une nouvelle image :

```powershell
docker compose up --build -d
```

---

# 17. Inspection

L'état de la stack peut être consulté avec :

```powershell
docker compose ps
```

PostgreSQL doit apparaître comme :

```text
healthy
```

Les logs peuvent être consultés individuellement :

```powershell
docker compose logs postgres
docker compose logs api
docker compose logs web
```

---

# 18. Vérification fonctionnelle

## API

L'endpoint de santé local est accessible avec :

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

## Frontend

Le frontend peut être vérifié avec :

```powershell
Invoke-WebRequest `
  http://localhost:3000 `
  -UseBasicParsing
```

Résultat attendu :

```text
StatusCode : 200
```

L'Application Shell est alors accessible depuis :

```text
http://localhost:3000
```

---

# 19. Migrations Entity Framework Core

SL-019 ne modifie pas la stratégie de migrations.

L'API n'exécute pas automatiquement :

```text
Database.Migrate()
```

à son démarrage.

Les migrations restent des opérations explicites et contrôlées.

La manière de les appliquer dans les futurs environnements de déploiement sera définie lorsque cette responsabilité sera introduite.

---

# 20. Validation

SL-019 a été validée avec les contrôles suivants.

## Docker

- build isolé de l'image API ;
- démarrage isolé de l'image API ;
- endpoint `/health` de l'API accessible ;
- build isolé du frontend ;
- démarrage isolé du frontend ;
- frontend accessible en HTTP ;
- construction conjointe des images avec Compose ;
- démarrage simultané de `web`, `api` et `postgres` ;
- healthcheck PostgreSQL réussi ;
- démarrage de l'API après PostgreSQL ;
- redémarrage complet après `docker compose down` ;
- persistence PostgreSQL vérifiée entre deux créations de conteneurs ;
- configuration Compose validée.

## Backend

```text
24 tests réussis
0 échec
```

Le build Release de la solution .NET est également valide.

## Frontend

```text
15 suites réussies
55 tests réussis
0 échec
```

Les contrôles suivants sont également valides :

```text
ESLint
TypeScript
Next.js production build
```

---

# 21. Limites de SL-019

SL-019 configure uniquement l'environnement Docker nécessaire au développement.

Elle n'introduit pas :

- GitHub Actions ;
- registry d'images ;
- déploiement Staging ;
- déploiement Production ;
- reverse proxy ;
- terminaison TLS publique ;
- migrations automatiques ;
- orchestration de production ;
- Kubernetes.

La CI/CD appartient à SL-020.

Le premier déploiement appartient à SL-021.

Les environnements opérationnels complets seront approfondis lors de la Milestone Deployment and Operations.

---

# 22. État à la fin de SL-019

À la fin de SL-019 :

- le frontend possède une image Docker reproductible ;
- l'API possède une image Docker reproductible ;
- PostgreSQL est intégré à la même stack ;
- les trois services démarrent ensemble ;
- PostgreSQL conserve ses données dans un volume dédié ;
- les services partagent un réseau Docker explicite ;
- les secrets locaux restent hors du repository ;
- l'API utilise le hostname Docker `postgres` pour accéder à la base ;
- le démarrage complet peut être effectué avec une commande ;
- l'environnement Docker peut être détruit puis recréé sans perdre les données PostgreSQL.