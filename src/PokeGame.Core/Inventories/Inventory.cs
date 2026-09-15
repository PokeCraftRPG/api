using Logitar.EventSourcing;
using PokeGame.Core.Inventories.Events;
using PokeGame.Core.Items;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventories;

public sealed class Inventory : AggregateRoot, IEntityProvider
{
  public const int MinimumQuantity = 0;
  public const int MaximumQuantity = 999;

  public const string EntityKind = "Inventory";

  public new InventoryId Id => new(base.Id);
  public TrainerId TrainerId => Id.TrainerId;

  private readonly Dictionary<ItemId, int> _quantities = [];
  public IReadOnlyDictionary<ItemId, int> Quantities => _quantities.AsReadOnly();

  public Inventory() : base()
  {
  }

  public Inventory(Trainer trainer) : this(trainer.Id)
  {
  }

  public Inventory(TrainerId trainerId) : this(new InventoryId(trainerId))
  {
  }

  public Inventory(InventoryId inventoryId) : base(inventoryId.StreamId)
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

  public void UseItem(ItemId itemId, ActorId? actorId = null)
  {
    WorldMismatchException.ThrowIfMismatch(this, itemId, nameof(itemId));

    int quantity = _quantities.GetValueOrDefault(itemId) - 1;
    ChangeQuantity(itemId, quantity, actorId);
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
