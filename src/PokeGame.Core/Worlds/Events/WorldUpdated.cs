using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public sealed record WorldUpdated(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
