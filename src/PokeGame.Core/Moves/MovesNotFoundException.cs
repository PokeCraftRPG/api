using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Moves;

public class MovesNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified moves were not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public IReadOnlyCollection<string> Moves
  {
    get => (IReadOnlyCollection<string>)Data[nameof(Moves)]!;
    private set => Data[nameof(Moves)] = value;
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
      error.Data[nameof(Moves)] = Moves;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public MovesNotFoundException(WorldId worldId, IEnumerable<string> moves, string propertyName)
    : base(BuildMessage(worldId, moves, propertyName))
  {
    WorldId = worldId.ToGuid();
    Moves = moves.Distinct().ToList().AsReadOnly();
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, IEnumerable<string> moves, string propertyName)
  {
    StringBuilder message = new();
    message.AppendLine(ErrorMessage);
    message.Append(nameof(WorldId)).Append(": ").Append(worldId.ToGuid()).AppendLine();
    message.Append(nameof(PropertyName)).Append(": ").AppendLine(propertyName);
    if (moves.Any())
    {
      message.Append(nameof(Moves)).Append(':').AppendLine();
      moves = moves.Distinct();
      foreach (string move in moves)
      {
        message.Append(" - ").AppendLine(move);
      }
    }
    return message.ToString();
  }
}
