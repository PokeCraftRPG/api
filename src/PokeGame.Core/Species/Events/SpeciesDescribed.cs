using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public record SpeciesDescribed(Description? Description) : DomainEvent;
