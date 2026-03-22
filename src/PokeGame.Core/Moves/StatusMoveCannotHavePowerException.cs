using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Moves;

public class StatusMoveCannotHavePowerException : DomainException
{
  private const string ErrorMessage = "Status moves cannot have power.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid MoveId
  {
    get => (Guid)Data[nameof(MoveId)]!;
    private set => Data[nameof(MoveId)] = value;
  }
  public Power Power
  {
    get => (Power)Data[nameof(Power)]!;
    private set => Data[nameof(Power)] = value;
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
      error.Data[nameof(MoveId)] = MoveId;
      error.Data[nameof(Power)] = Power.Value;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public StatusMoveCannotHavePowerException(Move move, Power power)
    : base(BuildMessage(move, power))
  {
    WorldId = move.WorldId.ToGuid();
    MoveId = move.EntityId;
    Power = power;
    PropertyName = nameof(Move.Power);
  }

  private static string BuildMessage(Move move, Power power) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), move.WorldId.ToGuid())
    .AddData(nameof(MoveId), move.EntityId)
    .AddData(nameof(Power), power)
    .AddData(nameof(PropertyName), nameof(Move.Power))
    .Build();
}
