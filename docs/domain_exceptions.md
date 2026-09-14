# Domain Exceptions

Aucun handler Command/Query ne catch d’exception : tout remonte via le bus. Seules `DomainException` (422), `ConflictException` (409) et `MemberInvitationExpiredException` (410) sont mappées ensuite. Ci-dessous, les throws d’`AggregateRoot` qui finissent en **500**.

### `WorldMismatchException` (`ArgumentException`)

Levée via `ThrowIfMismatch` dans : `Roster`, `Specimen`, `Evolution`, `Form`, `Variety`, `TrainerInventory`, `PokemonSpecies`, `Trainer`, `Item`.

### `ArgumentException`

| Aggregate  | Méthodes                                                  |
| ---------- | --------------------------------------------------------- |
| `Roster`   | `Add`, `Deposit`, `Remove`, `Replace`, `Swap`, `Withdraw` |
| `Specimen` | ctor (variety/form), `Evolve` (form/variety attendus)     |

### `ArgumentOutOfRangeException`

| Aggregate          | Méthodes                                                                       |
| ------------------ | ------------------------------------------------------------------------------ |
| `Asset`            | ctor (`kind`, `duration`)                                                      |
| `Evolution`        | ctor (`trigger`), `SetConditions` (`gender`, `timeOfDay`)                      |
| `Form`             | ctor (`category`)                                                              |
| `Item`             | ctor (`category`)                                                              |
| `MemberInvitation` | ctor (`expiresOn`)                                                             |
| `Move`             | ctor (`type`, `category`)                                                      |
| `PokemonSpecies`   | ctor (`category`)                                                              |
| `Specimen`         | ctor (`gender`, `teraType`, `abilitySlot`), `Evolve`, `LearnMove`, `SetStatus` |
| `Trainer`          | `SetGender`, `SetPartyLimit`                                                   |
| `TrainerInventory` | `AdjustQuantity` (`ThrowIfZero`)                                               |

### `InvalidOperationException`

| Aggregate          | Méthodes                               |
| ------------------ | -------------------------------------- |
| Plusieurs          | getters `Key` / champs non initialisés |
| `Specimen`         | ctor (egg + XP)                        |
| `PokemonSpecies`   | `FindRegionalNumber`                   |
| `Variety`          | `FindMove`                             |
| `Roster`           | `Replace` / `Swap` (même Pokémon)      |
| `MemberInvitation` | `Claim` (déjà claimé)                  |

### `NotImplementedException`

| Aggregate | Méthode        | Note                         |
| --------- | -------------- | ---------------------------- |
| `Roster`  | `Swap` (l.185) | les deux en party — TODO 409 |

Point le plus critique côté runtime : `Roster.Swap` → `NotImplementedException`. Le plus gros volume : `Roster` (`ArgumentException`) + `WorldMismatchException` partout.
