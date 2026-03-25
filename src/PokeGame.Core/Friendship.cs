namespace PokeGame.Core;

public record Friendship
{
  public byte Value { get; }

  public Friendship(byte value = 0)
  {
    Value = value;
  }

  public override string ToString() => Value.ToString();
}
