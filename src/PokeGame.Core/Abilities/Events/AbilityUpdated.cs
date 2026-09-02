using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public sealed record AbilityUpdated(Name? Name, Summary? Summary, Content? Content) : DomainEvent;
