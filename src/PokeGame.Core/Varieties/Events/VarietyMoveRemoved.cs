using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public sealed record VarietyMoveRemoved(Guid VarietyMoveId) : DomainEvent;
