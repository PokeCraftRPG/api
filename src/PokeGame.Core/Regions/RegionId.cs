using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions;

public readonly struct RegionId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public RegionId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Region.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public RegionId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Region.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public RegionId(string value) : this(new StreamId(value))
  {
  }

  public static RegionId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(Region.EntityKind, EntityId, WorldId);

  public static bool operator ==(RegionId left, RegionId right) => left.Equals(right);
  public static bool operator !=(RegionId left, RegionId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is RegionId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
