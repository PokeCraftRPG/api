using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public record MoveKeyChanged(Slug Key) : DomainEvent;
