using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public sealed record ItemCreated(ItemCategory Category, Key Key) : DomainEvent;
