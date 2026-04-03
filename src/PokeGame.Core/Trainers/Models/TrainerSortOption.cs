using Krakenar.Contracts.Search;

namespace PokeGame.Core.Trainers.Models;

public record TrainerSortOption : SortOption
{
  public new TrainerSort Field
  {
    get => Enum.Parse<TrainerSort>(base.Field);
    set => base.Field = value.ToString();
  }

  public TrainerSortOption(TrainerSort field = TrainerSort.Key, bool isDescending = false)
  {
    Field = field;
    IsDescending = isDescending;
  }
}
