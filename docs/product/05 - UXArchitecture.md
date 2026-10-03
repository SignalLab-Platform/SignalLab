# UX Architecture

## Objectif

Ce document définit les principes d'expérience utilisateur qui s'appliquent à l'ensemble de SignalLab.

Contrairement à l'Application Blueprint, qui présente l'organisation générale de l'application, ce document établit les règles de conception qui doivent être respectées lors de la création ou de l'évolution d'une interface.

Toutes les fonctionnalités de SignalLab doivent respecter ces règles afin de garantir une expérience cohérente, prévisible et homogène.

Les modèles d'interface de SignalLab sont directement dérivés du **Domain Model**. Un nouveau modèle d'interface ne doit être introduit que lorsqu'un besoin métier ne peut être représenté de manière satisfaisante par les modèles existants.

---

# Chapitre 1 — Principes généraux

## 1.1 Cohérence

- Une même interaction produit toujours le même comportement dans toute l'application.
- Un même composant possède toujours le même comportement, quel que soit le contexte.
- Une même information est toujours présentée de la même manière.

---

## 1.2 Simplicité

- Une interface ne présente que les informations nécessaires au contexte courant.
- Les actions secondaires ne doivent jamais concurrencer visuellement les actions principales.
- Une interface ne doit jamais avoir plusieurs responsabilités.

---

## 1.3 Prévisibilité

- Chaque action doit produire un résultat attendu par l'utilisateur.
- Les changements de contexte doivent toujours être explicites.
- Les changements de vue doivent être immédiats et ne jamais modifier le contexte.

---

## 1.4 Visibilité

- Les actions principales sont toujours visibles.
- L'utilisateur doit toujours connaître son contexte courant.
- L'utilisateur doit toujours savoir quelles actions sont possibles.

---

## 1.5 Persistance

- L'utilisateur ne doit jamais perdre son état de navigation sans action volontaire.
- Les recherches, filtres, sélections et positions de scroll sont restaurés lorsque cela est pertinent.

---

## 1.6 Feedback

- Toute action utilisateur produit un feedback visuel.
- Toute opération longue indique sa progression.
- Toute erreur est présentée de manière claire et exploitable.

---

## 1.7 Hiérarchie

- Le contexte est toujours prioritaire sur le contenu.
- La navigation est organisée selon la hiérarchie métier de SignalLab.
- Une ressource n'existe jamais indépendamment de son contexte.

---

## 1.8 Performance perçue

- Les interfaces doivent répondre immédiatement aux interactions utilisateur.
- Les chargements ne doivent jamais bloquer inutilement la navigation.
- Les états intermédiaires (chargement, synchronisation, sauvegarde) doivent être clairement indiqués lorsque leur durée est perceptible.

# Chapitre 2 — Layout

## 2.1 Objectif

Le **Layout** définit la structure permanente de l'application.

Il établit les responsabilités de chaque région de l'interface ainsi que les règles de composition qui garantissent une expérience cohérente dans l'ensemble de SignalLab.

Toutes les pages de l'application reposent sur cette structure.

---

## 2.2 Structure générale

Toutes les interfaces utilisent la structure suivante.

┌───────────────────────────────────────────────┐
│ Header                                        │
├───────────────┬───────────────────────────────┤
│               │                               │
│ Sidebar       │ Content                       │
│               │                               │
│               │                               │
└───────────────┴───────────────────────────────┘

Cette organisation est permanente.

Les responsabilités de chaque région ne changent jamais.

---

## 2.3 Header

Le **Header** est la barre globale de l'application.

Il est toujours visible.

Le **Header** contient uniquement des éléments globaux, notamment :

- le logo ;
- les notifications ;
- les paramètres utilisateur ;
- le profil utilisateur.

Les Notifications sont accessibles depuis une cloche située dans le Header.

Lorsqu'au moins une Notification non lue existe, la cloche affiche un indicateur permettant au User d'identifier qu'un événement nécessite son attention.

L'activation de la cloche ouvre une vue légère des Notifications récentes sans modifier le contexte métier courant.

Cette vue permet notamment de :

- distinguer les Notifications lues et non lues ;
- identifier rapidement l'événement signalé ;
- accéder à la cible métier d'une Notification lorsqu'elle existe.

La consultation des Notifications depuis le Header reste disponible quel que soit le contexte courant.

Le **Header** n'affiche jamais d'information spécifique au contexte courant.

Son contenu reste identique lors de toute navigation.

---

## 2.4 Sidebar

La **Sidebar** permet de naviguer entre les différents contextes de SignalLab.

Elle représente la hiérarchie métier de l'application.

Selon le contexte courant, elle affiche notamment :

- les Organizations ;
- les Projects ;
- les Campaigns.

La **Sidebar** reflète toujours le contexte actuellement ouvert.

Elle peut être réduite ou développée.

Son état est mémorisé entre les sessions.

---

## 2.5 Content

Le **Content** représente la zone de travail principale.

Son contenu dépend entièrement du contexte courant.

Selon la ressource affichée, le **Content** héberge :

- un Explorer ;
- un Workspace ;
- un Document.

Le **Content** contient également :

- la navigation locale ;
- les actions métier ;
- les informations de la ressource courante.

---

## 2.6 Changement de contexte

Lorsqu'un changement de contexte est effectué :

- le **Header** reste inchangé ;
- la **Sidebar** est mise à jour pour refléter le nouveau contexte ;
- le **Content** est entièrement remplacé.

Le changement de contexte ne modifie jamais les responsabilités de ces trois régions.

---

## 2.7 Adaptation Desktop

SignalLab est conçu exclusivement pour une utilisation desktop.

L'interface peut adapter sa présentation aux différentes tailles de fenêtres et d'écrans desktop.

Ces adaptations ne modifient jamais l'organisation logique de l'application :

- le Header conserve son rôle global ;
- la Sidebar conserve son rôle de navigation contextuelle ;
- le Content conserve son rôle de zone de travail principale.

Certains espacements, dimensions ou modes d'affichage peuvent être ajustés lorsque l'espace disponible diminue, à condition de préserver les responsabilités de chaque région.

Les navigateurs mobiles et les interactions tactiles ne sont pas pris en charge.

Sur un appareil mobile non supporté, SignalLab affiche une page informative indiquant qu'un navigateur desktop est requis.

---

## 2.8 Défilement

Le **Header** reste fixe.

La **Sidebar** possède son propre défilement lorsque son contenu dépasse la hauteur disponible.

Le **Content** possède également son propre défilement.

Le défilement d'une région n'affecte jamais les autres.

# Chapitre 3 — Navigation

## 3.1 Objectif

La navigation permet à l'utilisateur de se déplacer dans SignalLab tout en conservant une compréhension claire de son contexte.

Elle distingue deux concepts fondamentaux :

- la **Context Navigation** ;
- la **Local Navigation**.

Ces deux mécanismes ont des responsabilités distinctes et ne doivent jamais être confondus.

---

## 3.2 Context Navigation

La **Context Navigation** permet de changer de contexte métier.

Un changement de contexte remplace la ressource actuellement consultée par une autre ressource.

La **Context Navigation** peut être initiée depuis différents endroits de l'application, notamment :

- la Sidebar ;
- un Explorer ;
- une Workspace ;
- la Global Search ;
- une Notification ;
- un flux de création ;
- une URL ;
- l'historique du navigateur.

La localisation de l'action est indépendante de son effet.

Toute navigation qui ouvre une nouvelle ressource est une **Context Navigation**.

---

## 3.3 Effets d'un changement de contexte

Lorsqu'un changement de contexte est effectué :

- le Header reste inchangé ;
- la Sidebar reflète le nouveau contexte ;
- le Content est remplacé.

Toutes les informations spécifiques au contexte précédent disparaissent.

---

## 3.4 Local Navigation

La **Local Navigation** permet de naviguer à l'intérieur d'une même ressource.

Elle ne modifie jamais le contexte courant.

Selon le modèle d'interface, elle peut notamment permettre de :

- changer d'onglet ;
- ouvrir une section d'un document ;
- changer de vue.

La **Local Navigation** conserve toujours la même ressource.

---

## 3.5 URLs

Chaque contexte possède une URL unique.

Les vues internes d'une ressource possèdent également leur propre URL lorsque cela améliore la navigation ou le partage.

Une URL doit toujours permettre de reconstruire entièrement le contexte affiché.

Les états temporaires de l'interface ne sont pas représentés dans l'URL.

Cela inclut notamment :

- la recherche ;
- les filtres ;
- le tri ;
- la position de scroll ;
- les panneaux ouverts.

---

## 3.6 Navigation profonde

Toute ressource peut être ouverte directement via son URL.

Lorsqu'une URL est ouverte :

- le contexte est entièrement restauré ;
- la Sidebar reflète correctement la hiérarchie ;
- la ressource demandée est ouverte ;
- la vue demandée est affichée.

L'utilisateur ne doit jamais avoir à reconstruire manuellement le contexte.

---

## 3.7 Historique du navigateur

L'historique du navigateur reflète uniquement les changements significatifs de navigation.

Il inclut notamment :

- les changements de contexte ;
- les changements de vue possédant une URL.

Il n'inclut pas :

- les recherches ;
- les filtres ;
- le tri ;
- la position de scroll ;
- les interactions temporaires de l'interface.

---

## 3.8 Persistance

Chaque contexte mémorise indépendamment son état de navigation.

Lorsque cela est pertinent, les éléments suivants sont restaurés automatiquement :

- l'onglet actif ;
- la recherche ;
- les filtres ;
- le tri ;
- la position de scroll ;
- les panneaux ouverts ;
- la sélection courante.

Cette restauration ne modifie jamais le contexte métier.

# Chapitre 4 — Explorer

## 4.1 Objectif

Un **Explorer** permet de parcourir, rechercher et organiser une collection de ressources appartenant à un même contexte.

Contrairement à un **Workspace**, un **Explorer** ne représente jamais une ressource unique.

Il représente toujours un ensemble de ressources.

---

## 4.2 Structure

Tous les **Explorer** utilisent la même structure.

Search

↓

Tabs

↓

Primary Actions

↓

Content

Cet ordre est invariant.

---

## 4.3 Search

Chaque **Explorer** possède une **Context Search**.

La recherche est limitée au contexte courant.

Elle permet de retrouver rapidement une ressource parmi celles affichées par l'Explorer.

La recherche est :

- instantanée ;
- persistante ;
- indépendante des autres Explorers.

Changer de contexte ne conserve jamais la recherche d'un autre Explorer.

---

## 4.4 Tabs

Les **Tabs** permettent d'organiser les ressources par catégorie.

Chaque onglet représente une vue différente de la même collection.

Les **Tabs** constituent une **Local Navigation**.

Changer d'onglet :

- ne change jamais le contexte ;
- ne modifie jamais la Sidebar ;
- ne change pas la ressource sélectionnée lorsqu'elle reste visible.

Chaque onglet possède sa propre URL.

L'onglet actif est restauré lors du retour dans l'Explorer.

---

## 4.5 All Tab

Chaque **Explorer** possède un onglet **All**.

Cet onglet agrège toutes les catégories de ressources disponibles.

Les ressources y sont regroupées par sections.

Chaque section peut être développée ou réduite indépendamment.

L'état de chaque section est mémorisé.

---

## 4.6 Primary Actions

Les **Primary Actions** permettent de créer de nouvelles ressources dans le contexte courant.

Lorsqu'un contexte autorise plusieurs types de création, le bouton principal ouvre un menu proposant l'ensemble des ressources compatibles.

Après la création d'une ressource :

- la ressource est automatiquement sélectionnée ;
- si nécessaire, l'Explorer change d'onglet afin d'afficher la nouvelle ressource.

---

## 4.7 Content

Le **Content** affiche les ressources correspondant :

- au contexte courant ;
- à l'onglet actif ;
- à la recherche active.

La présentation dépend de la nature des ressources affichées.

À ce stade de l'application, un Explorer ne propose qu'un seul mode de présentation.

---

## 4.8 Sélection

Un **Explorer** ne permet qu'une seule sélection à la fois.

La ressource sélectionnée est toujours identifiable.

La sélection est restaurée lorsqu'elle est toujours valide.

---

## 4.9 Tri

Chaque onglet possède son propre mode de tri.

Le tri sélectionné est mémorisé indépendamment pour chaque onglet.

Modifier le tri d'un onglet n'affecte jamais les autres.

---

## 4.10 Chargement

Les collections volumineuses utilisent un chargement progressif.

Les ressources supplémentaires sont chargées automatiquement au fur et à mesure du défilement.

Le chargement ne doit jamais interrompre l'interaction avec les ressources déjà affichées.

---

## 4.11 États vides

Lorsqu'aucune ressource ne correspond au contexte courant ou à la recherche active, l'Explorer conserve exactement la même structure.

Seul le contenu est remplacé par un état vide approprié.

La recherche, les onglets et les actions principales restent accessibles.

---

## 4.12 Ouverture d'une ressource

L'ouverture d'une ressource dépend de sa nature.

Si la ressource représente un nouveau contexte, son ouverture déclenche une **Context Navigation**.

Dans le cas contraire, l'ouverture reste dans le contexte courant.

Le comportement est entièrement déterminé par le **Domain Model**.

---

## 4.13 Persistance

Chaque **Explorer** mémorise indépendamment :

- l'onglet actif ;
- la recherche ;
- le tri ;
- la position de scroll ;
- les sections développées ;
- la ressource sélectionnée.

Revenir dans un Explorer restaure automatiquement cet état lorsque cela reste pertinent.

# Chapitre 5 — Workspace

## 5.1 Objectif

Un **Workspace** permet de consulter et de modifier une ressource unique.

Contrairement à un **Explorer**, un **Workspace** ne représente jamais une collection de ressources.

Il offre plusieurs vues spécialisées permettant de travailler sur une même ressource sans jamais changer de contexte.

---

## 5.2 Structure

Tous les **Workspace** utilisent la même structure.

Workspace Header

↓

Tabs

↓

Content

Cet ordre est invariant.

---

## 5.3 Workspace Header

Le **Workspace Header** présente la ressource courante.

Il peut notamment afficher :

- le nom de la ressource ;
- son état ;
- ses informations principales.

Le **Workspace Header** contient également les actions qui concernent l'ensemble de la ressource.

Ces actions incluent notamment :

- le changement de statut ;
- le renommage ;
- l'archivage ;
- la suppression.

Les actions secondaires peuvent être regroupées dans un menu dédié.

Le **Workspace Header** reste identique quelle que soit la vue affichée.

---

## 5.4 Tabs

Les **Tabs** permettent de naviguer entre les différentes vues d'une même ressource.

Chaque onglet représente une vue spécialisée.

Les **Tabs** constituent une **Local Navigation**.

Changer d'onglet :

- ne change jamais le contexte ;
- ne modifie jamais la ressource courante ;
- ne réinitialise jamais l'état de la ressource.

Chaque onglet possède sa propre URL.

L'onglet actif est restauré lors du retour dans le Workspace.

---

## 5.5 Content

Le **Content** affiche la vue correspondant à l'onglet actif.

Chaque vue peut présenter ou modifier une partie différente de la ressource.

Toutes les vues manipulent néanmoins la même ressource.

Les actions propres à une vue restent confinées à cette vue et n'apparaissent pas dans le **Workspace Header**.

---

## 5.6 État de la ressource

La ressource conserve toujours le même état pendant la navigation entre les vues.

Changer d'onglet :

- ne réinitialise jamais les données ;
- n'interrompt jamais une opération en cours ;
- ne provoque jamais de perte de modifications.

Les vues représentent différentes perspectives d'une même ressource.

---

## 5.7 Navigation

Quitter un **Workspace** conserve son état.

Revenir ultérieurement restaure automatiquement la dernière vue consultée lorsque cela reste pertinent.

L'ouverture d'une autre ressource déclenche une **Context Navigation**, conformément au **Domain Model**.

---

## 5.8 Persistance

Chaque **Workspace** mémorise indépendamment :

- l'onglet actif ;
- la position de scroll ;
- les panneaux ouverts ou fermés.

Revenir dans un Workspace restaure automatiquement cet état lorsque cela reste pertinent.

---

## 5.9 Sauvegarde

Toutes les modifications sont enregistrées automatiquement.

Le **Workspace** ne présente jamais de bouton **Save**.

L'utilisateur n'a jamais à déclencher manuellement la sauvegarde.

Lorsque cela est pertinent, l'état de synchronisation est affiché afin d'informer l'utilisateur de la progression des enregistrements automatiques.

# Chapitre 6 — Document

## 6.1 Objectif

Un **Document** permet de consulter ou compléter une ressource organisée sous la forme d'un parcours de lecture structuré.

Contrairement à un **Workspace**, un **Document** ne propose pas plusieurs vues d'une même ressource.

Il présente une unique vue composée de plusieurs sections successives.

---

## 6.2 Structure

Tous les **Document** utilisent la même structure.

Document Header

↓

Table of Contents

↓

Document Content

Cet ordre est invariant.

---

## 6.3 Document Header

Le **Document Header** présente le document courant.

Il peut notamment afficher :

- le titre du document ;
- son état ;
- les informations principales.

Le **Document Header** contient également les actions qui concernent l'ensemble du document.

Le **Document Header** reste identique pendant toute la consultation du document.

---

## 6.4 Table of Contents

Chaque **Document** possède une **Table of Contents**.

La **Table of Contents** représente la structure logique du document.

Chaque entrée correspond à une section du document.

La **Table of Contents** constitue une **Local Navigation**.

Naviguer dans la **Table of Contents** :

- ne change jamais le contexte ;
- ne change jamais de document ;
- positionne la vue sur la section demandée.

La section actuellement visible est toujours identifiable.

---

## 6.5 Document Content

Le **Document Content** présente les sections du document sous la forme d'un flux de lecture continu.

Les sections sont affichées dans leur ordre logique.

Le document utilise un défilement continu.

Lorsque le volume de contenu le nécessite, les sections sont chargées progressivement au fur et à mesure du défilement.

Le chargement ne doit jamais interrompre la lecture ou l'interaction avec les sections déjà affichées.

---

## 6.6 Navigation

La navigation entre les sections conserve toujours le document courant.

Chaque section peut posséder sa propre URL lorsqu'un accès direct présente un intérêt.

Ouvrir une URL de section restaure automatiquement :

- le document ;
- le contexte ;
- la position sur la section demandée.

---

## 6.7 Persistance

Chaque **Document** mémorise indépendamment :

- la position de scroll ;
- la dernière section consultée ;
- les panneaux ouverts ou fermés.

Revenir dans un Document restaure automatiquement cet état lorsque cela reste pertinent.

---

## 6.8 Sauvegarde

Toutes les modifications sont enregistrées automatiquement.

Le **Document** ne présente jamais de bouton **Save**.

L'utilisateur n'a jamais à déclencher manuellement la sauvegarde.

Lorsque cela est pertinent, l'état de synchronisation est affiché afin d'informer l'utilisateur de la progression des enregistrements automatiques.

# Chapitre 7 — Search

## 7.1 Objectif

La recherche permet de retrouver rapidement une ressource sans parcourir manuellement la navigation.

SignalLab utilise un système de recherche unique.

Selon le contexte d'utilisation, ce système applique un périmètre de recherche différent.

---

## 7.2 Search Scopes

SignalLab distingue deux périmètres de recherche.

### Global Search

La **Global Search** est accessible depuis la Sidebar.

Elle recherche parmi l'ensemble des ressources accessibles à l'utilisateur.

### Context Search

La **Context Search** est intégrée aux Explorer.

Elle recherche uniquement parmi les ressources appartenant au contexte courant.

Le mécanisme de recherche est identique.

Seul le périmètre des ressources indexées diffère.

---

## 7.3 Ressources indexées

Toute ressource possédant une représentation dédiée dans l'application est indexable par le moteur de recherche.

Selon sa nature, une ressource peut être ouverte dans :

- un nouveau contexte ;
- un Workspace ;
- un Document.

La recherche ne détermine jamais le mode d'ouverture.

Celui-ci est défini par le Domain Model.

---

## 7.4 Résultats

Les résultats sont affichés de manière instantanée au fur et à mesure de la saisie.

Ils utilisent les mêmes règles de présentation quel que soit le périmètre de recherche.

Chaque résultat identifie clairement :

- son type ;
- son contexte ;
- son nom.

---

## 7.5 Navigation

Sélectionner un résultat ouvre directement la ressource correspondante.

L'ouverture peut déclencher :

- une Context Navigation ;
- l'ouverture d'un Workspace ;
- l'ouverture d'un Document.

Le comportement dépend uniquement de la ressource sélectionnée.

---

## 7.6 Persistance

La Context Search mémorise indépendamment :

- le texte recherché ;
- les éventuels filtres propres à l'Explorer.

La Global Search n'est jamais persistée.

Chaque nouvelle ouverture démarre avec une recherche vide.

---

## 7.7 Performance

La recherche ne doit jamais bloquer l'interface.

Les résultats sont mis à jour progressivement au fur et à mesure de la saisie.

Lorsqu'une recherche nécessite un chargement supplémentaire, celui-ci est indiqué à l'utilisateur sans interrompre son interaction.

# Chapitre 8 — Resource Creation

## 8.1 Objectif

La création de ressources permet d'ajouter de nouveaux éléments au sein du Domain Model.

Une ressource est toujours créée dans un contexte existant.

La création ne modifie jamais les règles de navigation ou de représentation des ressources.

---

## 8.2 Contexte de création

Toute création est initiée depuis le contexte propriétaire de la ressource.

Une ressource ne peut jamais être créée indépendamment de son contexte.

Par exemple :

- une Organization permet de créer des Projects ;
- une Organization permet de créer des Measures ;
- une Organization permet de créer des Form Templates ;
- un Project permet de créer des Campaigns.

Le contexte de création est entièrement défini par le Domain Model.

---

## 8.3 Initiation

La création est toujours initiée depuis une **Primary Action**.

Lorsqu'un contexte autorise plusieurs types de création, la **Primary Action** ouvre un menu présentant l'ensemble des ressources pouvant être créées dans ce contexte.

La création d'une ressource ne doit jamais dépendre d'une action cachée ou difficilement découvrable.

---

## 8.4 Création

Le processus de création collecte uniquement les informations nécessaires à l'initialisation de la ressource.

Les informations complémentaires sont renseignées ultérieurement dans le Workspace ou le Document associé.

---

## 8.5 Validation

La création est atomique.

À l'issue de la validation :

- soit la ressource est entièrement créée ;
- soit aucune ressource n'est créée.

Une ressource ne doit jamais exister dans un état partiellement créé.

---

## 8.6 Ouverture

Une fois créée, la nouvelle ressource devient immédiatement la ressource active.

L'application ouvre automatiquement la représentation correspondant à cette ressource.

Selon le Domain Model, cela peut correspondre à :

- une Context Navigation ;
- l'ouverture d'un Workspace ;
- l'ouverture d'un Document.

Le mécanisme d'ouverture est indépendant du processus de création.

---

## 8.7 Responsabilités

Le système de création est uniquement responsable de créer une nouvelle ressource.

Il ne détermine jamais la manière dont cette ressource est représentée ou ouverte.

Ces comportements sont définis par le Domain Model.

# Chapitre 9 — Permissions

## 9.1 Objectif

Le système de permissions détermine quelles actions sont autorisées sur chaque ressource.

Les permissions ne modifient jamais la structure de l'application.

Elles déterminent uniquement les actions disponibles pour l'utilisateur.

---

## 9.2 Actions

Chaque type de ressource expose un ensemble d'actions.

Par exemple :

- View
- Create
- Edit
- Archive
- Delete
- Change Status

Les actions disponibles dépendent de la nature de la ressource.

---

## 9.3 Autorisations

Les autorisations sont évaluées indépendamment de l'interface utilisateur.

Pour chaque ressource, le système détermine quelles actions sont autorisées.

L'interface ne décide jamais des permissions.

---

## 9.4 Interface utilisateur

L'interface utilisateur ne raisonne jamais en termes de rôles ou de permissions.

Elle raisonne uniquement en termes d'actions disponibles.

Selon les capacités exposées par le système d'autorisation, une action peut être :

- disponible ;
- indisponible ;
- totalement absente de l'interface lorsque cela est approprié.

---

## 9.5 Navigation

Les permissions n'affectent jamais les mécanismes de navigation.

La Context Navigation, la Local Navigation et la structure générale de l'application conservent toujours le même comportement.

Seules les actions accessibles peuvent varier.

---

## 9.6 Responsabilités

Le système de permissions est responsable de déterminer les actions autorisées.

L'interface utilisateur est responsable de représenter ces actions.

La logique d'autorisation ne doit jamais être implémentée dans les composants d'interface.

# Chapitre 10 — Data Lifecycle

## 10.1 Objectif

Le cycle de vie des données définit la manière dont SignalLab charge, présente, modifie et synchronise les ressources.

L'objectif est de fournir une interface réactive tout en garantissant la cohérence entre l'état local et les données du système.

---

## 10.2 États des données

Une ressource peut successivement traverser plusieurs états au cours de son cycle de vie :

- Loading ;
- Available ;
- Refreshing ;
- Modified ;
- Synchronizing ;
- Synchronized ;
- Error.

Chaque état possède un comportement d'interface spécifique.

---

## 10.3 Loading

Le **Loading** correspond au chargement initial d'une ressource.

Pendant cette phase :

- la structure de l'interface reste stable ;
- le contenu est remplacé par une représentation de chargement lorsque cela est pertinent ;
- les éléments déjà disponibles restent interactifs.

Lorsque la structure du contenu est connue, un **Skeleton** est privilégié à un indicateur de chargement générique.

---

## 10.4 Refreshing

Le **Refreshing** correspond à la mise à jour de données déjà affichées.

Le contenu existant reste visible et utilisable pendant toute la durée de l'actualisation.

L'utilisateur ne doit jamais perdre le contexte ou les informations déjà affichées.

Lorsque cela est pertinent, un indicateur discret signale qu'une actualisation est en cours.

---

## 10.5 Optimistic Updates

Les modifications utilisateur sont appliquées immédiatement dans l'interface.

La synchronisation avec le système intervient ensuite de manière asynchrone.

L'interface ne doit jamais attendre la confirmation du système avant de refléter une action utilisateur.

---

## 10.6 Synchronization

Toutes les modifications sont synchronisées automatiquement.

Lorsque cela est pertinent, l'interface indique qu'une synchronisation est en cours.

Cette indication ne doit jamais interrompre le travail de l'utilisateur.

---

## 10.7 Prévention

L'interface doit empêcher autant que possible les actions connues comme impossibles.

Lorsqu'une action est indisponible, elle est représentée comme telle avant son exécution.

Lorsque cela est pertinent, la raison de cette indisponibilité est explicitée à l'utilisateur.

L'utilisateur ne doit jamais découvrir une contrainte connue uniquement après avoir tenté une action.

---

## 10.8 Validation

Le système reste l'autorité finale sur la validité des opérations.

Lorsqu'une opération est refusée par le système alors qu'elle était considérée valide par l'interface, cette situation est traitée comme un cas exceptionnel.

L'interface restaure alors un état cohérent et informe l'utilisateur de l'échec de l'opération.

---

## 10.9 Responsabilités

L'interface est responsable de fournir une expérience fluide et réactive.

Le système est responsable de valider définitivement les opérations.

Le cycle de vie des données garantit que ces deux responsabilités restent indépendantes.

# Chapitre 11 — User Communication

## 11.1 Objectif

La communication utilisateur regroupe l'ensemble des mécanismes permettant à SignalLab de répondre aux interactions de l'utilisateur et de l'informer des événements importants.

Chaque mécanisme possède une responsabilité unique.

Une même information ne doit jamais être communiquée simultanément par plusieurs mécanismes.

---

## 11.2 Feedback

Le **Feedback** est la réponse immédiate de l'interface à une interaction utilisateur.

Le Feedback confirme que l'action a été prise en compte.

Il peut notamment prendre la forme :

- d'un changement d'état visuel ;
- d'un indicateur de progression ;
- d'une sélection ;
- d'un aperçu d'interaction.

Le Feedback est toujours :

- immédiat ;
- local ;
- directement lié à l'action de l'utilisateur.

---

## 11.3 Toasts

Les **Toasts** informent l'utilisateur du résultat d'une action récente.

Ils sont :

- temporaires ;
- non bloquants ;
- affichés indépendamment du contenu courant.

Les Toasts disparaissent automatiquement après une courte durée.

Ils peuvent proposer une action secondaire lorsque cela est pertinent, comme :

- Retry ;
- Undo ;
- Open.

Les Toasts ne constituent jamais un historique des événements.

---

## 11.4 Notifications

Les **Notifications** informent le User d'événements métier importants nécessitant son attention ou pouvant nécessiter une consultation ultérieure.

Elles sont :

- persistantes ;
- associées au User destinataire ;
- indépendantes de la vue actuellement affichée ;
- accessibles globalement depuis la cloche du Header ;
- distinguées entre lues et non lues.

Une Notification peut notamment signaler :

- une invitation ;
- un changement d'état pertinent d'une ressource ;
- une activité réalisée par un autre User ;
- une information nécessitant une consultation ultérieure.

L'ouverture de la cloche présente une vue légère des Notifications récentes.

Lorsqu'une Notification possède une cible métier, son activation ouvre la ressource ou l'action concernée selon les règles normales de navigation de SignalLab.

La Notification ne remplace jamais cette cible et n'en reproduit pas les règles métier.

Son état de lecture indique uniquement si le User a pris connaissance de la Notification.

La lecture d'une Notification ne vaut donc jamais acceptation, refus, traitement ou résolution de l'événement métier concerné.

Les Notifications peuvent constituer un historique des événements importants destinés au User.

Le MVP ne nécessite cependant pas un Notification Center complet ou un espace métier autonome consacré à cet historique.

Si le volume ou les usages futurs le nécessitent, une vue étendue des Notifications pourra compléter l'accès léger du Header sans modifier leur responsabilité.

---

## 11.5 Inline Messages

Les **Inline Messages** communiquent une information directement à l'endroit concerné par celle-ci.

Ils permettent notamment de présenter :

- une erreur de validation ;
- une information contextuelle ;
- une recommandation ;
- un état vide.

Les Inline Messages restent visibles tant que leur information demeure pertinente.

---

## 11.6 Dialogs

Les **Dialogs** sollicitent une décision explicite de l'utilisateur.

Ils interrompent temporairement le flux de travail jusqu'à ce qu'une décision soit prise.

Les Dialogs ne sont jamais utilisés pour transmettre une simple information.

Ils sont réservés aux situations nécessitant une confirmation ou un choix explicite.

---

## 11.7 Confirmations

Les actions métier majeures nécessitent une confirmation explicite avant leur exécution.

Cette confirmation est réalisée au moyen d'un Dialog.

Les actions facilement réversibles peuvent, lorsque cela est pertinent, être confirmées après leur exécution au moyen d'un Toast proposant une action d'annulation.

Le choix entre une confirmation préalable et une annulation ultérieure dépend de la nature de l'action métier.

---

## 11.8 Responsabilités

Chaque mécanisme possède une responsabilité clairement définie :

- le **Feedback** répond à une interaction ;
- le **Toast** communique un résultat temporaire ;
- la **Notification** communique un événement persistant ;
- l'**Inline Message** communique une information contextuelle ;
- le **Dialog** sollicite une décision.

Une même information ne doit être communiquée que par le mécanisme le plus approprié.

# Chapitre 12 — Empty States

## 12.1 Objectif

Les **Empty States** représentent l'absence de contenu au sein d'une interface.

Ils permettent d'expliquer cette situation à l'utilisateur sans modifier la structure ou le fonctionnement de l'application.

---

## 12.2 Structure

Un **Empty State** remplace uniquement la zone de contenu concernée.

Les autres éléments de l'interface restent inchangés.

Notamment :

- le Header conserve son contenu ;
- la Sidebar conserve son rôle de navigation ;
- les Tabs restent disponibles lorsqu'elles existent ;
- les Primary Actions conservent leur comportement.

L'absence de contenu ne modifie jamais la structure de l'interface.

---

## 12.3 Types d'Empty States

SignalLab distingue trois catégories d'Empty States.

### Empty Collection

Aucune ressource n'existe dans le contexte courant.

### Empty Result

Des ressources existent, mais aucune ne correspond à la recherche ou aux filtres appliqués.

### Empty Content

La ressource existe, mais une partie de son contenu est vide.

---

## 12.4 Responsabilités

Un **Empty State** est responsable :

- d'expliquer pourquoi aucun contenu n'est affiché ;
- d'orienter l'utilisateur lorsque cela est pertinent.

Un **Empty State** ne modifie jamais :

- la navigation ;
- les actions disponibles ;
- la structure générale de l'interface.

# Chapitre 13 — Functional Overlays

## 13.1 Objectif

Une **Functional Overlay** permet d'ouvrir un espace de travail transversal sans abandonner le contexte courant.

Elle est utilisée uniquement lorsqu'une fonctionnalité doit consommer plusieurs ressources du contexte sans devenir elle-même un niveau de la hiérarchie métier.

L'Analysis Workspace constitue le premier modèle de Functional Overlay de SignalLab.

---

## 13.2 Relation avec le contexte

L'ouverture d'une Functional Overlay :

- ne change pas le contexte métier actif ;
- ne modifie pas la Sidebar ;
- ne remplace pas définitivement le Content sous-jacent ;
- conserve l'état de navigation du contexte d'origine ;
- peut utiliser ce contexte pour initialiser sa propre configuration.

La fermeture restaure exactement le contexte et la vue qui étaient affichés avant l'ouverture.

---

## 13.3 Structure

Une Functional Overlay utilise la structure suivante :

Overlay Header

↓

Overlay Navigation and Controls

↓

Overlay Content

Elle couvre la zone nécessaire à son travail tout en indiquant clairement qu'un contexte sous-jacent existe toujours.

Le Header global et la Sidebar ne sont jamais réutilisés comme contrôles internes de l'Overlay.

---

## 13.4 Navigation

Une Functional Overlay peut être ouverte depuis plusieurs vues lorsque son effet est identique.

Le bouton flottant **Analytics** ouvre toujours l'Analysis Workspace, qu'il soit utilisé depuis un Project, une Campaign ou Results.

L'emplacement du bouton change uniquement les valeurs initiales fournies à l'Overlay.

Le bouton retour du navigateur et la touche d'échappement ferment l'Overlay lorsque cela ne provoque aucune perte non confirmée.

Une URL peut représenter une SavedAnalysis afin de rouvrir l'Overlay avec sa configuration, sans transformer la SavedAnalysis en nouveau niveau de contexte.

---

## 13.5 Persistance

Une Analysis temporaire n'est pas enregistrée automatiquement comme ressource métier.

L'action **Save Analysis** crée explicitement une SavedAnalysis.

Après cette création :

- les modifications de configuration sont autosauvegardées ;
- aucun résultat calculé n'est persisté ;
- l'état de synchronisation est affiché comme dans les autres Workspaces.

Fermer une Analysis temporaire contenant des modifications non sauvegardées exige une confirmation explicite.

---

# Chapitre 14 — Analysis Experience

## 14.1 Objectif

L'Analysis Workspace doit rendre un moteur analytique riche exploitable sans demander au User de configurer des méthodes statistiques.

L'interface expose les intentions de recherche, les populations, les preuves et les limites des résultats.

Elle ne présente jamais les Analytics comme des conclusions automatiques.

---

## 14.2 Initialisation contextuelle

Le contexte d'ouverture initialise le Analysis Scope :

- une Campaign depuis Campaign ou Results ;
- un Project depuis le Project Explorer ;
- la configuration persistée depuis une SavedAnalysis.

Cette initialisation est une valeur par défaut et non une contrainte.

Le User peut sélectionner toute Campaign appartenant au même Project.

---

## 14.3 Structure de l'Analysis Workspace

L'interface distingue visuellement :

- les Subjects analysés : Questions et Measures ;
- le Scope : Project et Campaigns ;
- les Groups comparés ;
- les Filters ;
- la Visualization ;
- les Sources et détails méthodologiques.

Modifier un élément recalcule uniquement les projections concernées.

Les calculs longs sont annulables et ne bloquent pas la navigation dans les résultats déjà disponibles.

---

## 14.4 Visualisations compatibles

SignalLab ne propose jamais un catalogue arbitraire de graphiques.

Chaque type de donnée dispose d'un ensemble fini de représentations compatibles.

Exemples :

- une Measure quantitative propose score, distribution, box plot, évolution et comparaison ;
- une Question catégorielle propose effectifs et proportions ;
- une Question textuelle propose recherche et liste de verbatims ;
- deux Measures proposent scatter et corrélation ;
- plusieurs Measures et Campaigns proposent profils et heatmaps.

Une représentation incompatible n'est jamais affichée comme option désactivée sans explication.

---

## 14.5 Interprétabilité

Chaque résultat affiche les informations nécessaires à sa lecture :

- population incluse ;
- données valides et manquantes ;
- couverture ;
- Campaigns et Builds ;
- méthode de calcul ;
- date de calcul ;
- version du moteur analytique.

`MeasureScore` et `OutcomeScore` restent visuellement distincts.

Les tailles d'effet, corrélations et intervalles d'incertitude utilisent un vocabulaire descriptif et ne suggèrent jamais une causalité.

---

## 14.6 Traçabilité

Toute visualisation permet d'accéder progressivement à ses sources lorsque les Permissions l'autorisent.

Result

↓

Submission Measure Scores

↓

Responses

↓

Submissions

↓

Campaign Participations

La navigation vers une source ne détruit jamais la configuration courante de l'Analysis.

---

## 14.7 Quantitatif et qualitatif

Les Questions `Contextual` associées à une Measure sont accessibles depuis la vue de cette Measure.

Le User peut notamment consulter les verbatims correspondant :

- à un groupe ;
- à une Campaign ou un Build ;
- à une plage de MeasureScore ;
- à un segment MAP lorsque disponible.

Aucun thème, sentiment ou résumé n'est généré automatiquement sans fonctionnalité explicitement prévue.

---

## 14.8 Confidentialité MAP

Les filtres MAP affichent uniquement :

- la population utilisable ;
- la population indisponible ;
- les résultats agrégés autorisés.

L'interface ne révèle jamais si un User individuel ne possède pas de profil, le garde privé ou utilise une version incompatible.