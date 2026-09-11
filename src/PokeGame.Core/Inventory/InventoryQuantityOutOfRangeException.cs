using PokeGame.Core.Items;

namespace PokeGame.Core.Inventory;

public sealed class InventoryQuantityOutOfRangeException : DomainException
{
  public InventoryQuantityOutOfRangeException(TrainerInventory inventory, ItemId itemId, int attemptedQuantity)
    : base("The specified inventory quantity is out of range.")
  {
    Data["WorldId"] = inventory.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = inventory.TrainerId.EntityId;
    Data["ItemId"] = itemId.EntityId;
    Data["MinimumQuantity"] = TrainerInventory.MinimumQuantity;
    Data["MaximumQuantity"] = TrainerInventory.MaximumQuantity;
    Data["AttemptedQuantity"] = attemptedQuantity;
    Data["PropertyName"] = "Quantity";
  }
}
