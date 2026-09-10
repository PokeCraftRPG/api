using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Items;

namespace PokeGame.Core.Inventory;

public sealed class InventoryQuantityOutOfRangeException : DomainException
{
  private const string ErrorMessage = "The specified inventory quantity is out of range.";

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
  public int MinimumQuantity
  {
    get => (int)Data[nameof(MinimumQuantity)]!;
    private set => Data[nameof(MinimumQuantity)] = value;
  }
  public int MaximumQuantity
  {
    get => (int)Data[nameof(MaximumQuantity)]!;
    private set => Data[nameof(MaximumQuantity)] = value;
  }
  public int AttemptedQuantity
  {
    get => (int)Data[nameof(AttemptedQuantity)]!;
    private set => Data[nameof(AttemptedQuantity)] = value;
  }
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(TrainerId)] = TrainerId;
      error.Data[nameof(ItemId)] = ItemId;
      error.Data[nameof(MinimumQuantity)] = MinimumQuantity;
      error.Data[nameof(MaximumQuantity)] = MaximumQuantity;
      error.Data[nameof(AttemptedQuantity)] = AttemptedQuantity;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public InventoryQuantityOutOfRangeException(TrainerInventory inventory, ItemId itemId, int attemptedQuantity)
    : base(BuildMessage(inventory, itemId, attemptedQuantity))
  {
    WorldId = inventory.TrainerId.WorldId.EntityId;
    TrainerId = inventory.TrainerId.EntityId;
    ItemId = itemId.EntityId;
    MinimumQuantity = TrainerInventory.MinimumQuantity;
    MaximumQuantity = TrainerInventory.MaximumQuantity;
    AttemptedQuantity = attemptedQuantity;
    PropertyName = "Quantity";
  }

  private static string BuildMessage(TrainerInventory inventory, ItemId itemId, int attemptedQuantity) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), inventory.TrainerId.WorldId.EntityId)
    .AddData(nameof(TrainerId), inventory.TrainerId.EntityId)
    .AddData(nameof(ItemId), itemId.EntityId)
    .AddData(nameof(MinimumQuantity), TrainerInventory.MinimumQuantity)
    .AddData(nameof(MaximumQuantity), TrainerInventory.MaximumQuantity)
    .AddData(nameof(AttemptedQuantity), attemptedQuantity)
    .AddData(nameof(PropertyName), "Quantity")
    .Build();
}
