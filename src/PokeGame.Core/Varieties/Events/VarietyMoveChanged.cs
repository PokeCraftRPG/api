using Logitar.EventSourcing;

namespace PokeGame.Core.Varieties.Events;

public sealed record VarietyMoveChanged(Guid VarietyMoveId, VarietyMove Move) : DomainEvent;
