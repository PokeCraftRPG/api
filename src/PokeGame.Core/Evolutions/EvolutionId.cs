using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions;

public readonly struct EvolutionId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public EvolutionId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Evolution.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public EvolutionId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Evolution.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public EvolutionId(string value) : this(new StreamId(value))
  {
  }

  public static EvolutionId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(Evolution.EntityKind, EntityId, WorldId);

  public static bool operator ==(EvolutionId left, EvolutionId right) => left.Equals(right);
  public static bool operator !=(EvolutionId left, EvolutionId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is EvolutionId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
