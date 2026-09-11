namespace PokeGame.Core.Pokemon;

public sealed class PokemonAlreadyOwnedException : ConflictException
{
  public PokemonAlreadyOwnedException(Specimen specimen)
    : base("The specified Pokémon already has an owner.")
  {
    PokemonOwnership ownership = specimen.Ownership ?? throw new ArgumentException("The ownership is required.", nameof(specimen));
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["TrainerId"] = ownership.TrainerId.EntityId;
  }
}
