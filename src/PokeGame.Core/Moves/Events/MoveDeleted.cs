namespace PokeGame.Core.Moves.Events;

public class MoveDeleted : DeleteEvent
{
  public MoveDeleted() : base()
  {
  }

  public MoveDeleted(Move move, Guid userId) : base(move, userId)
  {
  }
}
