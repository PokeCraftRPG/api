using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Assets;

public readonly struct AssetId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public AssetId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Asset.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("A world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public AssetId(string value) : this(new StreamId(value))
  {
  }

  public AssetId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Asset.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public static AssetId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(Asset.EntityKind, EntityId, WorldId);

  public static bool operator ==(AssetId left, AssetId right) => left.Equals(right);
  public static bool operator !=(AssetId left, AssetId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is AssetId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
