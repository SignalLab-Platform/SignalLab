# SignalLab — CI/CD

## 1. Objectif

SignalLab utilise GitHub Actions pour automatiser la validation des modifications avant leur intégration dans `main`.

SL-020 met en place la partie Continuous Integration de la chaîne de livraison.

La CI doit garantir qu'une modification ne peut pas être fusionnée dans `main` si elle dégrade :

- le Backend ;
- le Frontend ;
- leurs tests ;
- leurs builds de production ;
- leurs images Docker.

Le déploiement n'appartient pas à SL-020.

---

# 2. Workflow GitHub Actions

Le workflow principal est :

```text
.github/workflows/ci.yml
```

Il est nommé :

```text
CI
```

Il contient deux jobs indépendants :

```text
Backend
Frontend
```

Cette séparation permet d'identifier immédiatement quelle partie de l'application échoue.

---

# 3. Déclenchement

Le workflow s'exécute lors d'une Pull Request ciblant :

```text
main
```

Il s'exécute également après une modification effectivement intégrée dans :

```text
main
```

Les événements concernés sont donc :

```yaml
pull_request:
  branches:
    - main

push:
  branches:
    - main
```

La Pull Request valide une modification avant fusion.

Le déclenchement sur `main` revalide ensuite l'état réellement intégré du repository.

---

# 4. Permissions

Le workflow utilise uniquement :

```yaml
permissions:
  contents: read
```

La CI peut lire le repository mais ne dispose d'aucun droit d'écriture nécessaire au déploiement ou à la publication.

Aucun secret applicatif n'est nécessaire à SL-020.

---

# 5. Validation Backend

Le job :

```text
Backend
```

s'exécute sur un runner GitHub :

```text
ubuntu-latest
```

Il effectue successivement :

1. le checkout du repository ;
2. l'installation du SDK .NET ;
3. la restauration des dépendances ;
4. le build Release ;
5. l'exécution des tests ;
6. la construction de l'image Docker Backend.

---

# 6. Version .NET

La version du SDK n'est pas dupliquée dans le workflow.

GitHub Actions utilise :

```text
global.json
```

comme source de vérité.

La configuration actuelle est :

```json
{
  "sdk": {
    "rollForward": "latestFeature",
    "version": "10.0.302"
  }
}
```

Le workflow utilise `actions/setup-dotnet` avec ce fichier.

Cette approche maintient une seule définition de la version .NET pour le développement local et la CI.

---

# 7. Contrôles Backend

La CI exécute :

```text
dotnet restore
```

puis :

```text
dotnet build
```

en configuration :

```text
Release
```

et enfin :

```text
dotnet test
```

Les tests utilisent le build précédemment produit afin d'éviter une compilation redondante.

À la validation de SL-020, la suite Backend contient :

```text
24 tests
```

tous réussis.

---

# 8. Image Docker Backend

Après les contrôles .NET, la CI construit :

```text
apps/api/Dockerfile
```

L'image temporaire est nommée :

```text
signallab-api:ci
```

Cette image n'est pas publiée dans un registry.

Son build permet uniquement de garantir que les modifications du Backend restent compatibles avec l'environnement Docker défini dans SL-019.

---

# 9. Validation Frontend

Le job :

```text
Frontend
```

s'exécute indépendamment du Backend sur :

```text
ubuntu-latest
```

Il effectue successivement :

1. le checkout du repository ;
2. l'installation de Node.js ;
3. l'installation des dépendances ;
4. ESLint ;
5. le typecheck TypeScript ;
6. les tests ;
7. le build Next.js de production ;
8. la construction de l'image Docker Frontend.

---

# 10. Version Node.js

La version Node.js utilisée par la CI est :

```text
24.18.0
```

Elle correspond à la version utilisée par l'environnement de développement et par l'image Docker Frontend.

---

# 11. Installation des dépendances Frontend

Les dépendances sont installées avec :

```text
npm ci
```

Le fichier :

```text
apps/web/package-lock.json
```

constitue la source déterministe des dépendances npm.

GitHub Actions utilise également le cache npm basé sur ce lockfile afin d'accélérer les exécutions suivantes sans modifier leur résultat.

---

# 12. Contrôles Frontend

La CI exécute :

```text
npm run lint
npm run typecheck
npm test
npm run build
```

À la validation de SL-020 :

```text
15 suites de tests
55 tests
```

réussissent.

Le build Next.js de production est également validé.

---

# 13. Image Docker Frontend

Après les contrôles applicatifs, la CI construit :

```text
apps/web/Dockerfile
```

L'image temporaire est nommée :

```text
signallab-web:ci
```

Elle n'est pas publiée.

Cette étape garantit que les modifications Frontend restent compatibles avec l'image standalone définie dans SL-019.

---

# 14. PostgreSQL

SL-020 ne démarre pas PostgreSQL dans GitHub Actions.

Les tests actuellement exécutés par la CI ne nécessitent pas de base PostgreSQL réelle.

Ajouter un service PostgreSQL sans test consommateur introduirait une dépendance et du temps d'exécution inutiles.

Un service PostgreSQL pourra être introduit lorsque des tests d'intégration nécessiteront effectivement une base réelle.

---

# 15. Protection de `main`

La branche :

```text
main
```

est protégée par un GitHub Ruleset.

La règle impose :

```text
Require a pull request before merging
```

ainsi que :

```text
Require status checks to pass
```

Les deux status checks obligatoires sont :

```text
Backend
Frontend
```

Une Pull Request ne peut donc être fusionnée que lorsque les deux jobs GitHub Actions ont réussi.

---

# 16. Synchronisation avec `main`

La règle :

```text
Require branches to be up to date before merging
```

n'est pas activée actuellement.

Le projet étant développé par un seul développeur, cette contrainte provoquerait des relances supplémentaires de la CI lorsque `main` évolue sans apporter de bénéfice suffisant à ce stade.

Cette décision pourra être réévaluée si le mode de collaboration change.

---

# 17. Validation du blocage de fusion

La protection de branche a été testée explicitement.

Un échec volontaire a été introduit temporairement dans le job Frontend.

Le résultat obtenu était :

```text
Backend  ✅
Frontend ❌
```

GitHub a alors interdit la fusion de la Pull Request car un status check requis était en échec.

Après suppression de l'échec volontaire et nouvelle exécution :

```text
Backend  ✅
Frontend ✅
```

la Pull Request est redevenue mergeable.

Le critère selon lequel une erreur de CI bloque effectivement la fusion est donc validé fonctionnellement.

---

# 18. Construction séparée

Backend et Frontend utilisent deux jobs distincts.

Ils ne dépendent pas l'un de l'autre :

```text
CI
├── Backend
└── Frontend
```

Un problème dans une partie de la plateforme peut ainsi être identifié indépendamment de l'autre.

Cette séparation satisfait également le besoin de construire Frontend et Backend séparément.

---

# 19. Absence de déploiement

SL-020 ne :

- publie aucune image Docker ;
- déploie aucun environnement ;
- applique aucune migration de production ;
- manipule aucun secret de déploiement ;
- configure aucun hébergeur ;
- crée aucun environnement Staging ou Production distant.

Le workflow valide uniquement que le repository est dans un état intégrable.

Le premier déploiement appartient à SL-021.

---

# 20. Validation de SL-020

SL-020 a été validée avec une vraie Pull Request GitHub.

Les comportements suivants ont été vérifiés :

```text
Pull Request vers main                    ✅
Déclenchement automatique de la CI        ✅
Job Backend indépendant                   ✅
Job Frontend indépendant                  ✅
Build Backend Release                     ✅
Tests Backend                             ✅
Build image Docker Backend                ✅
Lint Frontend                             ✅
Typecheck Frontend                        ✅
Tests Frontend                            ✅
Build Next.js Production                  ✅
Build image Docker Frontend               ✅
Backend déclaré Required                  ✅
Frontend déclaré Required                 ✅
Échec d'un check bloque le merge          ✅
Retour au vert autorise le merge          ✅
```

La chaîne d'intégration continue de SignalLab est donc opérationnelle.

---

# 21. État à la fin de SL-020

À la fin de SL-020 :

- chaque Pull Request vers `main` est automatiquement validée ;
- Backend et Frontend sont contrôlés séparément ;
- les tests sont exécutés automatiquement ;
- les builds de production sont contrôlés ;
- les images Docker sont construites automatiquement ;
- aucun secret de déploiement n'est requis ;
- `main` exige une Pull Request ;
- les checks `Backend` et `Frontend` sont obligatoires ;
- un échec de CI empêche réellement la fusion.

La prochaine étape est SL-021 — Réaliser le premier déploiement.