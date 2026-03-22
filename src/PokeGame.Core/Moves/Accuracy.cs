using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Moves;

public record Accuracy
{
  public const byte MinimumValue = 1;
  public const byte MaximumValue = 100;

  public byte Value { get; }

  public Accuracy(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Accuracy>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Accuracy();
    }
  }
}
