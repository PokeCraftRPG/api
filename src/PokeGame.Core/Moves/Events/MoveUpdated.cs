namespace PokeGame.Core.Moves.Events;

public class MoveUpdated : UpdateEvent
{
  public Change<string>? Key { get; set; }
  public Change<string>? Name { get; set; }
  public Change<string>? Description { get; set; }

  public Change<int?>? Accuracy { get; set; }
  public Change<int?>? Power { get; set; }
  public Change<int>? PowerPoints { get; set; }

  public MoveUpdated() : base()
  {
  }

  public MoveUpdated(Move move) : base(move)
  {
  }
}
