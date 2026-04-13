using Logitar.EventSourcing;
using PokeGame.Core.Inventory.Events;
using PokeGame.Core.Items;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory;

public class InventoryAggregate : AggregateRoot, IEntityProvider
{
  public const string EntityKind = "Inventory";

  public new InventoryId Id => new(base.Id);
  public TrainerId TrainerId => Id.TrainerId;

  private readonly Dictionary<ItemId, int> _quantities = [];
  public IReadOnlyDictionary<ItemId, int> Quantities => _quantities.AsReadOnly();

  public InventoryAggregate() : base()
  {
  }

  public InventoryAggregate(Trainer trainer) : base(new InventoryId(trainer.Id).StreamId)
  {
  }

  public void Add(Item item, int quantity, UserId userId) => Add(item.Id, quantity, userId);
  public void Add(ItemId itemId, int quantity, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, itemId, nameof(itemId));

    ArgumentOutOfRangeException.ThrowIfNegative(quantity, nameof(quantity));

    if (quantity > 0)
    {
      Raise(new InventoryItemAdded(itemId, quantity), userId.ActorId);
    }
  }
  protected virtual void Handle(InventoryItemAdded @event)
  {
    _quantities.TryGetValue(@event.ItemId, out int existingQuantity);
    _quantities[@event.ItemId] = existingQuantity + @event.Quantity;
  }

  public void EnsureQuantity(Item item, int quantity) => EnsureQuantity(item.Id, quantity);
  public void EnsureQuantity(ItemId itemId, int quantity)
  {
    if (!HasQuantity(itemId, quantity))
    {
      throw new InsufficientInventoryQuantityException(this, itemId, quantity);
    }
  }

  public int GetQuantity(Item item) => GetQuantity(item.Id);
  public int GetQuantity(ItemId itemId) => _quantities.TryGetValue(itemId, out int quantity) ? quantity : 0;

  public bool HasQuantity(Item item, int quantity) => HasQuantity(item.Id, quantity);
  public bool HasQuantity(ItemId itemId, int quantity) => GetQuantity(itemId) >= quantity;

  public void Remove(Item item, int quantity, UserId userId) => Remove(item.Id, quantity, userId);
  public void Remove(ItemId itemId, int quantity, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, itemId, nameof(itemId));

    ArgumentOutOfRangeException.ThrowIfNegative(quantity, nameof(quantity));

    if (quantity > 0)
    {
      if (_quantities.TryGetValue(itemId, out int existingQuantity) && existingQuantity < quantity)
      {
        quantity = existingQuantity;
      }
      Raise(new InventoryItemRemoved(itemId, quantity), userId.ActorId);
    }
  }
  protected virtual void Handle(InventoryItemRemoved @event)
  {
    _quantities.TryGetValue(@event.ItemId, out int existingQuantity);

    if (existingQuantity <= @event.Quantity)
    {
      _quantities.Remove(@event.ItemId);
    }
    else
    {
      _quantities[@event.ItemId] = existingQuantity - @event.Quantity;
    }
  }

  public void Update(Item item, int quantity, UserId userId) => Update(item.Id, quantity, userId);
  public void Update(ItemId itemId, int quantity, UserId userId)
  {
    WorldMismatchException.ThrowIfMismatch(Id, itemId, nameof(itemId));

    ArgumentOutOfRangeException.ThrowIfNegative(quantity, nameof(quantity));

    if (!_quantities.TryGetValue(itemId, out int existingQuantity) || existingQuantity != quantity)
    {
      Raise(new InventoryItemUpdated(itemId, quantity), userId.ActorId);
    }
  }
  protected virtual void Handle(InventoryItemUpdated @event)
  {
    if (@event.Quantity < 1)
    {
      _quantities.Remove(@event.ItemId);
    }
    else
    {
      _quantities[@event.ItemId] = @event.Quantity;
    }
  }

  public Entity GetEntity() => new(EntityKind, TrainerId.EntityId, TrainerId.WorldId);
}
