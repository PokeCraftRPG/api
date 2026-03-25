using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public record VarietyKeyChanged(Slug Key) : DomainEvent;
