using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Species;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidEggCyclesException : DomainException
{
  private const string ErrorMessage = "The egg cycles cannot exceed the species egg cycles.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid PokemonId
  {
    get => (Guid)Data[nameof(PokemonId)]!;
    private set => Data[nameof(PokemonId)] = value;
  }
  public Guid SpeciesId
  {
    get => (Guid)Data[nameof(SpeciesId)]!;
    private set => Data[nameof(SpeciesId)] = value;
  }
  public byte MaximumEggCycles
  {
    get => (byte)Data[nameof(MaximumEggCycles)]!;
    private set => Data[nameof(MaximumEggCycles)] = value;
  }
  public byte AttemptedEggCycles
  {
    get => (byte)Data[nameof(AttemptedEggCycles)]!;
    private set => Data[nameof(AttemptedEggCycles)] = value;
  }
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(PokemonId)] = PokemonId;
      error.Data[nameof(SpeciesId)] = SpeciesId;
      error.Data[nameof(MaximumEggCycles)] = MaximumEggCycles;
      error.Data[nameof(AttemptedEggCycles)] = AttemptedEggCycles;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidEggCyclesException(Specimen specimen, PokemonSpecies species, byte attemptedEggCycles)
    : base(BuildMessage(specimen, species, attemptedEggCycles))
  {
    WorldId = specimen.WorldId.EntityId;
    PokemonId = specimen.EntityId;
    SpeciesId = species.EntityId;
    MaximumEggCycles = species.Eggs.Cycles;
    AttemptedEggCycles = attemptedEggCycles;
    PropertyName = nameof(Specimen.EggCycles);
  }

  private static string BuildMessage(Specimen specimen, PokemonSpecies species, byte attemptedEggCycles) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.EntityId)
    .AddData(nameof(PokemonId), specimen.EntityId)
    .AddData(nameof(SpeciesId), species.EntityId)
    .AddData(nameof(MaximumEggCycles), species.Eggs.Cycles)
    .AddData(nameof(AttemptedEggCycles), attemptedEggCycles)
    .AddData(nameof(PropertyName), nameof(Specimen.EggCycles))
    .Build();
}
