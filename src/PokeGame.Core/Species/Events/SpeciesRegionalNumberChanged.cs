using Logitar.EventSourcing;
using PokeGame.Core.Regions;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesRegionalNumberChanged(RegionId RegionId, Number Number) : DomainEvent;
