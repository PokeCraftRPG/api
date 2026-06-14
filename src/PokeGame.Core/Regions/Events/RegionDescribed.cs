using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public record RegionDescribed(Description? Description) : DomainEvent;
