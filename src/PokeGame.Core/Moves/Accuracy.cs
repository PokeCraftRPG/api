using FluentValidation;

namespace PokeGame.Core.Moves;

public sealed class Accuracy
{
  public int Value { get; }

  public Accuracy(int value)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public static Accuracy? TryCreate(int? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Accuracy accuracy && accuracy.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Accuracy>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Accuracy();
    }
  }
}
