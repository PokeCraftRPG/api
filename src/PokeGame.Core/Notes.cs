using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core;

public record Notes
{
  public string Value { get; }
  public long Size => Value.Length;

  public Notes(string value)
  {
    Value = value.Trim();
    new Validator().ValidateAndThrow(this);
  }

  public static Notes? TryCreate(string? value) => string.IsNullOrWhiteSpace(value) ? null : new(value);

  public override string ToString() => Value;

  private class Validator : AbstractValidator<Notes>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Notes();
    }
  }
}
