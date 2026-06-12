using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core;

public class Slug
{
  public const int MaximumLength = 100;

  public string Value { get; }

  public Slug(string value)
  {
    Value = Normalize(value);
    new Validator().ValidateAndThrow(this);
  }

  public static bool IsValid(string value) => value.Split('-').All(word => !string.IsNullOrEmpty(word) && word.All(char.IsLetterOrDigit));
  public static string Normalize(string value) => value.Trim().ToLowerInvariant();
  public static Slug? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override bool Equals(object? obj) => obj is Slug slug && slug.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<Slug>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Slug();
    }
  }
}
