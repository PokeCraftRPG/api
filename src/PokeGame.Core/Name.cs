using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core;

public record Name
{
  public const int MaximumLength = 100;

  public string Value { get; }
  public long Size => Value.Length;

  public Name(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Name? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override string ToString() => Value;

  private class Validator : AbstractValidator<Name>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Name();
    }
  }
}
