using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public record VarietyDefaultChanged(bool IsDefault) : DomainEvent;
