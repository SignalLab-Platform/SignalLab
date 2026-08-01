# Stratégie de tests

**Projet :** SignalLab
**Document :** Stratégie opérationnelle de tests
**Version :** 1.0
**Statut :** Validé
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# 1. Objectif

Ce document définit la stratégie de tests de SignalLab.

Il précise :

* les objectifs des tests ;
* les niveaux de tests ;
* la répartition entre Backend, Frontend et Persistence ;
* les tests d’architecture ;
* les conventions de nommage ;
* les règles de déterminisme ;
* les données de test ;
* les responsabilités de la CI ;
* les pratiques volontairement exclues.

L’objectif n’est pas d’atteindre un pourcentage arbitraire de couverture.

L’objectif est de sécuriser :

* les invariants métier ;
* les cas d’usage ;
* les frontières architecturales ;
* les contrats HTTP ;
* la Persistence ;
* les interactions utilisateur importantes ;
* les parcours critiques.

---

# 2. Principes

## 2.1 Tester les comportements

Les tests vérifient des comportements observables.

Ils ne doivent pas dépendre inutilement de la structure interne de l’implémentation.

Un refactoring conservant le même comportement ne devrait pas imposer la réécriture de nombreux tests.

## 2.2 Tester au niveau le plus bas pertinent

Une règle est testée au niveau le plus simple capable de la garantir.

Exemples :

```text
Invariant d’entité
→ Test unitaire Domain

Orchestration d’un cas d’usage
→ Test Application

Code de statut et contrat JSON
→ Test d’intégration API

Foreign Key PostgreSQL
→ Test d’intégration Persistence

Interaction avec un Dialog
→ Test de composant Frontend

Cycle complet critique
→ Test end-to-end
```

## 2.3 Tests déterministes

Un test doit produire le même résultat à chaque exécution.

Il ne doit pas dépendre sans contrôle de :

* l’heure réelle ;
* un ordre aléatoire ;
* Internet ;
* une donnée externe mutable ;
* un état laissé par un test précédent ;
* une temporisation arbitraire ;
* le fuseau local de la machine.

## 2.4 Indépendance

Chaque test prépare les données dont il a besoin.

Un test ne dépend pas de l’exécution préalable d’un autre test.

Les tests peuvent s’exécuter :

* seuls ;
* dans un ordre différent ;
* en parallèle lorsque leur infrastructure le permet.

## 2.5 Lisibilité

Un test documente le comportement attendu.

Il doit permettre de comprendre :

* le contexte ;
* l’action ;
* le résultat.

La duplication limitée est acceptable lorsqu’elle améliore la lisibilité du scénario.

---

# 3. Pyramide de tests

La stratégie privilégie plusieurs niveaux complémentaires :

```text
Nombre élevé
Tests unitaires Domain et Application
        ↓
Tests de composants et d’architecture
        ↓
Tests d’intégration API et Persistence
        ↓
Nombre limité
Tests end-to-end des parcours critiques
```

La pyramide ne constitue pas un quota rigide.

Le niveau choisi dépend du risque et du comportement à sécuriser.

---

# 4. Projets de tests Backend

Structure cible :

```text
apps/api/tests/
├── SignalLab.Domain.UnitTests/
├── SignalLab.Application.UnitTests/
├── SignalLab.ArchitectureTests/
├── SignalLab.Api.IntegrationTests/
└── SignalLab.Persistence.IntegrationTests/
```

Les projets sont ajoutés lorsqu’un premier comportement correspondant existe.

SL-013 ajoute uniquement le projet de tests d’architecture en plus des tests API déjà créés.

Aucun projet vide n’est créé uniquement pour anticiper un domaine absent, sauf lorsqu’il constitue directement un livrable de la Spec.

---

# 5. Tests unitaires Domain

Les tests unitaires Domain vérifient :

* création d’entité ;
* invariants ;
* Value Objects ;
* transitions d’état ;
* comportements métier ;
* immutabilité ;
* erreurs métier ;
* calculs déterministes.

Ils ne dépendent pas de :

* ASP.NET Core ;
* Entity Framework Core ;
* PostgreSQL ;
* système de fichiers ;
* réseau ;
* horloge réelle ;
* fournisseur d’identité.

## 5.1 Forme

Exemple conceptuel :

```csharp
[Fact]
public void Archive_WhenProjectIsActive_ShouldArchiveProject()
{
    // Arrange
    // Act
    // Assert
}
```

## 5.2 Cas limites

Chaque invariant important couvre :

* scénario valide ;
* limite basse ;
* limite haute ;
* état incompatible ;
* répétition d’une transition ;
* valeur absente ou invalide lorsqu’elle appartient au Domain.

## 5.3 Immutabilité

Les règles historiques doivent posséder des tests explicites.

Exemples futurs :

```text
Submission soumise non modifiable
Campaign active conservant son Build
FormTemplateVersion publiée immuable
OrganizationId d’un Project immuable
```

---

# 6. Tests Application

Les tests Application vérifient l’orchestration des cas d’usage.

Ils couvrent notamment :

* chargement des données nécessaires ;
* autorisation applicative ;
* appel du comportement Domain ;
* écriture demandée ;
* résultat retourné ;
* absence d’écriture en cas d’échec ;
* propagation du `CancellationToken`.

Les dépendances Infrastructure sont remplacées par des Fakes ou Stubs ciblés.

## 6.1 Command

Exemple de scénario :

```text
CreateProject
Given User autorisé dans Organization
When Command exécutée
Then Project créé et persisté
```

## 6.2 Query

Exemple de scénario :

```text
GetProject
Given Project accessible
When Query exécutée
Then ProjectDetails retourné
```

## 6.3 Autorisation

Les cas d’usage multi-tenant couvrent systématiquement :

* accès autorisé ;
* User non membre ;
* Permission absente ;
* ressource d’une autre Organization ;
* ressource inexistante ;
* Membership Removed.

---

# 7. Tests d’architecture

Les tests d’architecture empêchent les régressions structurelles.

Projet :

```text
SignalLab.ArchitectureTests
```

Ils vérifient notamment :

* Domain ne référence aucun autre projet SignalLab ;
* Application référence uniquement Domain ;
* Infrastructure référence Application et Domain ;
* Api référence Application et Infrastructure ;
* Domain ne référence pas ASP.NET Core ;
* Domain ne référence pas Entity Framework Core ;
* Application ne référence pas Entity Framework Core ;
* aucune dépendance interdite n’est ajoutée silencieusement.

## 7.1 Niveau de vérification

Les premières règles peuvent inspecter :

* les références de projets ;
* les références d’assemblies ;
* les packages ;
* les namespaces interdits.

Une bibliothèque spécialisée de tests d’architecture n’est pas ajoutée tant que les contrôles nécessaires restent simples à exprimer avec .NET.

## 7.2 Évolution

Lorsqu’une nouvelle règle structurelle devient importante, le test d’architecture correspondant est ajouté dans la même Spec que la règle.

La documentation seule ne suffit pas lorsqu’une frontière peut être vérifiée automatiquement.

---

# 8. Tests d’intégration API

Les tests d’intégration API utilisent l’application ASP.NET Core réelle au travers de :

```text
WebApplicationFactory<Program>
```

Ils vérifient :

* routing ;
* méthode HTTP ;
* codes de statut ;
* sérialisation JSON ;
* contrats Request/Response ;
* validation technique ;
* Problem Details ;
* Middlewares ;
* authentification lorsque configurée ;
* autorisation ;
* OpenAPI lorsque pertinent.

## 8.1 Frontière testée

Le test envoie une requête HTTP et observe une réponse HTTP.

Il ne doit pas appeler directement le Handler lorsqu’il cherche à valider le contrat public.

## 8.2 Scénarios

Pour un Endpoint métier, couvrir selon le risque :

* succès ;
* Request invalide ;
* absence d’authentification ;
* absence de Permission ;
* ressource absente ;
* conflit métier ;
* isolation d’Organization ;
* format de Response ;
* Problem Details.

## 8.3 Contrat

Les assertions privilégient :

* statut ;
* Headers pertinents ;
* structure JSON ;
* propriétés publiques ;
* code d’erreur stable.

Elles ne doivent pas dépendre d’un ordre de propriétés JSON non contractuel.

---

# 9. Tests d’intégration Persistence

Les tests de Persistence utilisent PostgreSQL lorsque le comportement testé dépend du fournisseur réel.

Ils vérifient :

* mappings EF Core ;
* Primary Keys ;
* Foreign Keys ;
* contraintes d’unicité ;
* colonnes obligatoires ;
* relations ;
* suppressions ;
* transactions ;
* migrations ;
* requêtes importantes ;
* index lorsque leur existence est critique ;
* concurrence ;
* isolation multi-tenant.

## 9.1 Fournisseur réel

La base InMemory EF Core ne constitue pas une preuve de compatibilité PostgreSQL.

Elle peut être utilisée uniquement pour un test très local ne dépendant pas du comportement relationnel, mais elle n’est pas la stratégie d’intégration canonique.

## 9.2 Isolation

Chaque test doit utiliser :

* une base dédiée ;
* un schéma isolé ;
* ou une stratégie de nettoyage fiable.

Un test ne doit pas dépendre des données laissées par un autre.

La stratégie exacte sera définie lors de SL-016 et SL-017.

## 9.3 Migrations

Les tests doivent pouvoir vérifier :

* application des migrations sur une base vide ;
* cohérence du modèle ;
* évolution depuis l’état précédent supporté lorsque pertinent.

---

# 10. Tests Frontend

Le Frontend utilise principalement :

```text
Jest
React Testing Library
```

Les tests vérifient le comportement visible et les interactions utilisateur.

Ils ne doivent pas reproduire l’implémentation interne des composants.

## 10.1 Tests de composants

Ils couvrent notamment :

* rendu ;
* interaction clavier et souris ;
* états disabled ;
* états loading ;
* erreurs ;
* états vides ;
* callbacks ;
* accessibilité observable ;
* affichage conditionnel selon Permissions ;
* comportement d’un formulaire.

Exemple :

```text
Render CreateProjectDialog
↓
Saisir un nom
↓
Valider
↓
Mutation appelée avec CreateProjectRequest
```

## 10.2 Queries accessibles

React Testing Library privilégie les sélecteurs proches de l’usage réel :

```text
getByRole
getByLabelText
getByText
findByRole
```

Les sélecteurs fondés sur des classes CSS ou des détails de structure sont évités.

## 10.3 Server State

Les composants utilisant TanStack Query doivent être testés avec un `QueryClient` isolé par test.

Le cache ne doit pas être partagé entre tests.

Les retries sont désactivés dans les tests lorsque leur présence ralentit ou rend les scénarios ambigus.

## 10.4 Requêtes réseau

Les appels HTTP sont remplacés par une frontière contrôlée.

La stratégie exacte de Mock Service Worker ou d’un autre outil n’est pas imposée avant le premier besoin d’intégration réseau Frontend.

Les composants ne doivent pas dépendre d’un service externe réel pendant les tests.

---

# 11. Tests de Hooks et fonctions

Les fonctions pures sont testées directement lorsqu’elles contiennent un comportement significatif.

Exemples :

* formatage ;
* transformation de contrat ;
* construction de Query Keys ;
* calcul visuel dérivé ;
* normalisation locale non métier.

Un Hook est testé lorsqu’il possède un comportement propre non suffisamment couvert par le composant consommateur.

Les Hooks simples qui délèguent directement à TanStack Query ne nécessitent pas automatiquement un test isolé.

---

# 12. Tests end-to-end

Les tests end-to-end sont réservés aux parcours critiques.

Ils seront introduits lorsque plusieurs étapes fonctionnelles existent réellement.

Exemples futurs :

```text
Créer un compte
Créer une Organization
Créer un Project
Créer et activer une Campaign
Soumettre une Participation
Consulter Results
Ouvrir une Analysis
```

## 12.1 Nombre limité

Les tests end-to-end sont plus coûteux et plus lents.

Ils ne remplacent pas :

* les tests Domain ;
* les tests Application ;
* les tests API ;
* les tests de composants.

## 12.2 Outil

Aucun outil end-to-end n’est imposé pendant SL-013.

Playwright ou une alternative sera choisi lors de la première Spec nécessitant un parcours complet automatisé.

Cette décision devra tenir compte :

* de Next.js ;
* de l’authentification Clerk ;
* de Docker ;
* des besoins CI ;
* du coût de maintenance.

---

# 13. Tests analytiques

Le moteur analytique nécessite une stratégie particulièrement déterministe.

Les tests devront couvrir :

* normalisation ;
* direction Aligned/Inverted ;
* pondération ;
* couverture ;
* agrégation ;
* percentiles ;
* quartiles ;
* médiane ;
* écart-type ;
* corrélation de Spearman ;
* Hedges g ;
* intervalles d’incertitude ;
* valeurs manquantes ;
* groupes vides ;
* petits échantillons ;
* version du moteur.

## 13.1 Jeux de référence

Les calculs utilisent des jeux de données de référence :

```text
Input déterministe
↓
Projection attendue documentée
```

Les résultats numériques utilisent une tolérance explicite lorsque les calculs flottants le nécessitent.

## 13.2 Recalculabilité

Une même configuration, les mêmes données et la même version du moteur doivent produire le même résultat.

Les tests doivent protéger cette propriété.

## 13.3 Résultats non persistés

Les tests vérifient que la configuration d’une SavedAnalysis peut être persistée et que la projection est recalculée sans devenir la source de vérité.

---

# 14. Tests de sécurité

Les comportements de sécurité importants possèdent des tests explicites.

Cela comprend :

* token absent ;
* token invalide ;
* Permission absente ;
* User d’une autre Organization ;
* accès par identifiant connu ;
* ressource volontairement masquée ;
* données sensibles absentes des erreurs ;
* isolation MAP ;
* Membership Removed ;
* dernier Owner protégé.

Les tests de sécurité doivent vérifier le comportement serveur, pas uniquement l’affichage Frontend.

---

# 15. Nommage des tests Backend

Convention principale :

```text
Method_WhenCondition_ShouldExpectedBehavior
```

Exemples :

```text
Create_WhenNameIsMissing_ShouldRejectCreation
Archive_WhenProjectIsActive_ShouldArchiveProject
Activate_WhenCampaignHasNoBuild_ShouldRejectTransition
Accept_WhenInvitationIsExpired_ShouldRejectAcceptance
```

Lorsque la méthode n’est pas le meilleur point d’entrée conceptuel, le nom peut commencer par le comportement ou le cas d’usage :

```text
CreateProject_WhenUserLacksPermission_ShouldReturnForbidden
GetProject_WhenProjectBelongsToAnotherOrganization_ShouldReturnNotFound
```

Le nom doit rester lisible et décrire le scénario.

---

# 16. Nommage des tests Frontend

Les descriptions Jest expriment le comportement utilisateur.

Exemple :

```typescript
describe("CreateProjectDialog", () => {
  it("submits the entered project name", () => {
    // ...
  });

  it("displays the API validation error", () => {
    // ...
  });
});
```

Les descriptions ne doivent pas mentionner des détails internes sans valeur fonctionnelle.

Exemple évité :

```text
calls setState with true
```

Exemple préféré :

```text
opens the confirmation dialog
```

---

# 17. Structure Arrange, Act, Assert

Les tests Backend utilisent clairement :

```text
Arrange
Act
Assert
```

Les commentaires sont facultatifs lorsque la séparation reste évidente.

Un test cherche à conserver une seule action principale.

Plusieurs assertions sont acceptables lorsqu’elles décrivent le même résultat observable.

---

# 18. Builders et Fixtures

Les Builders de test sont utilisés lorsque la création d’un objet valide devient répétitive.

Exemple futur :

```text
ProjectBuilder
CampaignBuilder
SubmissionBuilder
```

Un Builder fournit des valeurs valides par défaut et permet de modifier uniquement les propriétés pertinentes au scénario.

Il ne doit pas masquer les données essentielles au test.

## 18.1 Object Mother

Une collection globale de Fixtures génériques est évitée si elle rend les scénarios difficiles à comprendre.

Les données de test restent proches de leur domaine ou projet de tests.

## 18.2 Valeurs explicites

Les valeurs influençant le comportement doivent apparaître clairement dans le test.

---

# 19. Horloge et temps

Le code métier ne dépend pas directement de :

```text
DateTime.UtcNow
DateTime.Now
```

lorsque le temps influence un comportement testable.

Une abstraction comme :

```text
IClock
```

peut être définie dans Application ou dans un Common approprié.

Les tests injectent une heure fixe.

Les dates absolues sont préférées aux dépendances relatives ambiguës.

---

# 20. Identifiants et aléatoire

Les tests utilisent des identifiants déterministes lorsque leur valeur influence les assertions.

Un générateur aléatoire ne doit pas rendre le test non reproductible.

Lorsqu’un caractère aléatoire est réellement testé, la Seed doit être contrôlée.

---

# 21. Async

Les tests asynchrones retournent `Task`.

Ils n’utilisent pas :

```text
async void
Thread.Sleep
Task.Delay arbitraire
```

Une attente asynchrone utilise :

* l’API réellement attendue ;
* une primitive de synchronisation ;
* une assertion asynchrone ;
* un timeout explicite uniquement lorsque nécessaire.

---

# 22. Mocks, Stubs et Fakes

Les doubles de tests sont utilisés aux frontières.

## 22.1 Stub

Retourne une donnée prédéfinie.

## 22.2 Fake

Implémente une version simplifiée mais fonctionnelle d’une interface.

## 22.3 Mock

Vérifie une interaction attendue.

Les tests ne doivent pas mocker chaque classe interne.

Un excès de Mocks produit des tests couplés à l’implémentation.

Les tests Domain utilisent normalement des objets réels.

---

# 23. Snapshots

Les Snapshot Tests ne sont pas utilisés par défaut.

Ils peuvent être pertinents pour un contrat ou une structure stable difficile à vérifier autrement.

Un Snapshot ne doit pas remplacer des assertions compréhensibles.

Les grands Snapshots peu relus sont évités.

---

# 24. Couverture

Aucun seuil global arbitraire n’est imposé par SL-013.

La couverture peut être mesurée comme indicateur.

Elle ne constitue pas une preuve de qualité.

Une ligne couverte sans assertion pertinente ne protège aucun comportement.

Les règles prioritaires sont :

* chaque invariant important est testé ;
* chaque cas d’usage possède ses scénarios critiques ;
* chaque Endpoint protège son contrat ;
* chaque mapping de Persistence important est vérifié ;
* chaque parcours critique possède une couverture adaptée.

Un seuil pourra être introduit ultérieurement si son bénéfice devient concret et s’il ne favorise pas des tests artificiels.

---

# 25. Tests ignorés

Un test ne doit pas rester ignoré silencieusement.

Tout test temporairement ignoré doit préciser :

* la raison ;
* la Spec ou issue propriétaire ;
* la condition de réactivation.

Un test instable ne doit pas être simplement relancé jusqu’à réussite.

La cause doit être corrigée.

---

# 26. Performance des tests

Les tests unitaires doivent rester rapides.

Les tests d’intégration peuvent être plus coûteux mais doivent conserver une durée compatible avec la CI.

Les suites peuvent être séparées lorsque nécessaire :

```text
Unit
Architecture
Integration
EndToEnd
```

Une optimisation de test ne doit pas réduire son isolation ou masquer des régressions.

---

# 27. CI

Chaque Pull Request devra exécuter au minimum, lorsque SL-020 est implémentée :

```text
Restore Backend
Build Backend
Run Backend Unit Tests
Run Architecture Tests
Run Integration Tests
Install Frontend Dependencies
Lint Frontend
Typecheck Frontend
Run Frontend Tests
Build Frontend
Audit Dependencies
```

Une erreur bloque la fusion.

Les contrôles Backend et Frontend restent identifiables séparément.

---

# 28. Tests locaux

Depuis la racine, les commandes actuelles comprennent :

```powershell
dotnet test apps/api/SignalLab.Api.sln --configuration Release
npm.cmd --prefix apps/web test
```

Les commandes des nouvelles suites seront documentées lorsqu’elles sont ajoutées.

Un développeur doit pouvoir exécuter une suite spécifique sans lancer systématiquement tous les tests end-to-end futurs.

---

# 29. Documentation des scénarios

Les comportements complexes peuvent être documentés par :

* nom de test explicite ;
* commentaire court ;
* Builder lisible ;
* jeu de données de référence ;
* référence à l’invariant du Domain Model.

Le test ne doit pas recopier toute la documentation fonctionnelle.

Il doit rendre évident le comportement protégé.

---

# 30. Tests requis par type de changement

## 30.1 Nouvelle règle Domain

Ajouter :

* scénario valide ;
* scénario invalide ;
* cas limites ;
* transition répétée si pertinente.

## 30.2 Nouveau cas d’usage

Ajouter :

* succès ;
* ressource absente ;
* autorisation ;
* conflit métier ;
* effet de Persistence attendu.

## 30.3 Nouvel Endpoint

Ajouter :

* contrat de succès ;
* Request invalide ;
* Problem Details ;
* authentification et autorisation selon le contexte.

## 30.4 Nouveau mapping EF Core

Ajouter :

* mapping ;
* contraintes ;
* relations ;
* migration ;
* requête principale.

## 30.5 Nouveau composant interactif

Ajouter :

* rendu principal ;
* interaction ;
* état disabled/loading ;
* erreur ;
* accessibilité pertinente.

## 30.6 Nouvelle décision architecturale vérifiable

Ajouter ou mettre à jour :

```text
SignalLab.ArchitectureTests
```

---

# 31. Patterns exclus par défaut

Les pratiques suivantes sont évitées :

* tests dépendant de l’ordre ;
* appels réseau externes réels ;
* horloge réelle pour un comportement déterministe ;
* `Thread.Sleep` ;
* assertions absentes ou vagues ;
* test d’une implémentation privée ;
* Mock de toutes les classes ;
* base InMemory comme preuve de comportement PostgreSQL ;
* partage incontrôlé d’un cache TanStack Query ;
* Snapshot massif non relu ;
* taux de couverture comme unique objectif ;
* test ignoré sans propriétaire ;
* retry masquant un test instable ;
* test end-to-end pour une règle unitaire ;
* test unitaire prétendant valider une intégration technique.

---

# 32. Checklist d’une Spec

Avant validation d’une Spec logicielle :

* [ ] les invariants nouveaux sont testés ;
* [ ] les cas d’usage critiques sont testés ;
* [ ] les autorisations sont couvertes ;
* [ ] l’isolation d’Organization est couverte lorsque pertinente ;
* [ ] les contrats HTTP sont testés ;
* [ ] les mappings et contraintes de Persistence sont testés ;
* [ ] les interactions Frontend importantes sont testées ;
* [ ] les erreurs et états limites sont couverts ;
* [ ] les tests sont déterministes ;
* [ ] les tests peuvent s’exécuter seuls ;
* [ ] aucun test ignoré sans justification ne subsiste ;
* [ ] les tests d’architecture sont à jour ;
* [ ] les commandes documentées passent ;
* [ ] aucune couverture artificielle n’a remplacé un scénario utile.

---

# 33. Références

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `002 - FrontendConventions.md`
* `003 - APIConventions.md`
* `004 - PersistenceConventions.md`
* `06 - SoftwareArchitecture.md`
* `02 - DomainModel.md`
* `03 - UserJourneys.md`
* `M - Milestones.md`
* ADR présentes dans `docs/architecture/adr/`
