using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Forms;

namespace PokeGame.Core.Pokemon;

public class InvalidFormException : DomainException
{
  private const string ErrorMessage = "The source and target forms must belong to the same variety.";

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
  public Guid SourceId
  {
    get => (Guid)Data[nameof(SourceId)]!;
    private set => Data[nameof(SourceId)] = value;
  }
  public Guid TargetId
  {
    get => (Guid)Data[nameof(TargetId)]!;
    private set => Data[nameof(TargetId)] = value;
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
      error.Data[nameof(SourceId)] = SourceId;
      error.Data[nameof(TargetId)] = TargetId;
      error.Data[nameof(PokemonId)] = PokemonId;
      return error;
    }
  }

  public InvalidFormException(Specimen specimen, Form target)
    : base(BuildMessage(specimen, target))
  {
    WorldId = specimen.WorldId.ToGuid();
    VarietyId = specimen.VarietyId.EntityId;
    SourceId = specimen.FormId.EntityId;
    TargetId = target.EntityId;
    PokemonId = specimen.EntityId;
  }

  private static string BuildMessage(Specimen specimen, Form target) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), specimen.WorldId.ToGuid())
    .AddData(nameof(VarietyId), specimen.VarietyId.EntityId)
    .AddData(nameof(SourceId), specimen.FormId.EntityId)
    .AddData(nameof(TargetId), target.EntityId)
    .AddData(nameof(PokemonId), specimen.EntityId)
    .Build();
}
