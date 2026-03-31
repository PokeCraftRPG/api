using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Items;

public readonly struct ItemId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public ItemId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Item.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public ItemId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Item.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public ItemId(string value) : this(new StreamId(value))
  {
  }

  public static ItemId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(Item.EntityKind, EntityId, WorldId);

  public static bool operator ==(ItemId left, ItemId right) => left.Equals(right);
  public static bool operator !=(ItemId left, ItemId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is ItemId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
