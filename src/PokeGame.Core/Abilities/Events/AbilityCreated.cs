using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public sealed record AbilityCreated(Key Key) : DomainEvent;
