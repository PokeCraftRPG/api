namespace PokeGame.Core.Moves.Events;

public class MoveCreated : CreateEvent
{
  public PokemonType Type { get; set; }
  public MoveCategory Category { get; set; }

  public string Key { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string? Description { get; set; }

  public byte? Accuracy { get; set; }
  public byte? Power { get; set; }
  public byte PowerPoints { get; set; }

  public MoveCreated() : base()
  {
  }

  public MoveCreated(Move move) : base(move)
  {
    Type = move.Type;
    Category = move.Category;

    Key = move.Key;
    Name = move.Name;
    Description = move.Description;

    Accuracy = move.Accuracy;
    Power = move.Power;
    PowerPoints = move.PowerPoints;
  }
}
