using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Moves;

namespace PokeGame.Core.Varieties;

public sealed class DuplicateVarietyMoveException : ConflictException
{
  private const string ErrorMessage = "The specified move, learning method and level already exist on this variety.";

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
  public Guid VarietyMoveId
  {
    get => (Guid)Data[nameof(VarietyMoveId)]!;
    private set => Data[nameof(VarietyMoveId)] = value;
  }
  public Guid MoveId
  {
    get => (Guid)Data[nameof(MoveId)]!;
    private set => Data[nameof(MoveId)] = value;
  }
  public LearningMethod LearningMethod
  {
    get => (LearningMethod)Data[nameof(LearningMethod)]!;
    private set => Data[nameof(LearningMethod)] = value;
  }
  public byte? Level
  {
    get => (byte?)Data[nameof(Level)];
    private set => Data[nameof(Level)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(VarietyId)] = VarietyId;
      error.Data[nameof(VarietyMoveId)] = VarietyMoveId;
      error.Data[nameof(MoveId)] = MoveId;
      error.Data[nameof(LearningMethod)] = LearningMethod;
      error.Data[nameof(Level)] = Level;
      return error;
    }
  }

  public DuplicateVarietyMoveException(Variety variety, Guid id, VarietyMove move)
    : base(BuildMessage(variety, id, move))
  {
    WorldId = variety.WorldId.EntityId;
    VarietyId = variety.EntityId;
    VarietyMoveId = id;
    MoveId = move.MoveId.EntityId;
    LearningMethod = move.LearningMethod;
    Level = move.Level?.Value;
  }

  private static string BuildMessage(Variety variety, Guid id, VarietyMove move) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), variety.WorldId.EntityId)
    .AddData(nameof(VarietyId), variety.EntityId)
    .AddData(nameof(VarietyMoveId), id)
    .AddData(nameof(MoveId), move.MoveId.EntityId)
    .AddData(nameof(LearningMethod), move.LearningMethod)
    .AddData(nameof(Level), move.Level, "<null>")
    .Build();
}
