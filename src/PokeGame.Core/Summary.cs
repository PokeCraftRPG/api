using FluentValidation;

namespace PokeGame.Core;

public sealed class Summary
{
  public const int MaximumLength = 100;

  public string Value { get; }

  public Summary(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Summary? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override bool Equals(object? obj) => obj is Summary summary && summary.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<Summary>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Summary();
    }
  }
}
