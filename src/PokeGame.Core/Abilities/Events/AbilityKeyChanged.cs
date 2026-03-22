using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public record AbilityKeyChanged(Slug Key) : DomainEvent;
