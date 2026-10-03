# SignalLab — Product Foundation

> Version MVP – Document fondateur

---

# 1. Vision

SignalLab est une plateforme de **User Research dédiée au jeu vidéo**, conçue pour permettre aux studios de prendre de meilleures décisions de game design à partir de données qualitatives et quantitatives collectées auprès de leurs joueurs.

L'objectif de SignalLab n'est pas de produire davantage de données.

L'objectif est de **transformer les retours de playtests en connaissances exploitables dans le temps**.

Aujourd'hui, la plupart des studios réalisent leurs playtests à l'aide d'un ensemble d'outils généralistes : Google Forms, Typeform, Discord, Excel, Notion, Steam Reviews, questionnaires internes, captures d'écran ou discussions informelles.

Ces outils permettent de collecter des informations, mais rarement de les structurer.

Chaque campagne devient un événement isolé.

Les connaissances acquises disparaissent progressivement.

Les questions changent.

Les formulaires évoluent.

Les équipes changent.

Quelques mois plus tard, il devient presque impossible de répondre à des questions pourtant fondamentales :

* Est-ce que notre système de combat est réellement meilleur qu'il y a six mois ?
* Cette mise à jour a-t-elle amélioré la lisibilité du gameplay ?
* Les nouveaux joueurs vivent-ils la même expérience que les anciens ?
* Quels profils de joueurs apprécient réellement cette fonctionnalité ?

SignalLab existe pour répondre à ces questions.

---

# 2. Le problème que SignalLab résout

Le véritable problème n'est pas la création de questionnaires.

Le véritable problème est la perte de connaissance.

Les studios disposent généralement :

* d'un grand nombre de questionnaires différents ;
* de multiples versions du jeu ;
* de plusieurs centaines ou milliers de réponses ;
* de nombreux commentaires libres.

Pourtant, ces informations restent très difficiles à comparer dans le temps.

Chaque campagne recrée presque entièrement son propre contexte.

Les résultats deviennent rapidement incomparables.

Le studio finit souvent par refaire plusieurs fois les mêmes analyses sans réellement capitaliser sur les campagnes précédentes.

SignalLab transforme cette accumulation de questionnaires en une base de connaissances durable.

---

# 3. Philosophie du produit

SignalLab ne considère pas une campagne comme l'élément principal.

La campagne est simplement un moyen de collecter des observations.

Le véritable cœur du produit est constitué des **Measures**.

Une Measure représente un concept de game design que le studio souhaite suivre dans le temps.

Par exemple :

* Satisfaction des combats
* Lisibilité des déplacements
* Difficulté perçue
* Engagement narratif

Une campagne peut poser des questions différentes au fil du développement.

Pourtant, ces questions peuvent continuer à mesurer la même Measure.

Ainsi, SignalLab permet de comparer des campagnes dont les questionnaires ont évolué sans perdre la continuité analytique.

Les campagnes sont temporaires.

Les Measures sont permanentes.

---

# 4. Les principes fondamentaux

## Les données historiques sont sacrées

Une donnée collectée ne doit jamais être réécrite.

Une campagne figée reste figée.

Un formulaire figé reste figé.

Une réponse historique reste interprétable plusieurs années plus tard.

---

## Une information ne possède qu'une seule source de vérité

Chaque concept métier possède un propriétaire unique.

Une donnée ne doit jamais être dupliquée sans justification.

Les projections sont préférées aux copies.

---

## Les événements importants doivent atteindre l'utilisateur

SignalLab doit rendre visibles les événements métier qui nécessitent l'attention d'un utilisateur sans l'obliger à parcourir chaque contexte du produit.

Les notifications in-app constituent un accès transversal à ces événements.

Une notification ne remplace jamais l'entité métier qui l'a produite et n'en devient jamais la source de vérité.

Elle permet à l'utilisateur de comprendre qu'un événement pertinent s'est produit et d'accéder au contexte ou à l'action concernée.

Les mécanismes de diffusion externes ou temps réel ne sont pas nécessaires à ce principe.

---

## Les analytics sont construits sur les données métier

Les écrans analytiques ne créent aucune donnée.

Ils ne font qu'interpréter le modèle métier.

Le Domain Model constitue toujours la vérité.

---

## Les campagnes sont jetables

Une campagne représente une collecte ponctuelle.

Elle n'a pas vocation à devenir une entité permanente autour de laquelle tout le produit s'organise.

Une fois terminée, elle devient un élément de l'historique.

---

## Les Measures constituent la mémoire du produit

Les studios prennent des décisions sur des concepts de game design.

Ils ne prennent pas des décisions sur des questionnaires.

Les Measures assurent donc la continuité de cette connaissance.

---

## Les commentaires expliquent les chiffres

Les statistiques répondent à la question :

> Que se passe-t-il ?

Les commentaires répondent à la question :

> Pourquoi cela se passe-t-il ?

Les deux sont complémentaires.

---

## MAP enrichit l'analyse

Le Player Motivation Profile n'est pas un questionnaire supplémentaire.

Il constitue une grille de lecture permettant de segmenter les analyses.

MAP ne remplace jamais les réponses du participant.

Il permet simplement de mieux les interpréter.

---

# 5. Les utilisateurs

## Participant

Le participant rejoint une campagne, réalise éventuellement un assessment MAP, répond aux questions et partage son expérience.

Il ne manipule jamais de concepts analytiques.

Son parcours doit rester extrêmement simple.

---

## Research Lead

Le Research Lead conçoit les campagnes.

Il prépare les questionnaires.

Il choisit les Measures.

Il recrute les participants.

Il consulte les résultats.

---

## Game Designer

Le Game Designer souhaite comprendre l'impact de ses décisions.

Il consulte principalement les analytics.

Il compare différentes campagnes.

Il lit les commentaires associés aux résultats.

---

## Producer

Le Producer suit l'évolution globale du projet.

Il consulte principalement les tableaux de bord synthétiques.

---

## QA / Playtest Manager

Il organise les campagnes.

Il gère les participants.

Il suit les taux de participation.

Il ne réalise pas forcément les analyses.

---

# 6. Les grands workflows

Le fonctionnement général de SignalLab suit toujours le même cycle.

Créer un projet.

Définir les Measures.

Ajouter les versions du jeu.

Déclarer les builds.

Créer une campagne.

Préparer le recrutement.

Préparer le contexte.

Préparer le questionnaire.

Lancer la campagne.

Collecter les réponses.

Comparer les résultats.

Prendre des décisions de game design.

Créer une nouvelle campagne.

Le cycle recommence.

---

# 7. Les grands espaces du produit

Le produit est organisé autour de quelques espaces clairement identifiés.

Participant Space

Organisation Space

Project Space

Campaign Space

Analytics Space

Settings

Chaque espace possède une responsabilité claire.

L'utilisateur doit toujours comprendre immédiatement dans quel contexte il travaille.

---

# 8. Ce que SignalLab n'est pas

SignalLab n'est pas un constructeur générique de formulaires.

SignalLab n'est pas un clone de SurveyMonkey.

SignalLab n'est pas un dashboard builder.

SignalLab n'est pas un outil de Business Intelligence.

SignalLab n'est pas un réseau social pour testeurs.

SignalLab n'est pas un gestionnaire de projet.

SignalLab n'est pas une plateforme de stockage de builds.

SignalLab est un outil spécialisé de User Research destiné à aider les studios à comprendre leurs joueurs.

---

# 9. Les objectifs UX

Le produit doit rester simple malgré la richesse du modèle métier.

Les interfaces doivent guider l'utilisateur plutôt que lui exposer toute la complexité du domaine.

Les écrans doivent être organisés selon les intentions des utilisateurs et non selon la structure de la base de données.

Créer une campagne doit être rapide.

Comparer deux builds doit être naturel.

Retrouver une ancienne campagne doit être immédiat.

Comprendre une régression de gameplay doit demander le moins de manipulation possible.

Le produit doit privilégier la compréhension plutôt que la multiplication des graphiques.

---

# 10. Les principes d'évolution

Toute nouvelle fonctionnalité proposée doit être évaluée selon les questions suivantes :

Respecte-t-elle la philosophie des Measures ?

Améliore-t-elle réellement la compréhension des données ?

Conserve-t-elle la continuité historique ?

Évite-t-elle les duplications de données ?

Respecte-t-elle le Domain Model ?

Simplifie-t-elle le travail du studio plutôt que de l'alourdir ?

Ajoute-t-elle une vraie valeur pour le User Research, ou seulement une nouvelle fonctionnalité ?

Lorsqu'une proposition entre en conflit avec ces principes, les principes priment toujours.

---

# 11. Relation avec le Domain Model

Ce document décrit **l'intention**.

Le Domain Model décrit **le fonctionnement métier**.

Aucun choix d'interface, d'architecture logicielle ou de développement ne doit contredire ces deux documents.

Toute évolution du produit doit pouvoir être justifiée à la fois par la vision décrite ici et par les règles du Domain Model.

Ensemble, ces deux documents constituent la base de conception de SignalLab MVP.
