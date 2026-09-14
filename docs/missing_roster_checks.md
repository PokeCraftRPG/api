# Missing Roster Checks

### Checks sur `Roster` (tous `Actions.Update`)

| Commande         | Variables                                     |
| ---------------- | --------------------------------------------- |
| `CatchPokemon`   | `roster`                                      |
| `ReceivePokemon` | `sourceRoster` (si transfert), `targetRoster` |
| `ReleasePokemon` | `roster`                                      |
| `SwapPokemon`    | `roster`                                      |
| `TradePokemon`   | `sourceRoster`, `targetRoster`                |

### Commandes qui touchent le roster **sans** check

| Commande          | Opération         |
| ----------------- | ----------------- |
| `DepositPokemon`  | `roster.Deposit`  |
| `WithdrawPokemon` | `roster.Withdraw` |
