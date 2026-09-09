using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public sealed record ItemPropertiesChanged(Price? Price, Weight? Weight) : DomainEvent;
