using Logitar.EventSourcing;

namespace PokeGame.Core.Abilities.Events;

public sealed record AbilityDeleted : DomainEvent, IDeleteEvent;
