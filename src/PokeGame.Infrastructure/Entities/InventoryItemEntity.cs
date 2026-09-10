using Logitar.EventSourcing;

namespace PokeGame.Infrastructure.Entities;

internal class InventoryItemEntity
{
  public TrainerEntity? Trainer { get; private set; }
  public int TrainerId { get; private set; }

  public ItemEntity? Item { get; private set; }
  public int ItemId { get; private set; }

  public int Quantity { get; set; }

  public InventoryItemEntity(int trainerId, int itemId, int quantity)
  {
    TrainerId = trainerId;
    ItemId = itemId;
    Quantity = quantity;
  }

  private InventoryItemEntity()
  {
  }

  public IReadOnlyCollection<ActorId> GetActorIds() => Item?.GetActorIds() ?? [];

  public override bool Equals(object? obj) => obj is InventoryItemEntity inventory && inventory.TrainerId == TrainerId && inventory.ItemId == ItemId;
  public override int GetHashCode() => HashCode.Combine(TrainerId, ItemId);
  public override string ToString() => $"{base.ToString()} (TrainerId={TrainerId}, ItemId={ItemId})";
}
