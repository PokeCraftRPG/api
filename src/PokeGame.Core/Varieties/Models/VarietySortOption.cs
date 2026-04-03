using Krakenar.Contracts.Search;

namespace PokeGame.Core.Varieties.Models;

public record VarietySortOption : SortOption
{
  public new VarietySort Field
  {
    get => Enum.Parse<VarietySort>(base.Field);
    set => base.Field = value.ToString();
  }

  public VarietySortOption(VarietySort field = VarietySort.Key, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }
}
