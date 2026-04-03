using Krakenar.Contracts.Search;

namespace PokeGame.Core.Evolutions.Models;

public record EvolutionSortOption : SortOption
{
  public new EvolutionSort Field
  {
    get => Enum.Parse<EvolutionSort>(base.Field);
    set => base.Field = value.ToString();
  }

  public EvolutionSortOption(EvolutionSort field = EvolutionSort.CreatedOn, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }
}
