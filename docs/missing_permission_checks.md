# Missing Permission Checks

Hors Roster, un seul trou clair :

Commande Check manquant Raison
EvolvePokemon
Actions.Update sur inventory
Si trigger ItemUsed, UseItem + SaveAsync sans check (contrairement à CatchPokemon)
Le reste des commandes métier a les checks attendus sur les entités mutées. ClaimMemberInvitations n’a aucun check : commande système (claim à la création user), hors PermissionService.
