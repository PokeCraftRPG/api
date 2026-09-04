using FluentValidation;

namespace PokeGame.Core.Varieties;

public sealed class Genus
{
  public const int MaximumLength = 16;

  public string Value { get; }

  public Genus(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Genus? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override bool Equals(object? obj) => obj is Genus genus && genus.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<Genus>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Genus();
    }
  }
}
