using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public sealed record MoveDeleted : DomainEvent, IDeleteEvent;
