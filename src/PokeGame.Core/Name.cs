using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core;

public class Name
{
  public const int MaximumLength = 100;

  public string Value { get; }

  public Name(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Name? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override bool Equals(object? obj) => obj is Name name && name.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<Name>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Name();
    }
  }
}
