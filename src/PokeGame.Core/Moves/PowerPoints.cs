using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Moves;

public record PowerPoints
{
  public const byte MinimumValue = 1;
  public const byte MaximumValue = 40;

  public byte Value { get; }

  public PowerPoints(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<PowerPoints>
  {
    public Validator()
    {
      RuleFor(x => x.Value).PowerPoints();
    }
  }
}
