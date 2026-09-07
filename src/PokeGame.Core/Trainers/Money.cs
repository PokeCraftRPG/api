using FluentValidation;

namespace PokeGame.Core.Trainers;

public sealed class Money
{
  public int Value { get; }

  public Money(int value = 0)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Money? TryCreate(int? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Money money && money.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Money>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Money();
    }
  }
}
