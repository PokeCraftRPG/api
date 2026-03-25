namespace PokeGame.Core.Species.Models;

public interface IEggGroups
{
  EggGroup Primary { get; }
  EggGroup? Secondary { get; }
}
