using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public sealed record WorldDetailsChanged(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
