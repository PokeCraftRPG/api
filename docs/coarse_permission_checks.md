# Coarse Permission Checks

### Totaux Action × Agrégat

| Action                                   | Agrégat          | #        |
| ---------------------------------------- | ---------------- | -------- |
| Update                                   | Specimen         | 9        |
| Update                                   | Roster           | 7        |
| Update                                   | Variety          | 4        |
| Update                                   | Species          | 4        |
| Update                                   | TrainerInventory | 3        |
| Update                                   | Ability          | 2        |
| Update                                   | Move             | 2        |
| Update                                   | Region           | 2        |
| Update                                   | Trainer          | 2        |
| Update                                   | Form             | 2        |
| Update                                   | Evolution        | 2        |
| Update                                   | Item             | 2        |
| Update                                   | World            | 2        |
| Deposit                                  | Specimen         | 1        |
| Evolve                                   | Specimen         | 1        |
| Withdraw                                 | Specimen         | 1        |
| Accept / Decline / Cancel                | MemberInvitation | 1 chacun |
| InviteMember / Leave / Revoke / Transfer | World            | 1 chacun |
| Create\* / Upload                        | _(sans agrégat)_ | 1 chacun |

### Fautifs : même `Action` × même agrégat **2+ fois dans une commande**

(`IsAllowed(Update, entity)` ne regarde que world owner + world — le 2ᵉ check sur une autre instance est redondant.)

| Action | Agrégat  | Commande         | Checks                         |
| ------ | -------- | ---------------- | ------------------------------ |
| Update | Specimen | `SwapPokemon`    | `source`, `target`             |
| Update | Specimen | `TradePokemon`   | `source`, `target`             |
| Update | Roster   | `TradePokemon`   | `sourceRoster`, `targetRoster` |
| Update | Roster   | `ReceivePokemon` | `sourceRoster`, `targetRoster` |
