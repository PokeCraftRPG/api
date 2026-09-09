namespace PokeGame.Core;

public sealed class Friendship
{
  public byte Value { get; }

  public Friendship(byte value = 0)
  {
    Value = value;
  }

  public override bool Equals(object? obj) => obj is Friendship friendship && friendship.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();
}
