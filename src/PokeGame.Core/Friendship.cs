namespace PokeGame.Core;

public class Friendship
{
  public byte Value { get; }

  public Friendship()
  {
  }

  [JsonConstructor]
  public Friendship(byte value)
  {
    Value = value;
  }

  public static Friendship? TryCreate(byte? value) => value.HasValue ? new(value.Value) : null;

  public override bool Equals(object? obj) => obj is Friendship friendship && friendship.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();
}
