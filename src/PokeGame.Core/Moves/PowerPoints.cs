using FluentValidation;

namespace PokeGame.Core.Moves;

public sealed class PowerPoints
{
  public int Value { get; }

  public PowerPoints(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static PowerPoints? TryCreate(int? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is PowerPoints powerpoints && powerpoints.Value == Value;
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
