using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Items;

namespace PokeGame.Core.Inventory;

public class InsufficientInventoryQuantityException : DomainException
{
  private const string ErrorMessage = "The inventory does not have a sufficient quantity of the specified item.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid TrainerId
  {
    get => (Guid)Data[nameof(TrainerId)]!;
    private set => Data[nameof(TrainerId)] = value;
  }
  public Guid ItemId
  {
    get => (Guid)Data[nameof(ItemId)]!;
    private set => Data[nameof(ItemId)] = value;
  }
  public int AvailableQuantity
  {
    get => (int)Data[nameof(AvailableQuantity)]!;
    private set => Data[nameof(AvailableQuantity)] = value;
  }
  public int RequiredQuantity
  {
    get => (int)Data[nameof(RequiredQuantity)]!;
    private set => Data[nameof(RequiredQuantity)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(TrainerId)] = TrainerId;
      error.Data[nameof(ItemId)] = ItemId;
      error.Data[nameof(AvailableQuantity)] = AvailableQuantity;
      error.Data[nameof(RequiredQuantity)] = RequiredQuantity;
      return error;
    }
  }

  public InsufficientInventoryQuantityException(InventoryAggregate inventory, Item item, int requiredQuantity)
    : this(inventory, item.Id, requiredQuantity)
  {
  }
  public InsufficientInventoryQuantityException(InventoryAggregate inventory, ItemId itemId, int requiredQuantity)
    : base(BuildMessage(inventory, itemId, requiredQuantity))
  {
    WorldId = inventory.TrainerId.WorldId.ToGuid();
    TrainerId = inventory.TrainerId.EntityId;
    ItemId = itemId.EntityId;
    RequiredQuantity = requiredQuantity;
    AvailableQuantity = inventory.GetQuantity(itemId);
  }

  private static string BuildMessage(InventoryAggregate inventory, ItemId itemId, int requiredQuantity) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), inventory.TrainerId.WorldId.ToGuid())
    .AddData(nameof(TrainerId), inventory.TrainerId.EntityId)
    .AddData(nameof(ItemId), itemId.EntityId)
    .AddData(nameof(AvailableQuantity), inventory.GetQuantity(itemId))
    .AddData(nameof(RequiredQuantity), requiredQuantity)
    .Build();
}
