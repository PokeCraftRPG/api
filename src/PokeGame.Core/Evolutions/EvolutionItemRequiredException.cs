using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Evolutions;

public sealed class EvolutionItemRequiredException : DomainException
{
  private const string ErrorMessage = "An item is required when the evolution is triggered by an item.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid EvolutionId
  {
    get => (Guid)Data[nameof(EvolutionId)]!;
    private set => Data[nameof(EvolutionId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(EvolutionId)] = EvolutionId;
      return error;
    }
  }

  public EvolutionItemRequiredException(Evolution evolution)
    : base(BuildMessage(evolution))
  {
    WorldId = evolution.WorldId.EntityId;
    EvolutionId = evolution.EntityId;
  }

  private static string BuildMessage(Evolution evolution) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), evolution.WorldId.EntityId)
    .AddData(nameof(EvolutionId), evolution.EntityId)
    .Build();
}
