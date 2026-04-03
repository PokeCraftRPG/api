using Krakenar.Contracts.Search;

namespace PokeGame.Core.Items.Models;

public record ItemSortOption : SortOption
{
  public new ItemSort Field
  {
    get => Enum.Parse<ItemSort>(base.Field);
    set => base.Field = value.ToString();
  }

  public ItemSortOption(ItemSort field = ItemSort.Key, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }
}
