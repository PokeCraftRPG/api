using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Pokemon;

public record Level
{
  public const int MinimumValue = 1;
  public const int MaximumValue = 100;

  public int Value { get; }

  public Level(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Level>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Level();
    }
  }
}
