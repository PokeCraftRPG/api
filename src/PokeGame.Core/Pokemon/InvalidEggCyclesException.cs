using PokeGame.Core.Species;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidEggCyclesException : DomainException
{
  public InvalidEggCyclesException(Specimen specimen, PokemonSpecies species, byte attemptedEggCycles)
    : base("The egg cycles cannot exceed the species egg cycles.")
  {
    Data["WorldId"] = specimen.WorldId.EntityId;
    Data["PokemonId"] = specimen.EntityId;
    Data["SpeciesId"] = species.EntityId;
    Data["MaximumEggCycles"] = species.Eggs.Cycles;
    Data["AttemptedEggCycles"] = attemptedEggCycles;
    Data["PropertyName"] = nameof(Specimen.EggCycles);
  }

  public static void ThrowIfNotValid(Specimen specimen, PokemonSpecies species, byte attemptedEggCycles)
  {
    if (attemptedEggCycles > species.Eggs.Cycles)
    {
      throw new InvalidEggCyclesException(specimen, species, attemptedEggCycles);
    }
  }
}
