# M12 — Documentation and Portfolio

**Version :** 2.0  
**Statut :** 🟡 Draft  
**Dernière mise à jour :** 31/07/2026  
**Milestone précédente :** M11 — Deployment and Operations

---

> Cette Milestone finalise SignalLab en tant que produit, projet technique et réalisation de portfolio.
>
> Elle rassemble les éléments nécessaires à sa compréhension, son installation, son exploitation, sa démonstration et sa maintenance.

---

# Objectif

Produire une documentation complète ainsi que les supports permettant de présenter SignalLab dans un contexte académique, professionnel ou technique.

---

# Valeur produit

À la fin de cette Milestone, SignalLab peut être compris et utilisé sans dépendre exclusivement de la connaissance de son développeur initial.

Le projet peut également être présenté clairement dans un portfolio.

---

# Hors scope

Cette Milestone ne contient pas :

- nouvelles fonctionnalités métier ;
- évolution du Domain Model ;
- nouvelle architecture ;
- refonte visuelle ;
- fonctionnalités post-MVP.

---

# Principes

- La documentation décrit l'état réellement livré.
- Les documents produit et techniques conservent des responsabilités distinctes.
- Les fonctionnalités post-MVP sont clairement séparées du MVP.
- Les instructions sont reproductibles.
- Aucun secret ou donnée personnelle n'est publié.

---

# Specs

---

## SL-121 — Rédiger le README principal

### Description

Créer le README principal du repository.

### Contenu

- présentation de SignalLab ;
- problème résolu ;
- périmètre du MVP ;
- principales fonctionnalités ;
- technologies ;
- architecture générale ;
- prérequis ;
- installation locale ;
- lancement ;
- tests ;
- liens vers la documentation détaillée.

### Objectif produit

Présenter rapidement le projet à tout nouvel utilisateur ou contributeur.

### Critères d'acceptation

Le README correspond à l'application réellement livrée.

Les instructions d'installation sont vérifiées depuis un environnement propre.

### Definition of Done

Le README principal est complet et publié.

---

## SL-122 — Finaliser la documentation d'architecture

### Description

Mettre à jour l'ensemble de la documentation d'architecture afin qu'elle corresponde à l'implémentation finale.

### Cette Spec couvre notamment

- architecture Backend ;
- architecture Frontend ;
- Domain Model ;
- navigation ;
- persistance ;
- authentification et autorisation ;
- Data Lifecycle ;
- infrastructure ;
- ADR ;
- diagrammes principaux.

### Objectif produit

Permettre de comprendre les responsabilités et les décisions structurantes de SignalLab.

### Critères d'acceptation

Les diagrammes sont à jour.

Les divergences entre documents sont supprimées ou explicitement justifiées.

Les ADR importantes sont présentes.

### Definition of Done

La documentation d'architecture est complète.

---

## SL-123 — Documenter les APIs

### Description

Finaliser la documentation des endpoints et contrats du Backend.

### Cette Spec comprend notamment

- documentation OpenAPI ;
- authentification requise ;
- paramètres ;
- DTO ;
- réponses ;
- Problem Details ;
- exemples utiles ;
- conventions de pagination et filtrage.

### Objectif produit

Faciliter la maintenance et l'intégration du Frontend.

### Critères d'acceptation

Toutes les APIs du MVP sont documentées.

Les contrats exposés correspondent au comportement réel.

Aucune entité Domain n'est présentée comme contrat public.

### Definition of Done

La documentation API est finalisée.

---

## SL-124 — Rédiger le guide utilisateur

### Description

Créer un guide d'utilisation destiné aux Participants et aux membres des Organizations.

### Parcours couverts

- compte et profil ;
- Organizations ;
- Projects ;
- Builds ;
- Measures ;
- Form Templates ;
- Campaigns ;
- Participation ;
- Results ;
- Analysis ;
- MAP Profile.

### Objectif produit

Permettre une prise en main autonome des fonctionnalités principales.

### Critères d'acceptation

Les parcours essentiels sont documentés.

Les différences entre rôles sont explicites.

Les limitations du MVP sont indiquées.

### Definition of Done

Le guide utilisateur est disponible.

---

## SL-125 — Rédiger le guide d'exploitation

### Description

Documenter l'installation, le déploiement et la maintenance de SignalLab.

### Cette Spec comprend notamment

- environnements ;
- variables de configuration ;
- secrets ;
- déploiement ;
- migrations ;
- sauvegardes ;
- restaurations ;
- supervision ;
- diagnostic ;
- retour arrière.

### Objectif produit

Permettre de maintenir la plateforme sans dépendre d'informations implicites.

### Critères d'acceptation

Les procédures ont été exécutées au moins une fois.

Les responsabilités opérationnelles sont claires.

Aucun secret réel n'est documenté.

### Definition of Done

Le guide d'exploitation est complet.

---

## SL-126 — Préparer le Portfolio

### Description

Créer les supports permettant de présenter SignalLab comme réalisation produit et technique.

### Contenu

- contexte ;
- problème ;
- vision produit ;
- démarche de conception ;
- Domain Model ;
- architecture ;
- choix techniques ;
- parcours principal ;
- fonctionnalités livrées ;
- captures et démonstration ;
- difficultés rencontrées ;
- arbitrages MVP ;
- perspectives post-MVP.

### Objectif produit

Mettre en valeur la cohérence entre recherche utilisateur, conception produit et implémentation technique.

### Critères d'acceptation

Le portfolio distingue clairement MVP et vision long terme.

Les éléments visuels correspondent à la version finale.

Les choix structurants sont expliqués sans exposer d'informations sensibles.

### Definition of Done

Le portfolio est finalisé.

---

## SL-127 — Nettoyer et préparer le repository

### Description

Préparer le repository pour sa conservation ou sa diffusion.

### Cette Spec comprend notamment

- suppression des fichiers temporaires ;
- vérification du `.gitignore` ;
- suppression des secrets et exemples sensibles ;
- harmonisation de l'arborescence ;
- vérification des licences ;
- vérification des scripts ;
- archivage des documents obsolètes lorsque nécessaire.

### Objectif produit

Garantir un projet propre, compréhensible et maintenable.

### Critères d'acceptation

L'arborescence est cohérente.

Aucun secret n'est versionné.

Les commandes documentées fonctionnent.

Les conventions sont respectées.

### Definition of Done

Le repository est prêt à être partagé.

---

## SL-128 — Finaliser SignalLab

### Description

Clôturer officiellement le développement du MVP.

Cette Spec comprend :

- revue fonctionnelle globale ;
- revue des critères du MVP ;
- validation de la documentation ;
- validation du déploiement ;
- validation du repository ;
- validation du portfolio ;
- liste explicite des limitations et évolutions post-MVP.

### Objectif produit

Livrer une version finale cohérente, démontrable et documentée de SignalLab.

### Critères d'acceptation

Toutes les Milestones précédentes sont validées.

Le cycle complet du MVP fonctionne.

La documentation correspond à la version déployée.

Les limitations connues sont documentées.

Le portfolio est prêt.

### Definition of Done

SignalLab MVP est officiellement terminé.

---

# Fin de Milestone

À la fin de M12, SignalLab dispose :

- d'un MVP fonctionnel déployé ;
- d'une documentation produit et technique complète ;
- d'un guide utilisateur ;
- d'un guide d'exploitation ;
- d'un repository propre ;
- d'un portfolio finalisé ;
- d'une séparation explicite entre le MVP livré et la vision long terme.

Le projet SignalLab est officiellement finalisé dans son périmètre MVP.
