using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Storages;

public readonly struct StorageId
{
  private const char Separator = '|';

  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }

  public StorageId(StreamId streamId)
  {
    StreamId = streamId;

    string[] values = streamId.Value.Split(Separator);
    if (values.Length != 2)
    {
      throw new ArgumentException($"The value '{streamId}' is not a valid storage identifier.", nameof(streamId));
    }
    WorldId = new WorldId(values.First());
  }

  public StorageId(WorldId worldId)
  {
    string value = string.Join(Separator, worldId, Storage.EntityKind);
    StreamId = new StreamId(value);

    WorldId = worldId;
  }

  public StorageId(string value) : this(new StreamId(value))
  {
  }

  public static bool operator ==(StorageId left, StorageId right) => left.Equals(right);
  public static bool operator !=(StorageId left, StorageId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is StorageId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
