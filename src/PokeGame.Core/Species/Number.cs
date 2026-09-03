using FluentValidation;

namespace PokeGame.Core.Species;

public sealed class Number
{
  public int Value { get; }

  public Number(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override bool Equals(object? obj) => obj is Number number && number.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Number>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Number();
    }
  }
}
