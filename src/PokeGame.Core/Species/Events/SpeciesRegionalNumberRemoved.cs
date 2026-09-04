using Logitar.EventSourcing;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesRegionalNumberRemoved(RegionId RegionId) : DomainEvent;
