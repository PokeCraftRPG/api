using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesDeleted : DomainEvent, IDeleteEvent;
