using Logitar.EventSourcing;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory;

public readonly struct InventoryId : IEntityProvider
{
  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public TrainerId TrainerId { get; }

  public InventoryId(StreamId streamId)
  {
    StreamId = streamId;

    Entity entity = Entity.Parse(streamId.Value, TrainerInventory.EntityKind);
    if (!entity.WorldId.HasValue)
    {
      throw new ArgumentException("A world identifier is required.", nameof(streamId));
    }
    TrainerId = new TrainerId(entity.WorldId.Value, entity.Id);
  }

  public InventoryId(string value) : this(new StreamId(value))
  {
  }

  public InventoryId(TrainerId trainerId)
  {
    Entity entity = new(TrainerInventory.EntityKind, trainerId.EntityId, trainerId.WorldId);
    StreamId = new StreamId(entity.ToString());

    TrainerId = trainerId;
  }

  public Entity GetEntity() => new(TrainerInventory.EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public static bool operator ==(InventoryId left, InventoryId right) => left.Equals(right);
  public static bool operator !=(InventoryId left, InventoryId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is InventoryId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
