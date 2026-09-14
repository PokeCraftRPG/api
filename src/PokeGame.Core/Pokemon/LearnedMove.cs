using PokeGame.Core.Moves;

namespace PokeGame.Core.Pokemon;

public sealed record LearnedMove(MoveId MoveId, bool IsInMoveset);
