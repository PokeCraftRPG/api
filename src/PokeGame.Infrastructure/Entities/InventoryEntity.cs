using Logitar.EventSourcing;
using PokeGame.Core.Inventory.Events;

namespace PokeGame.Infrastructure.Entities;

internal class InventoryEntity
{
  public TrainerEntity? Trainer { get; private set; }
  public int TrainerId { get; private set; }

  public ItemEntity? Item { get; private set; }
  public int ItemId { get; private set; }

  public int Quantity { get; set; }

  public InventoryEntity(TrainerEntity trainer, ItemEntity item, InventoryItemAdded @event) : this(trainer, item)
  {
    Add(@event);
  }
  public InventoryEntity(TrainerEntity trainer, ItemEntity item, InventoryItemUpdated @event) : this(trainer, item)
  {
    Update(@event);
  }
  private InventoryEntity(TrainerEntity trainer, ItemEntity item)
  {
    Trainer = trainer;
    TrainerId = trainer.TrainerId;

    Item = item;
    ItemId = item.ItemId;
  }

  private InventoryEntity()
  {
  }

  public IReadOnlyCollection<ActorId> GetActorIds() => Item?.GetActorIds() ?? [];

  public void Add(InventoryItemAdded @event)
  {
    Quantity += @event.Quantity;
  }

  public void Remove(InventoryItemRemoved @event)
  {
    Quantity -= @event.Quantity;
  }

  public void Update(InventoryItemUpdated @event)
  {
    Quantity = @event.Quantity;
  }

  public override bool Equals(object? obj) => obj is InventoryEntity inventory && inventory.TrainerId == TrainerId && inventory.ItemId == ItemId;
  public override int GetHashCode() => HashCode.Combine(TrainerId, ItemId);
  public override string ToString() => $"{base.ToString()} (TrainerId={TrainerId}, ItemId={ItemId})";
}
