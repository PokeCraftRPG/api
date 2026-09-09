using FluentValidation;

namespace PokeGame.Core.Items;

public sealed class Weight
{
  public int Value { get; }

  public Weight(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Weight? TryCreate(int? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Weight weight && weight.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Weight>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Weight();
    }
  }
}
