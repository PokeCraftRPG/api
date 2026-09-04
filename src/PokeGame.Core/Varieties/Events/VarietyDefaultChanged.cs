using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public sealed record VarietyDefaultChanged(bool IsDefault) : DomainEvent;
