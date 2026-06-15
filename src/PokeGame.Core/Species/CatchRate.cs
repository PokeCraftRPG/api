using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species;

public class CatchRate
{
  public byte Value { get; }

  public CatchRate(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static CatchRate? TryCreate(byte? value) => value.HasValue ? new(value.Value) : null;

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
