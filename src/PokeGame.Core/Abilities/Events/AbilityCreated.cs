using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public record AbilityCreated(Slug Key) : DomainEvent;
