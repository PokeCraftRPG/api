using Logitar.EventSourcing;
using PokeGame.Core.Species;

namespace PokeGame.Core.Varieties.Events;

public sealed record VarietyCreated(SpeciesId SpeciesId, Key Key) : DomainEvent;
