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
    EntityId = entity.Id;
  }

  public WorldId(string value) : this(new StreamId(value))
  {
  }

  public WorldId(Guid entityId)
  {
    Entity entity = new(World.EntityKind, entityId);
    StreamId = new StreamId(entity.ToString());

    EntityId = entityId;
  }

  public static WorldId NewId() => new(Guid.NewGuid());

  public static bool operator ==(WorldId left, WorldId right) => left.Equals(right);
  public static bool operator !=(WorldId left, WorldId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is WorldId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
