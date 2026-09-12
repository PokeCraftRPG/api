namespace PokeGame.Core.Varieties;

public sealed class DuplicateVarietyMoveException : ConflictException
{
  public DuplicateVarietyMoveException(Variety variety, Guid id, VarietyMove move)
    : base("The specified move, learning method and level already exist on this variety.")
  {
    Data["WorldId"] = variety.WorldId.EntityId;
    Data["VarietyId"] = variety.EntityId;
    Data["VarietyMoveId"] = id;
    Data["MoveId"] = move.MoveId.EntityId;
    Data["LearningMethod"] = move.LearningMethod;
    Data["Level"] = move.Level?.Value;
  }
}
