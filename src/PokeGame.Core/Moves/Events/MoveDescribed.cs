using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public record MoveDescribed(Description? Description) : DomainEvent;
