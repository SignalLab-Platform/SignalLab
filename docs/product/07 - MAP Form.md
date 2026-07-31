# SignalLab — MAP Questionnaire

# Version 1.0

> **Identifiant de version :** `MAP-V1-FR-SL`  
> **Statut :** Spécification fonctionnelle — traduction française à valider  
> **Instrument source :** *Validating Motives of Autonomous Players (MAP) inventory: a bottom-up model of general motivational factors to videogame play*  
> **Auteurs :** Jukka Vahlo et Kai Tuuri  
> **Publication :** *User Modeling and User-Adapted Interaction*, 2025, volume 35, article 10  
> **DOI :** `10.1007/s11257-025-09431-7`  
> **Source :** https://link.springer.com/article/10.1007/s11257-025-09431-7

---

# 1. Objectif

Ce document spécifie la première version du questionnaire MAP intégrée à SignalLab.

MAP signifie **Motives of Autonomous Players**.

L'instrument mesure les raisons générales pour lesquelles une personne joue à des jeux vidéo, indépendamment :

- d'un jeu précis ;
- d'un genre précis ;
- d'une Campaign SignalLab ;
- d'un contexte professionnel ou scolaire particulier.

Le résultat n'est pas un type de joueur exclusif.

Il s'agit d'un profil composé de neuf scores motivationnels indépendants.

---

# 2. Statut scientifique de cette version

L'article source valide un instrument anglophone de :

- 34 items ;
- 9 dimensions ;
- une échelle de réponse en 7 points ;
- un calcul par moyenne des items de chaque dimension.

La présente version française est une **adaptation sémantique destinée au produit SignalLab**.

Elle n'est pas encore une traduction psychométriquement validée.

Avant de l'utiliser pour produire des affirmations scientifiques comparables à l'étude source, il faudra réaliser au minimum :

1. une traduction indépendante par plusieurs personnes ;
2. une rétrotraduction vers l'anglais ;
3. une revue par des experts bilingues ;
4. des entretiens cognitifs avec des joueurs francophones ;
5. un pilote quantitatif ;
6. une vérification de fiabilité et de structure factorielle.

Les formulations françaises ci-dessous sont volontairement paraphrasées afin de préserver le sens des items sans prétendre constituer une traduction officielle des auteurs.

---

# 3. Structure du modèle

| Clé | Dimension source | Libellé français SignalLab | Items | Nombre |
|---|---|---|---:|---:|
| `immersive_agency` | Immersive Agency | Agentivité immersive | `IA01` à `IA04` | 4 |
| `competitive_mastery` | Competitive Mastery | Maîtrise compétitive | `CM01` à `CM04` | 4 |
| `social` | Social | Motivation sociale | `SO01` à `SO04` | 4 |
| `utility` | Utility | Utilité cognitive | `UT01` à `UT03` | 3 |
| `nostalgia` | Nostalgia | Nostalgie | `NO01` à `NO04` | 4 |
| `addiction` | Addiction | Usage compulsif autodéclaré | `AD01` à `AD04` | 4 |
| `affective_engagement` | Affective Engagement | Engagement affectif positif | `AE01` à `AE04` | 4 |
| `boredom` | Boredom | Ennui | `BO01` à `BO03` | 3 |
| `escapism` | Escapism | Évasion | `ES01` à `ES04` | 4 |

Total : **34 items**.

## 3.1 Avertissement sur la dimension Addiction

La dimension source nommée `Addiction` mesure des déclarations subjectives liées à une difficulté perçue à contrôler le jeu.

Dans SignalLab :

- elle ne constitue pas un diagnostic ;
- elle ne permet pas de conclure à un trouble clinique ;
- elle ne doit jamais être présentée comme une évaluation médicale ;
- son libellé d'interface recommandé est **Usage compulsif autodéclaré** ;
- la clé et le nom scientifique source restent conservés pour la traçabilité.

---

# 4. Présentation du questionnaire

## 4.1 Introduction affichée au User

> Les affirmations suivantes portent sur les raisons pour lesquelles vous jouez généralement aux jeux vidéo. Pour chacune, indiquez dans quelle mesure elle vous correspond.

Chaque item complète implicitement la phrase :

> **Je joue aux jeux vidéo…**

## 4.2 Échelle de réponse

| Valeur | Libellé français |
|---:|---|
| 1 | Pas du tout d'accord |
| 2 | Pas d'accord |
| 3 | Plutôt pas d'accord |
| 4 | Ni d'accord ni pas d'accord |
| 5 | Plutôt d'accord |
| 6 | D'accord |
| 7 | Tout à fait d'accord |

## 4.3 Règles de réponse MVP

- Chaque item est obligatoire.
- Une seule réponse est autorisée par item.
- Les valeurs valides sont les entiers de `1` à `7`.
- Aucun item n'utilise de codage inversé.
- L'ordre des items est défini par la MAPModelVersion.
- Une réponse peut être modifiée tant que le MAPAssessment reste `Draft`.
- Après soumission, toutes les réponses deviennent immuables.

---

# 5. Questionnaire complet — adaptation française SignalLab

## 5.1 Agentivité immersive

| Ordre source | Code | Formulation française |
|---:|---|---|
| 1 | `IA01` | …pour interagir avec des personnages du jeu. |
| 2 | `IA02` | …pour m'immerger dans un univers imaginaire. |
| 3 | `IA03` | …parce que le jeu me permet de m'exprimer. |
| 4 | `IA04` | …parce que je peux y prendre des décisions qui ont du sens. |

## 5.2 Maîtrise compétitive

| Ordre source | Code | Formulation française |
|---:|---|---|
| 5 | `CM01` | …pour dépasser mes adversaires. |
| 6 | `CM02` | …pour progresser dans un classement compétitif. |
| 7 | `CM03` | …pour continuer à m'améliorer, même lorsque cela demande beaucoup de temps. |
| 8 | `CM04` | …pour m'entraîner avec persévérance jusqu'à maîtriser le jeu. |

## 5.3 Motivation sociale

| Ordre source | Code | Formulation française |
|---:|---|---|
| 9 | `SO01` | …pour créer des liens avec d'autres personnes. |
| 10 | `SO02` | …parce que j'apprécie les échanges avec d'autres joueurs. |
| 11 | `SO03` | …parce que mes amis jouent également. |
| 12 | `SO04` | …parce que jouer renforce ma proximité avec d'autres personnes. |

## 5.4 Utilité cognitive

| Ordre source | Code | Formulation française |
|---:|---|---|
| 13 | `UT01` | …pour stimuler mes capacités mentales. |
| 14 | `UT02` | …pour entretenir ma mémoire. |
| 15 | `UT03` | …pour garder l'esprit alerte. |

## 5.5 Nostalgie

| Ordre source | Code | Formulation française |
|---:|---|---|
| 16 | `NO01` | …parce que cela fait revenir des souvenirs. |
| 17 | `NO02` | …parce que cela me procure un sentiment de nostalgie. |
| 18 | `NO03` | …parce que cela me rappelle de bons moments de ma vie. |
| 19 | `NO04` | …parce que je garde de bons souvenirs liés aux jeux vidéo. |

## 5.6 Usage compulsif autodéclaré

| Ordre source | Code | Formulation française |
|---:|---|---|
| 20 | `AD01` | …parce que j'ai du mal à m'arrêter. |
| 21 | `AD02` | …parce que j'ai le sentiment d'en être dépendant. |
| 22 | `AD03` | …même lorsque contrôler mon envie de jouer est difficile. |
| 23 | `AD04` | …parce que je pense souvent au fait de jouer. |

## 5.7 Engagement affectif positif

| Ordre source | Code | Formulation française |
|---:|---|---|
| 24 | `AE01` | …parce que jouer me procure du plaisir. |
| 25 | `AE02` | …parce que jouer me divertit. |
| 26 | `AE03` | …parce que jouer est amusant. |
| 27 | `AE04` | …parce que les jeux vidéo m'intéressent. |

## 5.8 Ennui

| Ordre source | Code | Formulation française |
|---:|---|---|
| 28 | `BO01` | …parce que je m'ennuie. |
| 29 | `BO02` | …parce que je n'ai rien d'autre à faire. |
| 30 | `BO03` | …pour faire passer le temps. |

## 5.9 Évasion

| Ordre source | Code | Formulation française |
|---:|---|---|
| 31 | `ES01` | …pour prendre de la distance avec ma routine quotidienne. |
| 32 | `ES02` | …pour oublier temporairement ce qui m'entoure. |
| 33 | `ES03` | …pour penser à autre chose. |
| 34 | `ES04` | …pour détourner mon attention de la vie réelle. |

---

# 6. Calcul du profil

## 6.1 Principe général

Le profil MAP est un vecteur de neuf scores.

Il n'existe pas de score MAP total dans la V1.

```text
MAPProfile = {
    immersive_agency,
    competitive_mastery,
    social,
    utility,
    nostalgia,
    addiction,
    affective_engagement,
    boredom,
    escapism
}
```

## 6.2 Score brut d'une dimension

Pour chaque dimension `d` :

```text
Score(d) = Somme des réponses des items de d / Nombre d'items de d
```

Soit formellement :

```text
Sᵤ,ᵈ = (Σ rᵤ,ᵢ) / nᵈ
```

avec :

- `Sᵤ,ᵈ` : score du User `u` pour la dimension `d` ;
- `rᵤ,ᵢ` : réponse du User à l'item `i` ;
- `nᵈ` : nombre d'items de la dimension.

Chaque score reste compris entre `1.00` et `7.00`.

## 6.3 Formules par dimension

```text
immersive_agency = (IA01 + IA02 + IA03 + IA04) / 4
competitive_mastery = (CM01 + CM02 + CM03 + CM04) / 4
social = (SO01 + SO02 + SO03 + SO04) / 4
utility = (UT01 + UT02 + UT03) / 3
nostalgia = (NO01 + NO02 + NO03 + NO04) / 4
addiction = (AD01 + AD02 + AD03 + AD04) / 4
affective_engagement = (AE01 + AE02 + AE03 + AE04) / 4
boredom = (BO01 + BO02 + BO03) / 3
escapism = (ES01 + ES02 + ES03 + ES04) / 4
```

## 6.4 Précision

Décision SignalLab V1 :

- le Backend calcule avec une précision décimale suffisante ;
- la valeur persistée conserve au minimum quatre décimales ;
- l'interface affiche deux décimales ;
- aucun arrondi intermédiaire n'est appliqué ;
- l'arrondi d'affichage ne modifie pas la valeur persistée.

## 6.5 Données manquantes

L'article source calcule les moyennes à partir des items correspondants.

Pour le produit SignalLab V1, tous les items sont obligatoires.

Par conséquent :

- aucun score n'est calculé avec une réponse manquante ;
- aucun remplacement statistique n'est effectué ;
- aucun prorata n'est autorisé ;
- la soumission est refusée tant que les 34 réponses ne sont pas valides.

## 6.6 Codage inversé

Aucun item de la version validée n'est inversé.

Les réponses sont utilisées directement avec leur valeur de `1` à `7`.

---

# 7. Standardisation future pour les Analyses — post-MVP

L'étude source utilise également des scores standardisés pour comparer des groupes.

Pour une population de référence définie :

```text
Zᵤ,ᵈ = (Sᵤ,ᵈ - μᵈ) / σᵈ
```

avec :

- `Sᵤ,ᵈ` : score brut du User pour la dimension ;
- `μᵈ` : moyenne versionnée de la dimension dans la population de référence ;
- `σᵈ` : écart-type versionné de cette dimension.

## 7.1 Décision MVP

Le MVP n'implémente aucun z-score MAP ni population normative de référence.

Les segmentations utilisent exclusivement les neuf scores bruts continus `1–7` du MAPProfile actuel partagé.

La standardisation ne pourra être introduite qu'avec :

- une population de référence explicitement définie ;
- une version immuable de cette référence ;
- une provenance et une taille de population documentées ;
- une compatibilité explicite avec la MAPModelVersion ;
- une méthode de recalcul traçable.

Même après son introduction :

- les scores bruts `1–7` resteront la source de vérité du MAPProfile ;
- les z-scores resteront des projections analytiques recalculables ;
- aucun z-score ne sera persisté dans le profil personnel ;
- une comparaison identifiera toujours la référence utilisée.

Aucune norme francophone validée n'est définie dans la V1.

---

# 8. Interprétation du profil

## 8.1 Absence de typologie exclusive

SignalLab ne transforme pas les neuf scores en un unique « type de joueur ».

Un User peut présenter simultanément des scores élevés sur plusieurs dimensions.

## 8.2 Classement descriptif

L'interface peut classer les dimensions du score le plus élevé au plus faible afin de faciliter la lecture.

Ce classement :

- ne change pas les scores ;
- ne crée pas de catégorie ;
- ne permet pas de conclure qu'une dimension est absente ;
- reste purement descriptif.

## 8.3 Seuils

La V1 ne définit aucun seuil scientifique de type :

- faible ;
- moyen ;
- élevé ;
- critique.

De tels seuils nécessiteraient une population normative et une validation spécifique.

L'interface affiche donc les valeurs continues sur l'échelle `1–7`.

## 8.4 Absence de score global

Aucune moyenne des neuf dimensions n'est calculée.

Les dimensions représentent des motifs distincts et ne doivent pas être agrégées en un score général de motivation.

---

# 9. Calcul atomique dans SignalLab

Lors de la soumission d'un MAPAssessment :

```text
Validation des 34 réponses
+
Calcul des 9 moyennes
+
Création ou mise à jour du MAPProfile
+
Enregistrement de la provenance
+
Passage de l'assessment à Submitted
```

L'ensemble constitue une seule opération atomique.

Si une étape échoue :

- l'assessment reste `Draft` ;
- le MAPProfile actuel reste inchangé ;
- aucun résultat partiel n'est persisté.

---

# 10. Provenance persistée

Chaque MAPProfile V1 conserve au minimum :

- `SourceAssessmentId` ;
- `MAPModelVersionId` ;
- les neuf scores ;
- `CalculatedAt` ;
- la préférence de partage actuelle.

Un nouveau calcul remplace les scores et la provenance du profil actuel sans réinitialiser la préférence de partage choisie par le User.

Un premier MAPProfile est `Private` par défaut.

Chaque MAPAssessment conserve :

- les 34 réponses ;
- la version exacte du questionnaire ;
- l'ordre et les identifiants des items ;
- `StartedAt` ;
- `SubmittedAt` ou `AbandonedAt` ;
- son statut final.

La MAPModelVersion doit permettre de recalculer exactement le profil à partir de l'assessment historique.

---

# 11. Identifiants stables des items

Les identifiants V1 sont immuables.

Une modification sémantique d'un item nécessite une nouvelle MAPModelVersion et un nouvel identifiant de version.

Les codes d'items restent :

```text
IA01 IA02 IA03 IA04
CM01 CM02 CM03 CM04
SO01 SO02 SO03 SO04
UT01 UT02 UT03
NO01 NO02 NO03 NO04
AD01 AD02 AD03 AD04
AE01 AE02 AE03 AE04
BO01 BO02 BO03
ES01 ES02 ES03 ES04
```

La traduction affichée peut être corrigée uniquement avant publication de la MAPModelVersion.

Une fois la version publiée, ses formulations deviennent immuables.

---

# 12. Compatibilité avec les futures versions

Chaque MAPAssessment et MAPProfile conserve son `MAPModelVersionId`.

Dans le MVP :

- les scores de deux versions différentes ne sont pas comparés automatiquement ;
- aucune conversion n'est appliquée ;
- un segment analytique MAP utilise une seule version ;
- les profils d'une autre version apparaissent comme MAP indisponible pour ce segment.

Une future compatibilité devra être explicitement documentée et versionnée.

---

# 13. Critères d'acceptation fonctionnels

La MAP Questionnaire V1 est conforme lorsque :

- les 34 items sont présents ;
- chaque item possède un identifiant stable ;
- les 9 dimensions utilisent le mapping défini ;
- l'échelle accepte uniquement les valeurs `1–7` ;
- tous les items sont obligatoires ;
- aucun item n'est inversé ;
- chaque score est la moyenne arithmétique de ses items ;
- aucun score total n'est produit ;
- aucun seuil ou diagnostic n'est inféré ;
- l'assessment soumis est immuable ;
- le profil conserve sa version et son assessment source ;
- le calcul est déterministe et atomique ;
- un premier profil est privé par défaut ;
- un recalcul conserve le choix de partage existant ;
- aucune CampaignParticipation, Submission, Response ou SavedAnalysis ne stocke de snapshot MAP ;
- les Analyses ne distinguent jamais publiquement profil absent, privé ou incompatible ;
- aucun z-score MAP n'est calculé dans le MVP ;
- l'interface indique que la traduction française doit encore être validée ;
- la dimension Addiction n'est jamais présentée comme un diagnostic clinique.

---

# 14. Validation française recommandée

## Étape 1 — Traduction

- deux traductions françaises indépendantes ;
- consolidation par consensus ;
- conservation de la relation item source ↔ item français.

## Étape 2 — Rétrotraduction

- traduction française vers l'anglais par une personne n'ayant pas vu les items sources ;
- comparaison avec le questionnaire original ;
- correction des écarts sémantiques.

## Étape 3 — Entretiens cognitifs

Tester notamment :

- la compréhension des formulations ;
- l'ambiguïté du référent « jeux vidéo » ;
- la distinction entre compétition et maîtrise ;
- la perception des items Addiction ;
- la différence entre Ennui et Évasion.

## Étape 4 — Pilote quantitatif

Vérifier :

- la distribution des réponses ;
- les effets plancher et plafond ;
- la cohérence interne de chaque dimension ;
- les corrélations inter-dimensions ;
- les items mal compris ou peu discriminants.

## Étape 5 — Validation psychométrique

Réaliser idéalement :

- une analyse factorielle confirmatoire à neuf facteurs ;
- la Composite Reliability ;
- l'Average Variance Extracted ;
- la validité discriminante ;
- des tests d'invariance entre populations pertinentes.

La version française ne doit être déclarée « validée » qu'après réussite de ce processus.

---

# 15. Résultats méthodologiques de l'étude source

L'étude source rapporte notamment :

- un modèle final à 34 items et 9 facteurs ;
- 61 % de variance expliquée ;
- une validation confirmatoire sur un échantillon américain ;
- des indices d'ajustement jugés acceptables à bons ;
- une validation convergente et discriminante des neuf facteurs.

Ces résultats soutiennent l'utilisation de la structure source.

Ils ne valident pas automatiquement la traduction française SignalLab.

---

# 16. Référence et attribution

Vahlo, J., & Tuuri, K. (2025). *Validating Motives of Autonomous Players (MAP) inventory: a bottom-up model of general motivational factors to videogame play*. User Modeling and User-Adapted Interaction, 35, article 10. https://doi.org/10.1007/s11257-025-09431-7

L'article est publié en Open Access sous licence Creative Commons Attribution 4.0.

La formulation anglaise officielle des items se trouve dans l'annexe de l'article source.

Le présent document conserve la structure, les dimensions et la méthode de scoring, mais utilise une adaptation française SignalLab qui doit être validée avant tout usage scientifique formel.
