using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public sealed record MoveKeyChanged(Key Key) : DomainEvent;
