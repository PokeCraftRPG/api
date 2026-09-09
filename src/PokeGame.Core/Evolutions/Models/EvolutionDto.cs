using Krakenar.Contracts;
using PokeGame.Core.Forms.Models;
using PokeGame.Core.Items.Models;
using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Evolutions.Models;

public class EvolutionDto : Aggregate
{
  public FormDto Source { get; set; } = new();
  public FormDto Target { get; set; } = new();
  public EvolutionTrigger Trigger { get; set; }

  public int? Level { get; set; }
  public bool Friendship { get; set; }
  public Gender? Gender { get; set; }
  public ItemDto? Item { get; set; }
  public MoveDto? Move { get; set; }
  public string? Location { get; set; }
  public TimeOfDay? TimeOfDay { get; set; }
}
