using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public sealed record RegionCreated(Key Key) : DomainEvent;
