using Krakenar.Contracts.Search;

namespace PokeGame.Core.Worlds.Models;

public record WorldSortOption : SortOption
{
  public new WorldSort Field
  {
    get => Enum.Parse<WorldSort>(base.Field);
    set => base.Field = value.ToString();
  }

  public WorldSortOption(WorldSort field = WorldSort.Name, bool isDescending = false)
    : base(field.ToString(), isDescending)
  {
  }
}
