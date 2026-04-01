using Krakenar.Contracts.Search;

namespace PokeGame.Core.Abilities.Models;

public record AbilitySortOption : SortOption
{
  public new AbilitySort Field
  {
    get => Enum.Parse<AbilitySort>(base.Field);
    set => base.Field = value.ToString();
  }

  public AbilitySortOption(AbilitySort field = AbilitySort.Key, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }
}
