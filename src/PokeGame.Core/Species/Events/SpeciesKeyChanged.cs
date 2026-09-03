using Logitar.EventSourcing;

namespace PokeGame.Core.Species.Events;

public sealed record SpeciesKeyChanged(Key Key) : DomainEvent;
