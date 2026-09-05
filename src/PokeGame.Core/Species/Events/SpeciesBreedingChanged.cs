using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesBreedingChanged(SpeciesEggs Eggs) : DomainEvent;
