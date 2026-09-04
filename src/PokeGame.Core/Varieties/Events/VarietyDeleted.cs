using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public sealed record VarietyDeleted : DomainEvent, IDeleteEvent;
