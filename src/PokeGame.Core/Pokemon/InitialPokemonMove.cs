using PokeGame.Core.Moves;

namespace PokeGame.Core.Pokemon;

public sealed record InitialPokemonMove(MoveId MoveId, bool IsInMoveset);
