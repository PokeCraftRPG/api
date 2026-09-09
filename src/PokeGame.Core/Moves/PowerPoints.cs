using FluentValidation;

namespace PokeGame.Core.Moves;

public sealed class PowerPoints
{
  public byte Value { get; }

  public PowerPoints(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static PowerPoints? TryCreate(byte? value) => value.HasValue ? new(value.Value) : null;

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
