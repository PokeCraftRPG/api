using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public record AbilityDescribed(Description? Description) : DomainEvent;
