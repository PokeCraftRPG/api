using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public sealed record RegionDeleted : DomainEvent, IDeleteEvent;
