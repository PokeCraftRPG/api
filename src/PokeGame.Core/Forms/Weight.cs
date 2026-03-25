using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Forms;

public record Weight
{
  public int Value { get; }

  public Weight(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Weight>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Weight();
    }
  }
}
