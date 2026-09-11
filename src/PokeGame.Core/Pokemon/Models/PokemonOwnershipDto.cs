using PokeGame.Core.Items.Models;
using PokeGame.Core.Trainers.Models;

namespace PokeGame.Core.Pokemon.Models;

public record PokemonOwnershipDto
{
  public OwnershipEvent Event { get; set; }
  public TrainerDto Trainer { get; set; } = new();
  public ItemDto PokeBall { get; set; } = new();
  public int MetLevel { get; set; }
  public string MetAt { get; set; } = string.Empty;
  public DateTime MetOn { get; set; }
}
