using Logitar.EventSourcing;
using PokeGame.Core.Inventory.Events;
using PokeGame.Core.Items;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory;

public sealed class TrainerInventory : AggregateRoot, IEntityProvider
{
  public const int MinimumQuantity = 0;
  public const int MaximumQuantity = 999;

  public const string EntityKind = "TrainerInventory";

  public new InventoryId Id => new(base.Id);
  public TrainerId TrainerId => Id.TrainerId;

  private readonly Dictionary<ItemId, int> _quantities = [];
  public IReadOnlyDictionary<ItemId, int> Quantities => _quantities.AsReadOnly();

  public TrainerInventory() : base()
  {
  }

  public TrainerInventory(Trainer trainer) : this(trainer.Id)
  {
  }

  public TrainerInventory(TrainerId trainerId) : this(new InventoryId(trainerId))
  {
  }

  public TrainerInventory(InventoryId inventoryId) : base(inventoryId.StreamId)
  {
  }

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);

  public void AdjustQuantity(Item item, int delta, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, item, nameof(item));
    ArgumentOutOfRangeException.ThrowIfZero(delta);

    ItemId itemId = item.Id;
    int quantity = _quantities.GetValueOrDefault(itemId) + delta;
    ChangeQuantity(itemId, quantity, actorId);
  }

  public void SetQuantity(Item item, int quantity, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, item, nameof(item));

    ChangeQuantity(item.Id, quantity, actorId);
  }

  private void ChangeQuantity(ItemId itemId, int quantity, ActorId? actorId)
  {
    if (quantity < MinimumQuantity || quantity > MaximumQuantity)
    {
      throw new InventoryQuantityOutOfRangeException(this, itemId, quantity);
    }

    int existingQuantity = _quantities.GetValueOrDefault(itemId);
    if (existingQuantity != quantity)
    {
      if (existingQuantity == 0)
      {
        Raise(new InventoryItemAdded(itemId, quantity), actorId);
      }
      else if (quantity == 0)
      {
        Raise(new InventoryItemRemoved(itemId), actorId);
      }
      else
      {
        Raise(new InventoryItemChanged(itemId, quantity), actorId);
      }
    }
  }

  private void Handle(InventoryItemAdded @event)
  {
    _quantities[@event.ItemId] = @event.Quantity;
  }
  private void Handle(InventoryItemChanged @event)
  {
    _quantities[@event.ItemId] = @event.Quantity;
  }
  private void Handle(InventoryItemRemoved @event)
  {
    _quantities.Remove(@event.ItemId);
  }
}
