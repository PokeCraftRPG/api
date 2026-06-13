using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public record RegionRenamed(Name? Name) : DomainEvent;
