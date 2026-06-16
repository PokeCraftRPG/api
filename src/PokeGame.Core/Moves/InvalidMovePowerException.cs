using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Moves;

public class InvalidMovePowerException : DomainException
{
  private const string ErrorMessage = "A Status move cannot have Power.";

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
  public byte AttemptedPower
  {
    get => (byte)Data[nameof(AttemptedPower)]!;
    private set => Data[nameof(AttemptedPower)] = value;
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
      error.Data[nameof(AttemptedPower)] = AttemptedPower;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InvalidMovePowerException(Move move, Power attemptedPower, string propertyName)
    : base(BuildMessage(move, attemptedPower, propertyName))
  {
    WorldId = move.WorldId.EntityId;
    MoveId = move.EntityId;
    AttemptedPower = attemptedPower.Value;
    PropertyName = propertyName;
  }

  private static string BuildMessage(Move move, Power attemptedPower, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), move.WorldId.EntityId)
    .AddData(nameof(MoveId), move.EntityId)
    .AddData(nameof(AttemptedPower), attemptedPower)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
