using Krakenar.Contracts.Search;

namespace PokeGame.Core.Moves.Models;

public record MoveSortOption : SortOption
{
  public new MoveSort Field
  {
    get => Enum.Parse<MoveSort>(base.Field);
    set => base.Field = value.ToString();
  }

  public MoveSortOption(MoveSort field = MoveSort.Key, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }
}
