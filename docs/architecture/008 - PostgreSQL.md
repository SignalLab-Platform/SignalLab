# SignalLab — PostgreSQL

## 1. Objectif

PostgreSQL constitue la base de données relationnelle principale de SignalLab et la source persistante de vérité pour les données métier.

L'environnement de développement utilise PostgreSQL via Docker Compose afin de fournir une configuration locale reproductible.

---

# 2. Service PostgreSQL

Le service PostgreSQL est défini dans le fichier :

```text
compose.yaml
```

à la racine du repository.

Configuration initiale :

- PostgreSQL `18.6` ;
- port PostgreSQL interne `5432` ;
- port local configurable, `5432` par défaut ;
- healthcheck avec `pg_isready` ;
- redémarrage `unless-stopped` ;
- volume Docker persistant dédié.

Le service Compose est nommé :

```text
postgres
```

Il est donc accessible depuis les autres services Docker du même réseau via :

```text
postgres:5432
```

Depuis le système hôte, PostgreSQL est accessible via :

```text
localhost:5432
```

---

# 3. Configuration locale

Les valeurs locales sont définies dans :

```text
.env
```

Ce fichier contient notamment :

```text
POSTGRES_DB
POSTGRES_USER
POSTGRES_PASSWORD
POSTGRES_PORT
ConnectionStrings__Postgres
```

Le fichier `.env` contient des valeurs locales et des secrets de développement.

Il ne doit jamais être commité.

Le repository fournit à la place :

```text
.env.example
```

qui décrit les variables nécessaires sans contenir de secret réel.

---

# 4. Chaîne de connexion de l'API

L'API utilise la clé de configuration ASP.NET Core :

```text
ConnectionStrings:Postgres
```

Cette valeur n'est pas stockée dans `.env` en développement local.

En `Development`, elle est fournie par .NET User Secrets.

En `Staging` et `Production`, elle est fournie par l'environnement d'exécution. Une variable d'environnement ASP.NET Core peut utiliser :

```text
ConnectionStrings__Postgres
```

qui est automatiquement interprété comme :

```text
ConnectionStrings:Postgres
```

Lorsque l'API est exécutée directement sur la machine de développement, PostgreSQL est accessible via :

```text
localhost:5432
```

Lorsqu'un service sera exécuté dans le réseau Docker Compose, PostgreSQL sera accessible via :

```text
postgres:5432
```

La gestion détaillée des environnements et des secrets est définie dans `010 - Environments.md`.

---

# 5. Démarrage

Depuis la racine du repository :

```powershell
docker compose up -d postgres
```

Vérifier l'état du service :

```powershell
docker compose ps
```

Le service doit atteindre l'état :

```text
healthy
```

---

# 6. Vérification PostgreSQL

Vérifier que PostgreSQL accepte les connexions :

```powershell
docker compose exec postgres pg_isready -U signallab -d signallab
```

Interroger directement la base :

```powershell
docker compose exec postgres `
  psql `
  -X `
  -P pager=off `
  -U signallab `
  -d signallab `
  -c "SELECT current_database(), current_user, version();"
```

La base et l'utilisateur attendus en développement sont :

```text
Database : signallab
User     : signallab
```

---

# 7. Connexion depuis l'hôte

Le port publié peut être vérifié depuis Windows avec :

```powershell
Test-NetConnection 127.0.0.1 -Port 5432
```

Le résultat attendu est :

```text
TcpTestSucceeded : True
```

Cette vérification confirme que PostgreSQL est joignable depuis les processus exécutés localement sur la machine de développement.

---

# 8. Persistance

PostgreSQL utilise le volume Docker nommé :

```text
signallab_postgres_data
```

Les données persistent donc indépendamment du cycle de vie du conteneur.

La commande :

```powershell
docker compose down
```

supprime le conteneur et le réseau mais conserve les données.

Après :

```powershell
docker compose up -d postgres
```

PostgreSQL retrouve le même volume et les données précédemment stockées.

Cette persistance a été validée en créant une donnée technique temporaire, en supprimant puis recréant le conteneur, et en vérifiant que la donnée était toujours présente.

La donnée de validation a ensuite été supprimée.

---

# 9. Réinitialisation locale

Pour supprimer volontairement la base de développement et son volume :

```powershell
docker compose down -v
```

Cette commande supprime également :

```text
signallab_postgres_data
```

Elle doit être utilisée uniquement lorsqu'une réinitialisation complète des données locales est souhaitée.

Au prochain :

```powershell
docker compose up -d postgres
```

PostgreSQL initialise une nouvelle base avec les valeurs actuelles du `.env`.

---

# 10. Mot de passe PostgreSQL

`POSTGRES_PASSWORD` est utilisé par l'image PostgreSQL lors de l'initialisation d'un nouveau volume Docker.

Modifier ensuite cette valeur dans `.env` ne modifie pas automatiquement le mot de passe du rôle PostgreSQL déjà enregistré dans un volume existant.

En développement local, l'API possède séparément sa chaîne de connexion PostgreSQL dans .NET User Secrets.

Les informations d'identification utilisées par PostgreSQL et celles configurées pour l'API doivent donc correspondre, sans être stockées ensemble dans un fichier versionné.

Lorsqu'une base locale encore jetable doit être réinitialisée avec les valeurs actuelles du `.env` :

```powershell
docker compose down -v
docker compose up -d postgres
```

Si les informations d'identification PostgreSQL ont changé, la chaîne `ConnectionStrings:Postgres` conservée dans .NET User Secrets doit également être mise à jour.

---

# 11. Responsabilités

PostgreSQL garantit la persistence durable des données de SignalLab.

Cette Spec ne définit pas :

- le mapping des entités C# ;
- le `DbContext` ;
- les migrations de schéma ;
- les repositories ;
- les environnements Staging et Production ;
- la conteneurisation complète du frontend et du backend.

Ces responsabilités appartiennent aux Specs suivantes.

---

# 12. État à la fin de SL-016

À la fin de SL-016 :

- PostgreSQL 18.6 fonctionne via Docker Compose ;
- la connexion locale fonctionne via `localhost:5432` ;
- le réseau Docker fournit le hostname `postgres` ;
- la chaîne de connexion est externalisée ;
- aucun secret local n'est versionné ;
- les données survivent à la recréation du conteneur ;
- PostgreSQL constitue la base persistante de référence prête à être utilisée par Entity Framework Core.