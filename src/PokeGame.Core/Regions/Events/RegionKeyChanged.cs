using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public sealed record RegionKeyChanged(Key Key) : DomainEvent;
