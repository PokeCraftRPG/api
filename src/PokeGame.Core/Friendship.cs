namespace PokeGame.Core;

public sealed class Friendship
{
  public const byte HighValue = 170; // NOTE(fpion): exactly ⅔ of the maximum value.

  public byte Value { get; }

  public Friendship(byte value = 0)
  {
    Value = value;
  }

  public bool IsHigh() => Value >= HighValue;

  public override bool Equals(object? obj) => obj is Friendship friendship && friendship.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value.ToString();
}
