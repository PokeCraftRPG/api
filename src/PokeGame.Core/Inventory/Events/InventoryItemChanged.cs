using Logitar.EventSourcing;
using PokeGame.Core.Items;

namespace PokeGame.Core.Inventory.Events;

public sealed record InventoryItemChanged(ItemId ItemId, int Quantity) : DomainEvent;
