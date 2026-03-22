using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core;

public record Slug
{
  public const int MaximumLength = 100;

  public string Value { get; }
  public long Size => Value.Length;

  public Slug(string value)
  {
    Value = Normalize(value);
    new Validator().ValidateAndThrow(this);
  }

  public static string Normalize(string value) => value.Trim().ToLowerInvariant();
  public static Slug? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override string ToString() => Value;

  private class Validator : AbstractValidator<Slug>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Slug();
    }
  }
}
