using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public sealed record MoveUpdated(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
