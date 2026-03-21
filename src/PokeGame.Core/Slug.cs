using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core;

public record Slug
{
  public const int MaximumLength = 100;

  public string Value { get; }
  public string NormalizedValue => Normalize(Value);
  public long Size => Value.Length;

  public Slug(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static string Normalize(string value) => value.Trim().ToLowerInvariant();

  public override string ToString() => Value;

  private class Validator : AbstractValidator<Slug>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Slug();
    }
  }
}
