using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Trainers;

public record Money
{
  public int Value { get; }

  public Money(int value = 0)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Money>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Money();
    }
  }
}
