using FluentValidation;
using PokeGame.Core.Validation;

namespace PokeGame.Core.Items;

public record Price
{
  public int Value { get; }

  public Price(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Price? TryCreate(int? value) => value.HasValue ? new(value.Value) : null;

  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Price>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Price();
    }
  }
}
