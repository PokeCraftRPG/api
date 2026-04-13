using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Pokemon;

public class CannotReleaseEggPokemonException : DomainException
{
  private const string ErrorMessage = "A Pokémon that is still an egg cannot be released.";

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
  public byte? EggCycles
  {
    get => (byte?)Data[nameof(EggCycles)];
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

  public CannotReleaseEggPokemonException(Specimen specimen) : base(BuildMessage(specimen))
  {
    WorldId = specimen.WorldId.ToGuid();
    PokemonId = specimen.Id.EntityId;
    EggCycles = specimen.EggCycles?.Value;
  }

  private static string BuildMessage(Specimen specimen) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.ToGuid())
    .AddData(nameof(PokemonId), specimen.EntityId)
    .AddData(nameof(EggCycles), specimen.EggCycles, "<null>")
    .Build();
}
