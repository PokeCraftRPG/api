using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Pokemon;

public sealed class CannotCatchEggPokemon : DomainException
{
  private const string ErrorMessage = "An egg Pokémon cannot be caught.";

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
  public byte EggCycles
  {
    get => (byte)Data[nameof(EggCycles)]!;
    private set => Data[nameof(EggCycles)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(PokemonId)] = PokemonId;
      error.Data[nameof(EggCycles)] = EggCycles;
      return error;
    }
  }

  public CannotCatchEggPokemon(Specimen specimen)
    : base(BuildMessage(specimen))
  {
    WorldId = specimen.WorldId.EntityId;
    PokemonId = specimen.EntityId;
    EggCycles = specimen.EggCycles;
  }

  private static string BuildMessage(Specimen specimen) => new ErrorMessageBuilder(ErrorMessage).Build();
}
