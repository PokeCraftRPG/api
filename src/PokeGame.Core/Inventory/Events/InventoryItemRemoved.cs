using Logitar.EventSourcing;
using PokeGame.Core.Items;

namespace PokeGame.Core.Inventory.Events;

public sealed record InventoryItemRemoved(ItemId ItemId) : DomainEvent;
