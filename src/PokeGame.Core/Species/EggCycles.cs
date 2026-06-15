using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Species;

public class EggCycles
{
  public byte Value { get; }

  public EggCycles(byte value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static EggCycles? TryCreate(byte? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is EggCycles eggCycles && eggCycles.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<EggCycles>
  {
    public Validator()
    {
      RuleFor(x => x.Value).EggCycles();
    }
  }
}
