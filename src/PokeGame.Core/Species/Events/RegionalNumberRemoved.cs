using Logitar.EventSourcing;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Species.Events;

public record RegionalNumberRemoved(RegionId RegionId) : DomainEvent;
