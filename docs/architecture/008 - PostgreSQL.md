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

# 4. Chaîne de connexion

La chaîne de connexion ASP.NET Core est externalisée sous la clé :

```text
ConnectionStrings__Postgres
```

ASP.NET Core interprète cette variable comme :

```text
ConnectionStrings:Postgres
```

Pour une API exécutée directement sur la machine de développement, l'hôte PostgreSQL est :

```text
localhost
```

Pour un service exécuté dans le réseau Docker Compose, l'hôte PostgreSQL est :

```text
postgres
```

La configuration spécifique des environnements Development, Staging et Production appartient à SL-018.

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

`POSTGRES_PASSWORD` est utilisé lors de l'initialisation initiale du volume PostgreSQL.

Modifier ensuite cette valeur dans `.env` ne modifie pas automatiquement le mot de passe du rôle PostgreSQL déjà stocké dans un volume existant.

Les valeurs :

```text
POSTGRES_PASSWORD
```

et le mot de passe contenu dans :

```text
ConnectionStrings__Postgres
```

doivent rester cohérentes.

Lorsqu'une base locale encore vide doit être réalignée avec une nouvelle configuration, le volume peut être recréé avec :

```powershell
docker compose down -v
docker compose up -d postgres
```

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