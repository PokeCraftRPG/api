using FluentValidation;

namespace PokeGame.Core.Pokemon;

public sealed class Level
{
  public int Value { get; }

  public Level(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

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
