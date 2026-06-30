namespace PokeGame.Core.Moves.Events;

public class MoveUpdated : UpdateEvent
{
  public Change<string>? Key { get; set; }
  public Change<string>? Name { get; set; }
  public Change<string>? Description { get; set; }

  public Change<byte?>? Accuracy { get; set; }
  public Change<byte?>? Power { get; set; }
  public Change<byte>? PowerPoints { get; set; }

  public MoveUpdated() : base()
  {
  }

  public MoveUpdated(Move move) : base(move)
  {
  }
}
