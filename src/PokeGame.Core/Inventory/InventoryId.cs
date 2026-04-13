using Logitar.EventSourcing;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory;

public readonly struct InventoryId : IEntityProvider
{
  private const char Separator = '|';

  public StreamId StreamId { get; }
  public string Value => StreamId.Value;

  public TrainerId TrainerId { get; }

  public InventoryId(StreamId streamId)
  {
    StreamId = streamId;

    string[] values = streamId.Value.Split(Separator);
    if (values.Length != 3)
    {
      throw new ArgumentException($"The value '{streamId}' is not a valid inventory identifier.", nameof(streamId));
    }
    TrainerId = new TrainerId(string.Join(Separator, values.Take(2)));
  }

  public InventoryId(TrainerId trainerId)
  {
    string value = string.Join(Separator, trainerId, InventoryAggregate.EntityKind);
    StreamId = new StreamId(value);

    TrainerId = trainerId;
  }

  public InventoryId(string value) : this(new StreamId(value))
  {
  }

  public Entity GetEntity() => new(InventoryAggregate.EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public static bool operator ==(InventoryId left, InventoryId right) => left.Equals(right);
  public static bool operator !=(InventoryId left, InventoryId right) => !left.Equals(right);

  public override bool Equals([NotNullWhen(true)] object? obj) => obj is InventoryId id && id.Value == Value;
  public override int GetHashCode() => Value.GetHashCode();
  public override string ToString() => Value;
}
