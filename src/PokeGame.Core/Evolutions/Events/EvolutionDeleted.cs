using Logitar.EventSourcing;

namespace PokeGame.Core.Evolutions.Events;

public sealed record EvolutionDeleted : DomainEvent, IDeleteEvent;
