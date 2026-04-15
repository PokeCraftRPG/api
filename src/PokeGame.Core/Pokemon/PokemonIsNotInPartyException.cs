using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Pokemon;

public class PokemonIsNotInPartyException : DomainException
{
  private const string ErrorMessage = "The specified Pokémon is not in its trainer party.";

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

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(PokemonId)] = PokemonId;
      return error;
    }
  }

  public PokemonIsNotInPartyException(Specimen specimen) : base(BuildMessage(specimen))
  {
    WorldId = specimen.WorldId.ToGuid();
    PokemonId = specimen.EntityId;
  }

  private static string BuildMessage(Specimen specimen) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.ToGuid())
    .AddData(nameof(PokemonId), specimen.EntityId)
    .Build();
}
