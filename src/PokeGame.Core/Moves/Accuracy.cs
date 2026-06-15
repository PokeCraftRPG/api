using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Moves;

public class Accuracy
{
  public const byte MinimumValue = 1;
  public const byte MaximumValue = 100;

  public byte Value { get; }

  public Accuracy(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Accuracy? TryCreate(byte? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Accuracy accuracy && accuracy.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Accuracy>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Accuracy();
    }
  }
}
