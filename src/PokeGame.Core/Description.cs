using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core;

public class Description
{
  public string Value { get; }

  public Description(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Description? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override bool Equals(object? obj) => obj is Description description && description.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;

  private class Validator : AbstractValidator<Description>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Description();
    }
  }
}
