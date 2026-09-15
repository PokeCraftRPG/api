using Logitar.EventSourcing;
using PokeGame.Core.Items;

namespace PokeGame.Core.Inventories.Events;

public sealed record InventoryItemAdded(ItemId ItemId, int Quantity) : DomainEvent;
