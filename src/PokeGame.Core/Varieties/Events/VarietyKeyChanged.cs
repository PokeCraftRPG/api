using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public sealed record VarietyKeyChanged(Key Key) : DomainEvent;
