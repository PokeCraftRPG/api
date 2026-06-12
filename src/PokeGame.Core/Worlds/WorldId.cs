using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds;

public readonly struct WorldId
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public Guid EntityId { get; }

  public WorldId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, World.EntityKind);
    if (entity.WorldId.HasValue)
    {
      throw new ArgumentException("The value should not contain a world identifier.", nameof(streamId));
    }
    EntityId = entity.Id;
  }

  public WorldId(string value) : this(new StreamId(value))
  {
  }

  public WorldId(Guid entityId)
  {
    StreamId = new StreamId(new Entity(World.EntityKind, entityId).ToString());

    EntityId = entityId;
  }

  public static WorldId NewId() => new(Guid.NewGuid());

  public static bool operator ==(WorldId left, WorldId right) => left.Equals(right);
  public static bool operator !=(WorldId left, WorldId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is WorldId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
