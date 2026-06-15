using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Moves;

public class PowerPoints
{
  public const byte MinimumValue = 1;
  public const byte MaximumValue = 40;

  public byte Value { get; }

  public PowerPoints(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Power? TryCreate(byte? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is PowerPoints powerPoints && powerPoints.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<PowerPoints>
  {
    public Validator()
    {
      RuleFor(x => x.Value).PowerPoints();
    }
  }
}
