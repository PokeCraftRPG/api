using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public sealed record AbilityKeyChanged(Key Key) : DomainEvent;
