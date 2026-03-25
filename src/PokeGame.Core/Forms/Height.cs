using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Forms;

public record Height
{
  public int Value { get; }

  public Height(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Height>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Height();
    }
  }
}
