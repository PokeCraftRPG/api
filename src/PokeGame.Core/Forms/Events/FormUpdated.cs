using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public sealed record FormUpdated(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
