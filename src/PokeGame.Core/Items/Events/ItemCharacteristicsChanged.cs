using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public sealed record ItemCharacteristicsChanged(Price? Price, Weight? Weight) : DomainEvent;
