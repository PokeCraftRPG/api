using FluentValidation;

namespace PokeGame.Core;

public sealed class Level
{
  public const int MinimumValue = 1;
  public const int MaximumValue = 100;

  public int Value { get; }

  public Level(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Level? TryCreate(int? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Level level && level.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Level>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Level();
    }
  }
}
