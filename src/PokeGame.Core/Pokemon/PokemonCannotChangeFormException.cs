using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Pokemon;

public class PokemonCannotChangeFormException : DomainException
{
  private const string ErrorMessage = "This Pokémon variety cannot change form.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid VarietyId
  {
    get => (Guid)Data[nameof(VarietyId)]!;
    private set => Data[nameof(VarietyId)] = value;
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
      error.Data[nameof(VarietyId)] = VarietyId;
      error.Data[nameof(PokemonId)] = PokemonId;
      return error;
    }
  }

  public PokemonCannotChangeFormException(Specimen specimen)
    : base(BuildMessage(specimen))
  {
    WorldId = specimen.WorldId.ToGuid();
    VarietyId = specimen.VarietyId.EntityId;
    PokemonId = specimen.EntityId;
  }

  private static string BuildMessage(Specimen specimen) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.ToGuid())
    .AddData(nameof(VarietyId), specimen.VarietyId.EntityId)
    .AddData(nameof(PokemonId), specimen.EntityId)
    .Build();
}
