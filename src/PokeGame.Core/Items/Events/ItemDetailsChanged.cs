using Logitar.EventSourcing;

namespace PokeGame.Core.Items.Events;

public sealed record ItemDetailsChanged(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
