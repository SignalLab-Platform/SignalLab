# First Deployment

## 1. Objectif

Ce document décrit le premier déploiement de SignalLab.

Ce déploiement valide la capacité de la Platform Foundation à être exécutée
dans un environnement distant avant l'introduction des domaines métier.

Il couvre :

- le frontend Next.js ;
- l'API ASP.NET Core ;
- PostgreSQL ;
- la configuration Staging ;
- HTTPS ;
- les domaines de staging ;
- les règles minimales de sécurité et d'indexation.

Le fournisseur utilisé pour ce premier déploiement est Render.

Render constitue un choix d'hébergement et non une dépendance architecturale
de SignalLab.

---

## 2. Environnement

Le premier environnement distant est :

```text
Staging
```

Il utilise les domaines suivants :

```text
Frontend
https://staging.signallab.dev

API
https://api.staging.signallab.dev
```

Les domaines signallab.dev sont réservés aux environnements techniques.
Les futurs domaines publics de production reposent sur signallab.fr.

---

## 3. Architecture déployée

Le staging exécute trois ressources principales :

```text
Browser
   ↓
staging.signallab.dev
   ↓
Next.js
```

```text
Client
   ↓
api.staging.signallab.dev
   ↓
ASP.NET Core
   ↓
Entity Framework Core
   ↓
PostgreSQL
```

Le frontend et l'API utilisent les Dockerfiles versionnés dans le repository.
Docker Compose reste destiné au développement local et n'est pas utilisé
comme mécanisme de déploiement Render.

---

## 4. PostgreSQL

PostgreSQL est hébergé par Render dans le même environnement de staging.
L'API utilise la connexion privée interne à Render lorsque cela est possible.
La chaîne de connexion est fournie à l'application par :

```text
ConnectionStrings__Postgres
```

La chaîne de connexion et les credentials ne sont jamais stockés dans Git.
L'External Database URL est réservée aux opérations administratives et aux
validations ponctuelles depuis l'extérieur de Render.
La connexion PostgreSQL distante a été validée explicitement avec psql.
Les migrations Entity Framework Core ne sont pas automatiquement exécutées
au démarrage de l'API.
Leur application reste une opération explicite et contrôlée.

---

## 5. API

L'API est exécutée dans un conteneur ASP.NET Core.
La configuration Render utilise :

```text
ASPNETCORE_ENVIRONMENT=Staging
HttpsRedirection__Enabled=false
ConnectionStrings__Postgres=<secret Render>
```

HttpsRedirection__Enabled est désactivé dans le conteneur car TLS est
terminé par Render avant que la requête atteigne Kestrel.
L'API reste publiquement accessible en HTTPS.
Le health endpoint est disponible à :

```text
GET https://api.staging.signallab.dev/health
```

Réponse attendue :

```
HTTP 200
Healthy
```

Le health endpoint valide actuellement la disponibilité de l'API.
La connectivité PostgreSQL a été validée séparément et ne dépend pas de ce
health endpoint.

---

## 6. Frontend

Le frontend Next.js est exécuté à partir de son image Docker standalone.
Render fournit dynamiquement le port HTTP utilisé par le conteneur.
Le frontend est publiquement accessible à :

```text
https://staging.signallab.dev
```

Le premier déploiement validé utilise :

```text
Next.js 16.3.7
React 19.2.4
Node.js 24.18.0
```

Le frontend ne possède pas encore de configuration d'URL API car la Platform
Foundation n'effectue pas encore de requêtes métier vers l'API.
Cette configuration sera introduite lorsqu'une Milestone fonctionnelle en aura
réellement besoin.

---

## 7. Indexation du staging

L'environnement de staging ne doit pas être indexé par les moteurs de
recherche.
Le frontend ajoute donc l'en-tête HTTP :

```text
X-Robots-Tag: noindex, nofollow
```

lorsque le hostname correspond à :

```text
staging.signallab.dev
*.onrender.com
```

Cette règle est implémentée dans :

```text
apps/web/src/proxy.ts
```

Elle ne s'applique pas au futur domaine de production :

```text
app.signallab.fr
```

La séparation a été vérifiée localement en simulant les deux hostnames puis
sur le staging public.

---

## 8. DNS

Les DNS sont actuellement gérés chez Porkbun.
Le frontend utilise un CNAME :

```text
staging.signallab.dev
→ signallab-z6ht.onrender.com
```

L'API utilise un CNAME :

```text
api.staging.signallab.dev
→ signallab-api-staging.onrender.com
```

Les cibles Render sont des détails d'infrastructure et peuvent évoluer sans
modifier les URLs publiques de staging.

---

## 9. Secrets

Aucun secret de staging ne doit être ajouté au repository.
Les secrets nécessaires au déploiement sont stockés dans la configuration de
l'environnement Render.
Cela concerne notamment :

```text
ConnectionStrings__Postgres
```

Les URLs contenant des credentials PostgreSQL sont également considérées
comme des secrets.
En cas d'exposition accidentelle, les credentials concernés doivent être
immédiatement renouvelés.

---

## 10. Validation

Le frontend est validé avant déploiement avec :

```text
npm --prefix apps/web audit --omit=dev
npm --prefix apps/web run lint
npm --prefix apps/web run typecheck
npm --prefix apps/web test
npm --prefix apps/web run build
```

L'audit de production doit retourner :

```text
found 0 vulnerabilities
```

L'image Docker frontend peut être validée avec :

```text
docker build `
  --file apps/web/Dockerfile `
  --tag signallab-web:local `
  apps/web
```

Le frontend public peut être validé avec :

```text
curl.exe -I https://staging.signallab.dev/home
```

La réponse doit notamment contenir :

```text
HTTP/1.1 200
x-robots-tag: noindex, nofollow
```

L'API publique peut être validée avec :

```text
Invoke-WebRequest `
  https://api.staging.signallab.dev/health `
  -UseBasicParsing
```

La réponse attendue est :

```text
StatusCode : 200
Content    : Healthy
```

---

## 11. Déploiement depuis Git

Pendant la réalisation de SL-021, les services Render ont temporairement suivi
la branche :

```text
sl-021-first-deployment
```

Après validation et fusion de la Pull Request, les services de staging doivent
suivre :

```text
main
```

Le staging ne doit pas dépendre durablement d'une branche de feature.
Une fois les services configurés sur main, un nouveau déploiement doit être
effectué et les URLs publiques doivent être revalidées avant suppression de la
branche SL-021.

---

## 12. Limites actuelles

Ce premier déploiement ne constitue pas le déploiement de production du MVP.
Sont notamment hors scope de SL-021 :
- le déploiement de production ;
- les domaines publics signallab.fr ;
- le déploiement automatisé depuis GitHub Actions ;
- l'application automatique des migrations ;
- les fonctionnalités métier ;
- l'authentification ;
- l'observabilité de production avancée.

Ces responsabilités seront introduites uniquement par les Milestones qui les
possèdent.

---

## 13. Résultat

La Platform Foundation de SignalLab dispose désormais d'un environnement de
staging distant reproductible.
Le Shell est publiquement accessible.
L'API répond en HTTPS.
PostgreSQL est accessible depuis l'environnement distant.
Les secrets sont externalisés.
Le staging est exclu de l'indexation publique.
La même architecture pourra évoluer vers la production sans modifier les
responsabilités fondamentales du frontend, de l'API ou de la persistence.