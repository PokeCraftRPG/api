using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species;

public record Number
{
  public const int MinimumValue = 1;
  public const int MaximumValue = 9999;

  public int Value { get; }

  public Number(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Number>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Number();
    }
  }
}
