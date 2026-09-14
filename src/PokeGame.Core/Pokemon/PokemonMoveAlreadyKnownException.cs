using PokeGame.Core.Moves;

namespace PokeGame.Core.Pokemon;

public sealed class PokemonMoveAlreadyKnownException : ConflictException
{
  public PokemonMoveAlreadyKnownException(Specimen specimen, MoveId moveId)
    : base("The Pokémon already knows the specified move.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["MoveId"] = moveId.EntityId;
  }
}
