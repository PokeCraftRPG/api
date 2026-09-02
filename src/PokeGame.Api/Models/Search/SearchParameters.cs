using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using PokeGame.Core.Search;

namespace PokeGame.Api.Models.Search;

public record SearchParameters
{
  public const int DefaultLimit = 10;

  [FromQuery(Name = "ids")]
  public List<Guid> Ids { get; set; } = [];

  [FromQuery(Name = "search")]
  public List<string> SearchTerms { get; set; } = [];

  [FromQuery(Name = "search_mode")]
  public SearchMode SearchMode { get; set; }

  [FromQuery(Name = "sort")]
  public List<string> Sort { get; set; } = [];

  [FromQuery(Name = "offset")]
  public int Offset { get; set; }

  [FromQuery(Name = "limit")]
  public int? Limit { get; set; }

  public void Fill<T>(SearchPayload<T> payload) where T : struct, Enum
  {
    payload.Ids.Clear();
    payload.Ids.AddRange(Ids);

    payload.Search.Terms.Clear();
    foreach (string term in SearchTerms)
    {
      payload.Search.Terms.Add(term.Trim());
    }
    payload.Search.Mode = SearchMode;

    payload.Sort.Clear();
    List<ValidationFailure> failures = new(capacity: Sort.Count);
    for (int i = 0; i < Sort.Count; i++)
    {
      string attemptedValue = Sort[i];
      string sort = attemptedValue.Trim();
      SortDirection direction = sort.StartsWith('-') ? SortDirection.Descending : SortDirection.Ascending;
      if (sort.StartsWith('-') || sort.StartsWith('+'))
      {
        sort = sort[1..];
      }

      if (Enum.TryParse(sort, ignoreCase: true, out T field) && Enum.IsDefined(field))
      {
        payload.Sort.Add(new SortOption<T>(field, direction));
      }
      else
      {
        string propertyName = $"sort[{i}]";
        failures.Add(new ValidationFailure(propertyName, $"'{propertyName}' is not a valid sort field.", attemptedValue)
        {
          CustomState = new { direction },
          ErrorCode = "SortValidator"
        });
      }
    }
    if (failures.Count > 0)
    {
      throw new ValidationException(failures);
    }

    payload.Offset = Offset;
    payload.Limit = Limit ?? DefaultLimit;
  }
}
