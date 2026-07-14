using FluentValidation;
using FluentValidation.Validators;
using Logitar;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Validation;

internal class GenderValidator<T> : IPropertyValidator<T, string>
{
  private readonly HashSet<string> _knownValues = ["female", "male", "other"];

  public string Name { get; } = "GenderValidator";

  public GenderValidator(IEnumerable<string>? knownValues = null)
  {
    if (knownValues is not null)
    {
      _knownValues.Clear();
      _knownValues.AddRange(knownValues.Select(UserHelper.NormalizeGender));
    }
  }

  public string GetDefaultMessageTemplate(string errorCode)
  {
    return $"'{{PropertyName}}' must be one of the following: {string.Join(", ", _knownValues)}.";
  }

  public bool IsValid(ValidationContext<T> context, string value) => _knownValues.Contains(UserHelper.NormalizeGender(value));
}
