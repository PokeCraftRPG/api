using FluentValidation;

namespace PokeGame.Core.Items;

public sealed class Price
{
  public int Value { get; }

  public Price(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Price? TryCreate(int? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Price price && price.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Price>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Price();
    }
  }
}
