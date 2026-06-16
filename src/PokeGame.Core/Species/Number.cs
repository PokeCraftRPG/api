using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species;

public class Number
{
  public const int MinimumValue = 1;
  public const int MaximumValue = 9999;

  public int Value { get; }

  public Number(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Number? TryCreate(int? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Number number && number.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Number>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Number();
    }
  }
}
