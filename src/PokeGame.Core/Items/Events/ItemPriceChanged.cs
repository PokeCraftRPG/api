using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public sealed record ItemPriceChanged(Price? Price) : DomainEvent;
