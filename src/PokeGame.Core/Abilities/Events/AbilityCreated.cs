using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public record AbilityCreated(Name Name) : DomainEvent;
