using Logitar.EventSourcing;

namespace PokeGame.Core.Forms.Events;

public sealed record FormDetailsChanged(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
