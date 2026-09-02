using FluentValidation;

namespace PokeGame.Core.Search;

public record SortOption<T> where T : struct, Enum
{
  public T Field { get; set; }
  public SortDirection Direction { get; set; }

  public SortOption()
  {
  }

  public SortOption(T field, SortDirection direction = SortDirection.Ascending)
  {
    Field = field;
    Direction = direction;
  }
}

internal class SortOptionValidator<T> : AbstractValidator<SortOption<T>> where T : struct, Enum
{
  public SortOptionValidator()
  {
    RuleFor(x => x.Field).IsInEnum();
    RuleFor(x => x.Direction).IsInEnum();
  }
}
