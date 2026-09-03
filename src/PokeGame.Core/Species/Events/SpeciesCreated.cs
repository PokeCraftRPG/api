using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesCreated(Number Number, SpeciesCategory Category, Key Key) : DomainEvent;
