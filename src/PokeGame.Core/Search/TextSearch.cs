using FluentValidation;

namespace PokeGame.Core.Search;

public record TextSearch
{
  public List<string> Terms { get; set; } = [];
  public SearchMode Mode { get; set; }

  public TextSearch()
  {
  }

  public TextSearch(IEnumerable<string> terms, SearchMode mode = SearchMode.All)
  {
    Terms.AddRange(terms);
    Mode = mode;
  }
}

internal class TextSearchValidator : AbstractValidator<TextSearch>
{
  private const int MaximumCount = 10;
  private const int MinimumLength = 2;
  private const int MaximumLength = 100;

  public TextSearchValidator()
  {
    RuleFor(x => x.Terms).Must(terms => terms.Count <= MaximumCount)
      .WithErrorCode("TermsValidator")
      .WithMessage($"'{{PropertyName}}' may only include up to {MaximumCount} terms.");
    RuleForEach(x => x.Terms).Cascade(CascadeMode.Stop)
      .NotEmpty()
      .Must(HaveValidEscape)
        .WithErrorCode("TermSyntaxValidator")
        .WithMessage("'{PropertyName}' contains an incomplete escape sequence.")
      .Must(HaveMinimumLength)
        .WithErrorCode("MinimumTermLengthValidator")
        .WithMessage($"'{{PropertyName}}' must contain at least {MinimumLength} searchable characters.")
      .MaximumLength(MaximumLength);

    RuleFor(x => x.Mode).IsInEnum();
  }

  private static bool HaveValidEscape(string term)
  {
    int count = 0;
    for (int i = term.Length - 1; i >= 0 && term[i] == '\\'; i--)
    {
      count++;
    }
    return count % 2 == 0;
  }

  private static bool HaveMinimumLength(string term)
  {
    int length = 0;
    int last = term.Length - 1;
    bool escaped = false;

    for (int i = 0; i < term.Length; i++)
    {
      char c = term[i];

      if (escaped)
      {
        length++;
        escaped = false;
        continue;
      }

      switch (c)
      {
        case '\\':
          if (i == last)
          {
            throw new ArgumentException("The term cannot end with '\\'.", nameof(term));
          }
          escaped = true;
          break;
        case '*':
        case '?':
          break;
        case '^':
          if (i > 0)
          {
            length++;
          }
          break;
        case '$':
          if (i < last)
          {
            length++;
          }
          break;
        default:
          length++;
          break;
      }
    }

    return length >= MinimumLength;
  }
}
