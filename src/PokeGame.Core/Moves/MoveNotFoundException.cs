using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public class MoveNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified move was not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public string Move
  {
    get => (string)Data[nameof(Move)]!;
    private set => Data[nameof(Move)] = value;
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
      error.Data[nameof(Move)] = Move;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public MoveNotFoundException(WorldId worldId, string move, string propertyName)
    : base(BuildMessage(worldId, move, propertyName))
  {
    WorldId = worldId.ToGuid();
    Move = move;
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, string move, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), worldId.ToGuid())
    .AddData(nameof(Move), move)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
