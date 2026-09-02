using FluentValidation;

namespace PokeGame.Core.Search;

public record SearchPayload<T> where T : struct, Enum
{
  public List<Guid> Ids { get; set; } = [];
  public TextSearch Search { get; set; } = new();

  public List<SortOption<T>> Sort { get; set; } = [];

  public int Offset { get; set; }
  public int Limit { get; set; }

  public virtual void Validate() => new SearchValidator<T>().ValidateAndThrow(this);
}

internal class SearchValidator<T> : AbstractValidator<SearchPayload<T>> where T : struct, Enum
{
  private const int MaximumIdCount = 100;
  private const int MaximumSortCount = 3;
  private const int MaximumLimit = 100;

  public SearchValidator()
  {
    RuleFor(x => x.Ids).Must(ids => ids.Count <= MaximumIdCount)
      .WithErrorCode("IdsValidator")
      .WithMessage($"'{{PropertyName}}' may only include up to {MaximumIdCount} identifiers.");
    RuleFor(x => x.Search).SetValidator(new TextSearchValidator());

    RuleFor(x => x.Sort).Must(options => options.Count <= MaximumSortCount)
      .WithErrorCode("MaximumSortValidator")
      .WithMessage($"'{{PropertyName}}' may only include up to {MaximumSortCount} options.");
    RuleFor(x => x.Sort).Must(HaveUniqueFields)
      .WithErrorCode("UniqueSortValidator")
      .WithMessage($"'{{PropertyName}}' may not contain duplicate fields.");
    RuleForEach(x => x.Sort).SetValidator(new SortOptionValidator<T>());

    RuleFor(x => x.Offset).GreaterThanOrEqualTo(0);
    RuleFor(x => x.Limit).InclusiveBetween(0, MaximumLimit);
  }

  private static bool HaveUniqueFields(IReadOnlyCollection<SortOption<T>> options)
  {
    HashSet<T> fields = new(options.Count);
    foreach (SortOption<T> option in options)
    {
      if (!fields.Add(option.Field))
      {
        return false;
      }
    }
    return true;
  }
}
