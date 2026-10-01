# SignalLab — Environments

## 1. Objectif

SignalLab définit trois environnements d'exécution :

- `Development` ;
- `Staging` ;
- `Production`.

Toute autre valeur est considérée comme une erreur de configuration.

La configuration doit échouer dès le démarrage lorsqu'un environnement ou une valeur obligatoire est invalide.

---

# 2. Environnements supportés

## Development

`Development` correspond à l'environnement de développement local.

Il utilise notamment :

- les fichiers `appsettings.json` et `appsettings.Development.json` ;
- .NET User Secrets pour les secrets locaux de l'API ;
- PostgreSQL local via Docker Compose ;
- les launch profiles de `SignalLab.Api`.

Les secrets Development ne sont jamais stockés dans le repository.

## Staging

`Staging` correspond à l'environnement déployé utilisé pour valider l'application avant Production.

Les secrets sont injectés par l'environnement d'exécution.

Staging ne dépend pas de .NET User Secrets.

## Production

`Production` correspond à l'environnement réel de SignalLab.

Comme Staging, ses secrets sont injectés par l'environnement d'exécution et ne sont jamais présents dans le repository.

---

# 3. Sélection de l'environnement

ASP.NET Core utilise :

```text
ASPNETCORE_ENVIRONMENT
```

Les valeurs acceptées par SignalLab sont exclusivement :

```text
Development
Staging
Production
```

Au démarrage, `EnvironmentConfiguration` valide explicitement cette valeur.

Un environnement inconnu provoque immédiatement une erreur explicite.

Exemple :

```text
Unsupported environment 'Banana'. Supported environments are: Development, Staging, Production.
```

Cette validation empêche l'application de démarrer avec une configuration accidentelle ou ambiguë.

---

# 4. Sources de configuration

La configuration ASP.NET Core suit les sources standards de la plateforme.

Conceptuellement :

```text
appsettings.json
        ↓
appsettings.{Environment}.json
        ↓
User Secrets en Development
        ↓
variables d'environnement
```

Une source chargée plus tard peut remplacer une valeur fournie par une source précédente.

Les fichiers `appsettings` ne doivent contenir que des valeurs non sensibles.

Des fichiers spécifiques à Staging ou Production ne sont créés que lorsqu'un override non sensible est réellement nécessaire.

---

# 5. Secrets Development

Les secrets locaux de l'API sont stockés avec .NET User Secrets.

Le projet concerné est :

```text
apps/api/src/SignalLab.Api/SignalLab.Api.csproj
```

Le `UserSecretsId` présent dans le projet n'est pas un secret.

Il identifie uniquement le store local utilisé par .NET.

Configurer la chaîne PostgreSQL locale :

```powershell
dotnet user-secrets set `
  "ConnectionStrings:Postgres" `
  "<local-postgres-connection-string>" `
  --project apps\api\src\SignalLab.Api\SignalLab.Api.csproj
```

Vérifier les clés configurées :

```powershell
dotnet user-secrets list `
  --project apps\api\src\SignalLab.Api\SignalLab.Api.csproj
```

Les valeurs User Secrets sont stockées hors du repository.

---

# 6. Docker Compose et `.env`

Le fichier `.env` à la racine appartient à la configuration locale de Docker Compose.

Il contient les paramètres nécessaires au service PostgreSQL :

```text
POSTGRES_DB
POSTGRES_USER
POSTGRES_PASSWORD
POSTGRES_PORT
```

`.env` est ignoré par Git.

Le repository fournit uniquement :

```text
.env.example
```

avec des valeurs fictives.

La chaîne de connexion de l'API n'est pas dupliquée dans `.env` en Development.

Cette séparation évite de maintenir indépendamment :

```text
POSTGRES_PASSWORD
```

et :

```text
ConnectionStrings:Postgres
```

dans le même fichier local.

---

# 7. Configuration Staging et Production

Staging et Production reçoivent les valeurs sensibles depuis leur environnement d'exécution.

Une variable ASP.NET Core peut représenter une clé hiérarchique en utilisant `__`.

Ainsi :

```text
ConnectionStrings__Postgres
```

correspond à :

```text
ConnectionStrings:Postgres
```

Cette convention est utilisée par l'infrastructure de déploiement.

Les configurations de staging utilisent notamment cette convention pour :

```text
ConnectionStrings__Postgres
Authentication__Clerk__Issuer
Authentication__Clerk__AuthorizedParties__0
Cors__AllowedOrigins__0
```

Les valeurs exactes dépendent de l'environnement et ne doivent pas être dupliquées inutilement dans le repository.

Aucun secret Staging ou Production ne doit être commité.

---

# 8. Validation PostgreSQL

`ConnectionStrings:Postgres` est obligatoire.

La configuration échoue au démarrage lorsque la chaîne est :

- absente ;
- vide ;
- composée uniquement d'espaces ;
- syntaxiquement invalide.

Une chaîne PostgreSQL valide doit également définir :

- `Host` ;
- `Database` ;
- `Username`.

Exemples d'erreurs :

```text
Required configuration 'ConnectionStrings:Postgres' is missing or empty.
```

```text
Configuration 'ConnectionStrings:Postgres' is not a valid PostgreSQL connection string.
```

```text
Configuration 'ConnectionStrings:Postgres' must define a Host.
```

La validation ne tente pas de contacter PostgreSQL.

Elle vérifie la configuration, pas la disponibilité de l'infrastructure.

---

# 9. Configuration Authentication et CORS

L'authentification Clerk nécessite côté API :

```text
Authentication:Clerk:Issuer
Authentication:Clerk:AuthorizedParties
```

Issuer identifie l'émetteur Clerk accepté par l'API.

AuthorizedParties définit les origines Frontend dont le claim JWT azp est accepté.

En Development, ces valeurs peuvent provenir de appsettings.Development.json.

En Staging et Production, elles sont injectées par l'environnement d'exécution.

L'accès cross-origin à l'API utilise séparément :

```text
Cors:AllowedOrigins
```

Cette configuration contrôle les origines auxquelles ASP.NET Core autorise le navigateur à exposer les réponses de l'API.

AuthorizedParties et AllowedOrigins répondent donc à deux responsabilités différentes :
- AuthorizedParties participe à la validation de l'identité portée par le JWT ;
- AllowedOrigins applique la politique CORS HTTP du Backend.

Les deux configurations doivent rester cohérentes avec les domaines Frontend de l'environnement concerné.

L'API échoue au démarrage lorsque `Authentication:Clerk:Issuer`, `Authentication:Clerk:AuthorizedParties` ou `Cors:AllowedOrigins` est absent ou vide selon les règles attendues.

---

# 10. Validation fail-fast

Les validations sont exécutées pendant la composition de l'application.

Le démarrage suit conceptuellement :

```text
Créer le WebApplicationBuilder
↓
Valider l'environnement
↓
Valider et enregistrer Infrastructure
↓
Construire l'application
↓
Démarrer le serveur HTTP
```

Une configuration invalide empêche donc SignalLab de démarrer.

L'objectif est qu'une erreur d'exploitation soit détectée immédiatement plutôt qu'au premier accès à une fonctionnalité dépendante.

---

# 11. Gestion des secrets

Aucun secret ne doit être présent dans Git.

Cela comprend notamment :

- mots de passe PostgreSQL réels ;
- chaînes de connexion réelles ;
- clés d'API ;
- tokens ;
- secrets de signature ;
- futures clés d'Identity Provider.

Les valeurs d'exemple doivent être manifestement fictives.

Les tests peuvent utiliser des valeurs telles que :

```text
Password=test
```

lorsqu'elles servent uniquement à tester le parsing d'une configuration et ne correspondent à aucun environnement réel.

---

# 12. Matrice des environnements

| Configuration                              | Development    | Staging          | Production         |
|--------------------------------------------|----------------|------------------|--------------------|
| `ASPNETCORE_ENVIRONMENT`                   | `Development`  | `Staging`        | `Production`       |
| `appsettings.json`                         | Oui            | Oui              | Oui                |
| Overrides `appsettings.{Environment}.json` | Si nécessaire  | Si nécessaire    | Si nécessaire      |
| .NET User Secrets                          | Oui            | Non              | Non                |
| Variables d'environnement                  | Possible       | Oui              | Oui                |
| Secrets dans Git                           | Jamais         | Jamais           | Jamais             |
| PostgreSQL                                 | Local Docker   | Injecté par l'environnement | Injecté par l'environnement |
| Clerk                                      | Development instance | Staging configuration | Production configuration |
| CORS origins                               | Local frontend | Staging frontend | Production frontend |

---

# 13. Tests

Les tests automatisés vérifient notamment :

- `Development` accepté ;
- `Staging` accepté ;
- `Production` accepté ;
- environnement inconnu refusé ;
- chaîne PostgreSQL absente refusée ;
- chaîne vide refusée ;
- chaîne syntaxiquement invalide refusée ;
- absence de `Host` refusée ;
- absence de `Database` refusée ;
- absence de `Username` refusée ;
- chaîne PostgreSQL valide acceptée ;
- configuration Clerk valide acceptée ;
- `Authentication:Clerk:Issuer` absent refusé ;
- `Authentication:Clerk:Issuer` vide refusé ;
- `Authentication:Clerk:AuthorizedParties` absent refusé ;
- `Authentication:Clerk:AuthorizedParties` vide refusé ;
- configuration CORS valide acceptée ;
- `Cors:AllowedOrigins` absent refusé ;
- `Cors:AllowedOrigins` vide refusé ;
- requête protégée sans token refusée avec `401 Unauthorized` ;
- JWT valide accepté ;
- JWT avec signature invalide refusé ;
- JWT expiré refusé ;
- JWT avec issuer invalide refusé ;
- JWT sans claim `azp` refusé ;
- JWT avec `azp` non autorisé refusé ;
- identité externe `sub` conservée et accessible après validation du JWT ;
- preflight CORS depuis une origine autorisée accepté ;
- preflight CORS depuis une origine non autorisée refusé.

Les validations ont également été vérifiées manuellement en exécutant réellement l'API dans les différents environnements.

---

# 14. Responsabilités futures

SL-018 définit les conventions d'environnement.

Elle ne configure pas encore :

- les images Docker du frontend et du backend ;
- le démarrage complet de la stack avec Docker Compose ;
- la CI/CD ;
- l'infrastructure de Staging ;
- l'infrastructure de Production ;
- les secrets des futurs services métier.

La conteneurisation complète appartient à SL-019.

Les environnements finaux seront consolidés lors de la Milestone Deployment and Operations sans redéfinir les conventions établies ici.

---

# 15. État à la fin de SL-018

À la fin de SL-018 :

- `Development`, `Staging` et `Production` sont explicitement supportés ;
- tout environnement inconnu est refusé ;
- la configuration PostgreSQL est validée au démarrage ;
- les erreurs de configuration sont explicites ;
- Development utilise .NET User Secrets ;
- Staging et Production sont prêts à recevoir leurs secrets depuis l'environnement d'exécution ;
- `.env` est réservé à la configuration Docker locale ;
- aucun secret réel n'est versionné ;
- les comportements sont couverts par les tests automatisés.