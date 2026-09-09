using FluentValidation;

namespace PokeGame.Core;

public sealed class Level
{
  public byte Value { get; }

  public Level(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Level? TryCreate(byte? value) => value.HasValue ? new(value.Value) : null;

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
