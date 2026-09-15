using PokeGame.Core.Items;

namespace PokeGame.Core.Inventories;

public sealed class InventoryQuantityOutOfRangeException : DomainException
{
  public InventoryQuantityOutOfRangeException(Inventory inventory, ItemId itemId, int attemptedQuantity)
    : base("The specified inventory quantity is out of range.")
  {
    Data["WorldId"] = inventory.TrainerId.WorldId.EntityId;
    Data["TrainerId"] = inventory.TrainerId.EntityId;
    Data["ItemId"] = itemId.EntityId;
    Data["MinimumQuantity"] = Inventory.MinimumQuantity;
    Data["MaximumQuantity"] = Inventory.MaximumQuantity;
    Data["AttemptedQuantity"] = attemptedQuantity;
    Data["PropertyName"] = "Quantity";
  }
}
