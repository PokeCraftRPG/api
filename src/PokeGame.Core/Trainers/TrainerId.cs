using Logitar.EventSourcing;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

public readonly struct TrainerId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public WorldId WorldId { get; }
  public Guid EntityId { get; }

  public TrainerId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, Trainer.EntityKind);
    WorldId = entity.WorldId ?? throw new ArgumentException("The world identifier is required.", nameof(streamId));
    EntityId = entity.Id;
  }

  public TrainerId(WorldId worldId, Guid entityId)
  {
    Entity entity = new(Trainer.EntityKind, entityId, worldId);
    StreamId = new StreamId(entity.ToString());

    WorldId = worldId;
    EntityId = entityId;
  }

  public TrainerId(string value) : this(new StreamId(value))
  {
  }

  public static TrainerId NewId(WorldId worldId) => new(worldId, Guid.NewGuid());

  public Entity GetEntity() => new(Trainer.EntityKind, EntityId, WorldId);

  public static bool operator ==(TrainerId left, TrainerId right) => left.Equals(right);
  public static bool operator !=(TrainerId left, TrainerId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is TrainerId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
