# Parité PokeCraft ↔ SkillCraft

Comparaison de `PokeCraftRPG/api` avec `SkillCraftRPG/api`.  
Objectif : amener PokéCraft à parité avec SkillCraft — ajouter les features manquantes, aligner les conventions, retirer ce qui n’existe pas chez SkillCraft.

Les domaines métier (Abilities/Moves/Species vs Castes/Talents) sont **volontairement différents** — ce n’est pas un gap de parité.

---

## Décisions prises

| Question | Décision | Motif |
|---|---|---|
| Renommage assemblies `PokeGame.Api.*` | **Non** | Pas de solution `PokeGame.Cms.*` (contrairement à `SkillCraft.Cms.*`) — le naming `PokeGame` / `PokeGame.Core` reste |
| Worlds `Description` → `HtmlContent` | **Non, jamais** | — |
| Content `Description` → `Summary`/`HtmlContent` | **Laisser tel quel** | `Description` = texte in-game façon jeux Pokémon ; pas besoin de Summary court ni de HTML |
| `tools/PokeGame.Tools.Seeding` | **Conserver** | Le tool équivalent existe dans la solution `SkillCraft.Cms.*` |

---

## Déjà à parité (ne pas toucher)

- Couches Core / Infrastructure / PostgreSQL / Tests + IntegrationTests
- Agrégats `IAuditable` + `IResource` + `IVersioned`, dual ID (int PK + Guid)
- Events `Create` / `Update` / `Delete` → table History (audit, pas event sourcing)
- `Optional<T>`, CreateOrReplace, PATCH, search via `SearchParameters`
- `[Authorize]` + `[RequireWorld]` + header `X-World`
- Permissions owner + `WorldLimit`
- Mapper manuel, `Db.*`, repos scoped, FluentValidation nested
- Naming projets `PokeGame` / `PokeGame.Core` / … (pas `*.Api.*`)
- Champ texte `Description` (Worlds + contenu jeu)
- Tool de seeding `tools/PokeGame.Tools.Seeding`

---

## 1. Features manquantes à ajouter

| # | Changement | Pourquoi |
|---|---|---|
| 1 | **`IdentityService.Register`** dans `AddPokeGameCore` | SkillCraft l’enregistre ; PokéCraft a le code Identity mais ne le branche pas en DI Core |
| 2 | **`SessionController`** (`GET /sessions`, `DELETE /sessions/{id}`) | Présent chez SkillCraft ; absent chez PokéCraft |
| 3 | **Identity sessions** : `SessionModel`, `ListActiveSessions`, `SignOutAccountCommand`, `SessionMapper`, `DeviceType`, `UserExperience` | Fichiers présents côté SkillCraft, absents côté PokéCraft |
| 4 | **`Queries/ReadAccountProfile`** (si le controller lit le profil via query dédiée) | SkillCraft a une query séparée ; vérifier l’alignement du flux profil |
| 5 | **`ResourceNotFoundException`** | SkillCraft l’utilise pour les FK manquantes (Talent requis, Script, etc.) ; utile dès que Species référence des Regions |
| 6 | **`IdentityException` dans `IsBadRequest`** | SkillCraft mappe Identity → 400 ; PokéCraft ne le fait pas |
| 7 | **Régional numbers dans les commandes Species** | Schéma/mapper/unicité OK ; handlers ne les appliquent pas encore (`TODO.md`) — feature métier PokéCraft, mais « à finir » |

---

## 2. Conventions SkillCraft à adopter

| # | Changement | Détail |
|---|---|---|
| 9 | **DbContext** : `PokemonContext` → `GameContext` (ou garder `Pokemon`) | SkillCraft : `GameContext` + schéma `Game`. PokéCraft : `PokemonContext` + schéma `Pokemon` — à trancher si on aligne le nom du contexte uniquement |
| 11 | **Validation slug World** : règle `.Key()` → `.Slug()` (comme SkillCraft) | Même sémantique, nom de règle aligné |
| 13 | **ExceptionHandler Validation** | SkillCraft construit une Error `"Validation failed."` ; PokéCraft passe l’exception brute — aligner le mapping |

**Hors convention (décidé de ne pas aligner) :**

- Naming projets `PokeGame.Api.*` — pas de Cms sister-solution
- `Description` → `HtmlContent` / `Summary` — texte in-game Pokémon
- `Key` slugs sur Ability/Move/Region/Species — feature métier (équivalent du `Tier` immuable sur Talent)

---

## 3. À retirer (présent chez PokéCraft, absent chez SkillCraft)

| # | Changement | Pourquoi |
|---|---|---|
| 14 | ~~**HTTP DELETE** sur Worlds / Abilities / Moves / Regions~~ | **fait** — retiré des controllers |
| 15 | ~~**`Actions.Delete`** + handlers `Delete*` + `*Service.DeleteAsync`~~ | **fait** — retiré |
| 16 | **Events `*Deleted` côté API surface** (optionnel) | Conservés avec `Remove` sur les repos (comme SkillCraft) — pas exposés en HTTP |

---

## 4. Hors scope « parité » (garder / domaine Pokémon)

- Domaines Abilities, Moves, Regions, Species (+ enums Type/Category/Growth/Egg)
- `NumberAlreadyUsedException`, validation Accuracy/Power/PP/Friendship/CatchRate
- Read by `key:{key}` et `number:{n}`
- Collections `RegionalNumbers` (spécificité Species)
- Schéma SQL `Pokemon` (équivalent de `Game`)
- Champ `Description` (texte in-game)
- Tool de seeding

---

## Ordre de travail suggéré

1. ~~**Nettoyage surface API** — retirer DELETE + `Actions.Delete` (#14–16)~~ **fait**
2. **Identity à parité** — Register DI, sessions, SignOut, ExceptionHandler (#1–4, #6, #13)
3. **Exceptions partagées** — `ResourceNotFoundException` (#5)
4. **Convention validation** — `.Slug()` pour les clés World (#11) ; optionnellement renommer `PokemonContext` (#9)
5. **Finir Species regional numbers** (#7) — feature PokéCraft, pas SkillCraft
