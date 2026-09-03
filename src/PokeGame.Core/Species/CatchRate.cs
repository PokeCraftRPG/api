using FluentValidation;

namespace PokeGame.Core.Species;

public sealed class CatchRate
{
  public const int MaximumValue = byte.MaxValue;

  public int Value { get; }

  public CatchRate(int value = MaximumValue)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override bool Equals(object? obj) => obj is CatchRate catchRate && catchRate.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<CatchRate>
  {
    public Validator()
    {
      RuleFor(x => x.Value).CatchRate();
    }
  }
}
