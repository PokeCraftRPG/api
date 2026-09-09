using FluentValidation;

namespace PokeGame.Core.Moves;

public sealed class Power
{
  public byte Value { get; }

  public Power(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Power? TryCreate(byte? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Power power && power.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Power>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Power();
    }
  }
}
