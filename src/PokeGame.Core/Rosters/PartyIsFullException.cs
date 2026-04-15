using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Rosters;

public class PartyIsFullException : DomainException
{
  private const string ErrorMessage = "The trainer party is full.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid TrainerId
  {
    get => (Guid)Data[nameof(TrainerId)]!;
    private set => Data[nameof(TrainerId)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(TrainerId)] = TrainerId;
      return error;
    }
  }

  public PartyIsFullException(Roster roster) : base(BuildMessage(roster))
  {
    WorldId = roster.TrainerId.WorldId.ToGuid();
    TrainerId = roster.TrainerId.EntityId;
  }

  private static string BuildMessage(Roster roster) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), roster.TrainerId.WorldId.ToGuid())
    .AddData(nameof(TrainerId), roster.TrainerId.EntityId)
    .Build();
}
