# Conventions de Persistence

**Projet :** SignalLab
**Document :** Conventions opérationnelles de Persistence
**Version :** 1.0
**Statut :** Validé
**Spec propriétaire :** SL-013 — Figer l’architecture de la solution

---

# 1. Objectif

Ce document définit les conventions applicables à la Persistence de SignalLab.

Il précise :

* le rôle de PostgreSQL ;
* le rôle d’Entity Framework Core ;
* l’organisation de la Persistence dans Infrastructure ;
* les responsabilités du `DbContext` ;
* les conventions de mapping ;
* les règles de lecture et d’écriture ;
* les transactions ;
* les contraintes et index ;
* les migrations ;
* les données persistées ou recalculées ;
* les patterns volontairement exclus.

SL-013 fixe ces conventions sans installer PostgreSQL ni Entity Framework Core.

L’installation et la configuration appartiennent respectivement à :

```text
SL-016 — Configurer PostgreSQL
SL-017 — Configurer Entity Framework Core
```

---

# 2. Technologies canoniques

La Persistence canonique de SignalLab utilise :

```text
PostgreSQL
Entity Framework Core
```

PostgreSQL constitue la base de données relationnelle principale.

Entity Framework Core constitue l’Object-Relational Mapper du Backend.

Aucune autre base de données ou technologie de Persistence n’est introduite sans besoin démontré et décision documentée.

---

# 3. Source de vérité

PostgreSQL constitue la source de vérité persistante des données métier.

Le Frontend, le cache TanStack Query et les projections analytiques ne remplacent jamais cette source.

Le Backend reste l’unique autorité capable de modifier les données métier.

```text
Frontend
    ↓ intention
API ASP.NET Core
    ↓
Application
    ↓
Domain
    ↓
Infrastructure
    ↓
PostgreSQL
```

Les données affichées côté Frontend peuvent être mises en cache, mais elles restent des représentations de l’état serveur.

---

# 4. Responsabilités des couches

## 4.1 Domain

Le Domain définit :

* les entités ;
* les Value Objects ;
* les invariants ;
* les comportements métier ;
* les transitions d’état ;
* les relations conceptuelles.

Le Domain ne connaît pas :

* Entity Framework Core ;
* PostgreSQL ;
* `DbContext` ;
* les migrations ;
* les tables ;
* les colonnes ;
* les requêtes SQL ;
* les détails de sérialisation.

Une entité Domain ne doit pas être conçue uniquement pour correspondre à une table.

## 4.2 Application

L’Application définit les besoins de Persistence nécessaires aux cas d’usage.

Elle peut contenir :

* interfaces de lecture ;
* interfaces d’écriture ;
* projections attendues ;
* critères explicites ;
* abstractions de transaction lorsqu’un besoin réel le justifie.

Exemples :

```text
IProjectReader
IProjectWriter
ICampaignReader
ICampaignWriter
```

L’Application ne référence pas Entity Framework Core.

Elle ne manipule pas directement :

```text
DbContext
DbSet
IQueryable fourni par Infrastructure
EntityTypeBuilder
Migration
```

## 4.3 Infrastructure

Infrastructure contient les détails de Persistence :

* `DbContext` ;
* `DbSet` ;
* configurations Fluent API ;
* migrations ;
* implémentations des interfaces Application ;
* requêtes Entity Framework Core ;
* gestion des transactions ;
* conventions de base de données.

Aucune règle métier ne doit exister exclusivement dans Infrastructure.

## 4.4 API

`SignalLab.Api` configure Infrastructure au Composition Root.

L’API ne doit pas accéder directement au `DbContext` depuis les Endpoints métier.

La chaîne attendue reste :

```text
Endpoint
↓
Application Handler
↓
Interface Application
↓
Implémentation Infrastructure
↓
DbContext
```

---

# 5. Organisation cible

Structure cible :

```text
SignalLab.Infrastructure/
└── Persistence/
    ├── SignalLabDbContext.cs
    ├── DependencyInjection.cs
    ├── Configurations/
    ├── Migrations/
    ├── Conventions/
    └── Modules/
        ├── Organizations/
        ├── Projects/
        ├── Campaigns/
        └── ...
```

Cette structure reste indicative.

Les dossiers métier ne sont créés que par leur Spec propriétaire.

## 5.1 `SignalLabDbContext`

Un seul `DbContext` principal est utilisé pour le Modular Monolith tant qu’aucun besoin démontré ne justifie une séparation.

Nom canonique proposé :

```text
SignalLabDbContext
```

Le `DbContext` représente l’unité de travail technique avec PostgreSQL.

## 5.2 Configurations

Chaque type persisté possède une configuration Fluent API distincte lorsque son mapping nécessite une configuration.

Exemple :

```text
Configurations/
├── OrganizationConfiguration.cs
├── ProjectConfiguration.cs
└── CampaignConfiguration.cs
```

Une configuration ne doit pas contenir de règle métier.

## 5.3 Migrations

Les migrations appartiennent à Infrastructure.

Elles sont versionnées dans Git.

Elles ne sont pas générées dans `SignalLab.Api` ou `SignalLab.Domain`.

---

# 6. DbContext

## 6.1 Durée de vie

Dans une requête HTTP classique, le `DbContext` utilise une durée de vie Scoped.

Une requête HTTP obtient normalement une instance de `DbContext`.

Cette instance :

* charge les entités ;
* suit les modifications ;
* persiste les changements ;
* participe à la transaction du cas d’usage.

## 6.2 Unité de travail

Entity Framework Core fournit déjà un mécanisme d’Unit of Work au travers du `DbContext`.

SignalLab n’ajoute pas une abstraction générique d’Unit of Work au-dessus d’EF Core par défaut.

Une abstraction supplémentaire ne peut être introduite que si elle résout un problème réel non couvert par le `DbContext`.

## 6.3 Accès au DbContext

Le `DbContext` reste un détail Infrastructure.

Il ne doit pas être injecté dans :

* une entité Domain ;
* un Value Object ;
* un Handler Application ;
* un Endpoint métier.

Les implémentations Infrastructure utilisent le `DbContext` pour satisfaire les interfaces définies dans Application.

---

# 7. Mapping

## 7.1 Fluent API

Le mapping utilise prioritairement Fluent API.

Les configurations sont regroupées dans Infrastructure.

Cette approche permet de conserver les détails de Persistence hors du Domain.

## 7.2 Attributs de Persistence

Les attributs Entity Framework Core ne doivent pas être ajoutés aux entités Domain.

Exemples non autorisés dans Domain :

```text
[Table]
[Column]
[Key]
[ForeignKey]
[Index]
```

Les attributs purement C# sans dépendance technologique peuvent être utilisés lorsqu’ils possèdent une valeur propre au langage ou au Domain.

## 7.3 Noms des tables

Les noms de tables doivent être :

* explicites ;
* homogènes ;
* stables ;
* liés aux concepts canoniques.

La convention singulier ou pluriel sera fixée par SL-017 avant la première migration.

Elle devra ensuite rester uniforme dans l’ensemble du schéma.

## 7.4 Noms des colonnes

Les colonnes doivent représenter clairement leur donnée.

Les conventions exactes de casse PostgreSQL seront fixées par SL-017.

Une propriété métier ne doit pas recevoir un nom différent uniquement pour des raisons techniques sans mapping explicite.

---

# 8. Identifiants

Les identifiants appartiennent au Domain lorsqu’ils représentent l’identité d’une entité métier.

Exemples futurs :

```text
OrganizationId
ProjectId
CampaignId
UserId
```

Leur représentation persistée est définie par Infrastructure.

Le Frontend et les contrats HTTP les manipulent comme des valeurs opaques.

Aucun comportement Client ne doit dépendre de leur format interne.

La stratégie exacte de génération des identifiants doit être uniforme et documentée avant l’introduction du premier domaine.

---

# 9. Value Objects

Les Value Objects sont persistés selon la forme la plus adaptée au modèle relationnel.

Options possibles :

* propriété convertie ;
* owned type ;
* complex type lorsque la version EF Core utilisée le permet ;
* colonnes explicites ;
* table séparée si l’identité ou la cardinalité le justifie.

Le choix dépend du comportement métier et de la structure du Value Object.

Un Value Object ne doit pas être transformé en entité uniquement pour faciliter le mapping.

---

# 10. Relations

Les relations persistées doivent respecter le Domain Model.

Exemples :

```text
Organization → Projects
Project → Campaigns
Campaign → Participations
Submission → Responses
```

Les relations sont configurées explicitement lorsque leurs comportements ne peuvent pas être déduits sans ambiguïté.

Les décisions importantes incluent :

* cardinalité ;
* propriété de la relation ;
* clé étrangère ;
* obligation ou optionnalité ;
* comportement de suppression ;
* index nécessaires.

Une relation persistée ne doit pas introduire une relation métier absente du Domain Model.

---

# 11. Suppression

La suppression physique n’est pas utilisée automatiquement.

Le comportement dépend du cycle de vie défini par le Domain.

Exemples de concepts distincts :

```text
Archived
Removed
Cancelled
Expired
Deleted
```

Un statut `Archived` ou `Removed` ne doit pas être implémenté par une suppression physique si le Domain exige la conservation historique.

Les règles de suppression en cascade doivent être explicites.

Une cascade ne doit jamais supprimer silencieusement une donnée historique devant rester consultable.

---

# 12. Lecture

## 12.1 AsNoTracking

Les requêtes de lecture qui ne modifient pas les entités utilisent `AsNoTracking` lorsque cela est pertinent.

Cela concerne notamment :

* listes ;
* projections ;
* vues de consultation ;
* données destinées à une Response ;
* calculs analytiques en lecture.

Une entité chargée pour être modifiée reste suivie par le `DbContext`.

## 12.2 Projections

Les Queries peuvent projeter directement vers un modèle de lecture Application lorsque cela :

* évite de charger un Aggregate complet inutilement ;
* réduit les données transférées ;
* conserve les règles métier ;
* ne duplique pas une formule métier importante dans SQL.

Exemple conceptuel :

```text
ProjectSummary
CampaignDetails
ParticipationOverview
```

## 12.3 IQueryable

`IQueryable` ne traverse pas la frontière Infrastructure → Application.

Infrastructure exécute la Query et retourne un résultat matérialisé ou un contrat Application explicite.

Cela empêche Application de dépendre implicitement d’Entity Framework Core et de la forme du schéma.

## 12.4 Pagination

Les collections potentiellement importantes sont paginées.

La requête doit appliquer :

1. filtres ;
2. tri stable ;
3. pagination ;
4. projection ;
5. matérialisation asynchrone.

Les collections ne sont pas chargées entièrement en mémoire avant pagination.

---

# 13. Écriture

Une opération d’écriture suit généralement le flux suivant :

```text
Charger l’Aggregate nécessaire
↓
Exécuter le comportement Domain
↓
Observer les modifications
↓
Persister avec SaveChangesAsync
```

Les setters publics ne doivent pas remplacer les comportements Domain.

Infrastructure ne modifie pas directement les propriétés internes afin de contourner un invariant.

## 13.1 SaveChangesAsync

Les écritures utilisent `SaveChangesAsync`.

Le `CancellationToken` provenant du cas d’usage est propagé.

## 13.2 Nombre de sauvegardes

Un cas d’usage cohérent cherche à effectuer une sauvegarde logique unique.

Plusieurs appels à `SaveChangesAsync` dans un même cas d’usage doivent être justifiés.

Une sauvegarde intermédiaire ne doit pas créer un état incohérent observable.

---

# 14. Interfaces de Persistence

Les interfaces appartiennent à Application lorsque les cas d’usage en ont besoin.

Elles représentent des capacités précises.

Exemples préférés :

```text
IProjectReader
IProjectWriter
IOrganizationReader
ICampaignWriter
```

Exemple non retenu :

```text
IRepository<TEntity>
```

## 14.1 Granularité

Une interface doit correspondre à une responsabilité cohérente.

Elle ne doit pas exposer toutes les opérations CRUD possibles par anticipation.

Exemple :

```csharp
public interface IProjectReader
{
    Task<Project?> GetByIdAsync(
        ProjectId projectId,
        CancellationToken cancellationToken);
}
```

Les méthodes sont ajoutées lorsqu’un cas d’usage réel en a besoin.

## 14.2 Lecture et écriture

Les responsabilités de lecture et d’écriture peuvent être séparées lorsque cela clarifie les dépendances.

Cette séparation n’impose pas un CQRS distribué ni deux bases de données.

Elle organise uniquement les capacités exposées à Application.

---

# 15. Generic Repository

SignalLab n’utilise pas de Generic Repository.

Entity Framework Core fournit déjà :

* suivi des entités ;
* requêtes ;
* ajout ;
* suppression ;
* unité de travail ;
* transactions.

Une abstraction générique comme :

```text
IRepository<TEntity>
```

masquerait souvent les besoins réels des cas d’usage et produirait une interface trop large.

Les interfaces de Persistence restent spécifiques aux modules et aux intentions.

---

# 16. Transactions

## 16.1 Frontière transactionnelle

La transaction correspond au cas d’usage nécessitant une cohérence atomique.

Exemples futurs :

```text
Créer Organization + premier OrganizationMember Owner
Accepter OrganizationInvitation + créer ou réactiver Membership
Activer Campaign + figer ses références historiques
Soumettre Submission + Responses
```

L’ensemble des modifications doit réussir ou être annulé.

## 16.2 Transaction implicite

Un appel unique à `SaveChangesAsync` utilise la transaction fournie par EF Core lorsque cela suffit.

Une transaction explicite est utilisée lorsque le cas d’usage comporte :

* plusieurs sauvegardes nécessaires ;
* plusieurs opérations devant rester atomiques ;
* un niveau d’isolation particulier ;
* une interaction technique justifiant explicitement cette portée.

## 16.3 Transactions distribuées

Le MVP n’introduit pas de transaction distribuée.

Les modules internes partagent le même processus et la même base PostgreSQL.

Les effets externes nécessitant une cohérence particulière devront être traités par une stratégie dédiée lorsqu’un cas réel apparaîtra.

---

# 17. Contraintes de données

PostgreSQL garantit les contraintes structurelles.

Cela comprend notamment :

* Primary Keys ;
* Foreign Keys ;
* colonnes obligatoires ;
* unicité ;
* Check Constraints lorsque pertinentes ;
* index ;
* longueurs maximales structurelles.

Ces contraintes renforcent le modèle mais ne remplacent pas le Domain.

## 17.1 Invariants métier

Les invariants restent exprimés dans le Domain.

Exemples :

```text
Une Organization possède toujours au moins un Owner.
Une Campaign active possède exactement un Build.
Une Submission est immuable après soumission.
```

La base peut ajouter une protection structurelle lorsque l’invariant est représentable sans ambiguïté.

Elle ne devient pas l’unique emplacement de la règle.

## 17.2 Unicité

Une contrainte d’unicité n’est ajoutée que si le Domain la définit.

Exemple interdit :

```text
Imposer un nom de Project unique sans règle métier correspondante.
```

Une contrainte technique non définie fonctionnellement ne doit pas limiter silencieusement le produit.

---

# 18. Index

Un index répond à un besoin de :

* clé étrangère ;
* unicité ;
* filtrage fréquent ;
* tri fréquent ;
* jointure ;
* performance mesurée.

Les index ne sont pas ajoutés arbitrairement à toutes les colonnes.

Ils sont documentés dans la configuration de l’entité.

Les performances sont mesurées avant l’ajout d’optimisations complexes.

---

# 19. Concurrence

Une stratégie de concurrence est introduite lorsque plusieurs utilisateurs peuvent modifier la même ressource de manière crédible.

Options futures possibles :

* token de concurrence ;
* colonne de version ;
* timestamp ;
* ETag exposé par l’API.

La stratégie doit permettre de détecter une mise à jour perdue.

Aucune stratégie générique n’est imposée avant le premier cas d’usage concerné.

Les conflits de concurrence sont transformés en résultat Application explicite puis en `409 Conflict` lorsque cela est approprié.

---

# 20. Migrations

Les évolutions de schéma utilisent les Migrations Entity Framework Core.

Chaque migration doit être :

* nommée clairement ;
* versionnée ;
* reproductible ;
* revue ;
* testée ;
* cohérente avec le Domain Model ;
* applicable sur une base propre ;
* applicable sur l’état précédent supporté.

## 20.1 Nommage

Le nom décrit le changement structurel.

Exemples futurs :

```text
InitialPlatformSchema
AddOrganizations
AddProjectStatus
AddCampaignBuildReference
```

Les noms génériques sont évités :

```text
UpdateDatabase
Changes
Fix
Migration1
```

## 20.2 Modification d’une migration

Une migration non partagée et non appliquée hors de l’environnement local peut être régénérée avant validation.

Une migration déjà versionnée et appliquée dans un environnement partagé ne doit pas être réécrite silencieusement.

Une nouvelle migration corrige le schéma.

## 20.3 Données de migration

Les migrations de données doivent être explicites et testées.

Une transformation destructive nécessite :

* une justification ;
* une stratégie de sauvegarde ou de migration ;
* une validation sur des données représentatives.

---

# 21. Données persistées

Les données métier structurées sont persistées dans PostgreSQL.

Cela inclut notamment à terme :

* Users métier ;
* Organizations ;
* OrganizationMembers ;
* OrganizationInvitations ;
* Projects ;
* Builds ;
* Measures ;
* FormTemplates et versions ;
* Campaigns ;
* CampaignForms ;
* Participations ;
* Submissions ;
* Responses ;
* MAP Profiles ;
* SavedAnalysis.

Cette liste représente l’architecture cible et ne crée aucun domaine pendant SL-013.

---

# 22. Données analytiques

Les résultats analytiques ne sont pas persistés comme source de vérité.

Une Analysis produit une projection recalculable à partir :

* des données métier persistées ;
* des métadonnées Domain ;
* de la configuration d’analyse ;
* de la version du moteur analytique.

Une `SavedAnalysis` persiste sa configuration, pas le résultat calculé comme vérité durable.

```text
SavedAnalysis Configuration
+
Persisted Research Data
+
AnalyticalEngineVersion
↓
Recalculated Projection
```

Les formules métier analytiques ne doivent pas être définies exclusivement en SQL.

Le moteur analytique reste responsable de leur sens et de leur déterminisme.

---

# 23. Données historiques

Les données historiques doivent respecter les règles d’immutabilité définies par le Domain Model.

Exemples :

* une Submission soumise reste immuable ;
* une Response historique n’est pas modifiée automatiquement ;
* les bindings d’un CampaignForm sont figés selon le cycle défini ;
* les références historiques restent consultables après archivage.

La Persistence ne doit pas remplacer ces règles par des mises à jour destructrices.

---

# 24. Multi-tenancy

SignalLab est multi-tenant par Organization.

La connaissance d’un identifiant ne suffit jamais à autoriser un accès.

Les Queries et Commands doivent vérifier :

* l’identité courante ;
* le Membership ;
* les Permissions ;
* l’appartenance de la ressource à l’Organization.

Les filtres d’Organization ne doivent pas reposer uniquement sur une convention oubliable dans le Frontend.

La stratégie technique exacte d’isolation sera définie avec les domaines Identity et Organization.

Toute requête multi-tenant doit être testée contre les accès croisés.

---

# 25. Seed Data

Les Seed Data sont limités aux besoins techniques ou canoniques clairement définis.

Ils peuvent concerner à terme :

* catalogue de Permissions ;
* rôles système ;
* catalogue fermé de QuestionTypes ;
* données de démonstration explicitement séparées.

Les données métier utilisateur ne doivent pas être ajoutées comme Seed de Production.

Les données de développement doivent être distinguées des données requises par le système.

---

# 26. Tests de Persistence

Les tests de Persistence utilisent PostgreSQL lorsque le comportement dépend réellement de PostgreSQL.

Une base InMemory ne doit pas être utilisée pour valider des comportements relationnels qui diffèrent du fournisseur réel.

Les tests couvrent notamment :

* mappings ;
* contraintes ;
* index importants ;
* relations ;
* comportement de suppression ;
* transactions ;
* requêtes ;
* migrations ;
* isolation des Organizations ;
* concurrence lorsque configurée.

La stratégie complète est définie dans :

```text
005 - TestingStrategy.md
```

---

# 27. Performance

Les optimisations sont guidées par des mesures.

Pratiques attendues :

* projection des colonnes nécessaires ;
* `AsNoTracking` pour les lectures appropriées ;
* pagination avant matérialisation ;
* absence de boucle provoquant des requêtes répétées ;
* limitation des Includes excessifs ;
* index liés aux usages réels ;
* inspection des requêtes lentes ;
* budgets définis pour les calculs analytiques.

Une dénormalisation ne doit pas être introduite sans mesure et décision documentée.

---

# 28. Journalisation

Les erreurs de Persistence sont journalisées côté Backend sans exposer de données sensibles.

Ne doivent pas apparaître dans les réponses HTTP :

* requêtes SQL ;
* chaînes de connexion ;
* secrets ;
* stack traces ;
* noms internes sensibles.

La journalisation ne doit pas enregistrer inutilement :

* tokens ;
* réponses personnelles complètes ;
* données MAP sensibles ;
* payloads de recherche confidentiels.

---

# 29. Stockage de fichiers

Le MVP ne stocke pas :

* artefacts binaires de Build ;
* pièces jointes ;
* exports ;
* archives de fichiers.

Les Builds représentent des références métier et non des fichiers hébergés.

Un futur stockage objet constituerait une responsabilité séparée de PostgreSQL et nécessiterait une Spec dédiée.

---

# 30. Patterns exclus par défaut

Les éléments suivants ne sont pas utilisés sans besoin démontré :

* Generic Repository ;
* Unit of Work générique ajoutée au-dessus d’EF Core ;
* accès au `DbContext` depuis Api ou Application ;
* attributs EF Core dans Domain ;
* `IQueryable` exposé hors Infrastructure ;
* base InMemory comme preuve de compatibilité PostgreSQL ;
* règles métier implémentées uniquement dans une configuration EF Core ;
* SQL arbitraire construit depuis une entrée utilisateur ;
* migration non versionnée ;
* modification silencieuse d’une migration partagée ;
* résultat analytique persisté comme source de vérité ;
* stockage binaire dans PostgreSQL sans décision dédiée ;
* suppression en cascade non explicitée ;
* optimisation non mesurée.

---

# 31. Checklist d’une entité persistée

Avant validation :

* [ ] le concept existe dans le Domain Model ;
* [ ] le module propriétaire est identifié ;
* [ ] le Domain ne référence pas EF Core ;
* [ ] la configuration Fluent API est distincte ;
* [ ] la Primary Key est définie ;
* [ ] les relations et Foreign Keys sont explicites ;
* [ ] les propriétés obligatoires sont contraintes ;
* [ ] les unicités correspondent à de vraies règles ;
* [ ] les comportements de suppression sont vérifiés ;
* [ ] les index répondent à un usage réel ;
* [ ] les règles métier restent dans Domain ;
* [ ] les lectures utilisent une projection adaptée ;
* [ ] les opérations asynchrones propagent le `CancellationToken` ;
* [ ] les tests utilisent PostgreSQL lorsque nécessaire ;
* [ ] une migration versionnée décrit le changement ;
* [ ] aucune donnée historique n’est détruite silencieusement.

---

# 32. Références

* `000 - SolutionArchitecture.md`
* `001 - BackendConventions.md`
* `003 - APIConventions.md`
* `005 - TestingStrategy.md`
* `02 - DomainModel.md`
* `06 - SoftwareArchitecture.md`
* `M - Milestones.md`
* ADR présentes dans `docs/architecture/adr/`
