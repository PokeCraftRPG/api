using Logitar.EventSourcing;

namespace PokeGame.Core.Regions.Events;

public record RegionCreated(Name Name) : DomainEvent;
