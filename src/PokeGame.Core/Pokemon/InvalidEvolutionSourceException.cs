using PokeGame.Core.Evolutions;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidEvolutionSourceException : DomainException
{
  public InvalidEvolutionSourceException(Specimen specimen, Evolution evolution)
    : base("The Pokémon current form does not match the source form of the specified evolution.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["EvolutionId"] = evolution.EntityId;
    Data["ExpectedFormId"] = evolution.SourceId.EntityId;
    Data["AttemptedFormId"] = specimen.FormId.EntityId;
  }
}
