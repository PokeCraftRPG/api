using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public sealed record AbilityDetailsChanged(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
