using Krakenar.Contracts.Search;

namespace PokeGame.Core.Species.Models;

public record SpeciesSortOption : SortOption
{
  public new SpeciesSort Field
  {
    get => Enum.Parse<SpeciesSort>(base.Field);
    set => base.Field = value.ToString();
  }

  public SpeciesSortOption(SpeciesSort field = SpeciesSort.Key, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }
}
