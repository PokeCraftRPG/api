using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public record SpeciesRenamed(Name? Name) : DomainEvent;
