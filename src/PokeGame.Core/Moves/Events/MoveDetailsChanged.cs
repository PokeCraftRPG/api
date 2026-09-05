using Logitar.EventSourcing;

namespace PokeGame.Core.Moves.Events;

public sealed record MoveDetailsChanged(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
