using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public record AbilityRenamed(Name? Name) : DomainEvent;
