namespace PokeGame.Core.Moves;

public sealed class InvalidMovePowerException : DomainException
{
  public InvalidMovePowerException(Move move, Power attemptedPower)
    : base("A status move cannot have power.")
  {
    Data["WorldId"] = move.WorldId.EntityId;
    Data["MoveId"] = move.EntityId;
    Data["AttemptedPower"] = attemptedPower.Value;
    Data["PropertyName"] = nameof(Move.Power);
  }
}
