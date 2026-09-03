using FluentValidation;

namespace PokeGame.Core;

public sealed class Friendship
{
  public int Value { get; }

  public Friendship(int value = 0)
  {
    Value = value;
    new Validator().ValidateAndThrow(this);
  }

  public override bool Equals(object? obj) => obj is Friendship friendship && friendship.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();

  private class Validator : AbstractValidator<Friendship>
  {
    public Validator()
    {
      RuleFor(x => x.Value).Friendship();
    }
  }
}
