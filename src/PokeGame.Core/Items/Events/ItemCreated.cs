using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public record ItemCreated(ItemCategory Category, Slug Key) : DomainEvent;
