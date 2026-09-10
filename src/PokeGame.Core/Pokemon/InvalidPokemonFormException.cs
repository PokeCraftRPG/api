using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public sealed class InvalidPokemonFormException : DomainException
{
  private const string ErrorMessage = "The specified Pokémon form does not belong to the Pokémon’s variety.";

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
  public Guid VarietyId
  {
    get => (Guid)Data[nameof(VarietyId)]!;
    private set => Data[nameof(VarietyId)] = value;
  }
  public Guid AttemptedVarietyId
  {
    get => (Guid)Data[nameof(AttemptedVarietyId)]!;
    private set => Data[nameof(AttemptedVarietyId)] = value;
  }
  public Guid AttemptedFormId
  {
    get => (Guid)Data[nameof(AttemptedFormId)]!;
    private set => Data[nameof(AttemptedFormId)] = value;
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
      error.Data[nameof(VarietyId)] = VarietyId;
      error.Data[nameof(AttemptedVarietyId)] = AttemptedVarietyId;
      error.Data[nameof(AttemptedFormId)] = AttemptedFormId;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidPokemonFormException(Specimen specimen, Form form)
    : base(BuildMessage(specimen, form))
  {
    WorldId = specimen.WorldId.EntityId;
    PokemonId = specimen.EntityId;
    VarietyId = specimen.VarietyId.EntityId;
    AttemptedVarietyId = form.VarietyId.EntityId;
    AttemptedFormId = form.EntityId;
    PropertyName = nameof(Specimen.FormId);
  }

  private static string BuildMessage(Specimen specimen, Form form) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.EntityId)
    .AddData(nameof(PokemonId), specimen.EntityId)
    .AddData(nameof(VarietyId), specimen.VarietyId.EntityId)
    .AddData(nameof(AttemptedVarietyId), form.VarietyId.EntityId)
    .AddData(nameof(AttemptedFormId), form.EntityId)
    .AddData(nameof(PropertyName), nameof(Specimen.FormId))
    .Build();
}
