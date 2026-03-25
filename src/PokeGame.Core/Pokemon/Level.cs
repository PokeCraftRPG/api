using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Pokemon;

public record Level
{
  public const byte MinimumValue = 1;
  public const byte MaximumValue = 100;

  public byte Value { get; }

  public Level(byte value)
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
