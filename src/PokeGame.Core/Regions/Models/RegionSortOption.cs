using Krakenar.Contracts.Search;

namespace PokeGame.Core.Regions.Models;

public record RegionSortOption : SortOption
{
  public new RegionSort Field
  {
    get => Enum.Parse<RegionSort>(base.Field);
    set => base.Field = value.ToString();
  }

  public RegionSortOption(RegionSort field = RegionSort.Key, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }
}
