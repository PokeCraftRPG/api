using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public record MoveRenamed(Name? Name) : DomainEvent;
