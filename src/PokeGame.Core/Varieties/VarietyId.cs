using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties;

public readonly struct VarietyId
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public VarietyId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Variety.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public VarietyId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Variety.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public VarietyId(string value) : this(new StreamId(value))
  {
  }

  public static VarietyId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public static bool operator ==(VarietyId left, VarietyId right) => left.Equals(right);
  public static bool operator !=(VarietyId left, VarietyId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is VarietyId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
