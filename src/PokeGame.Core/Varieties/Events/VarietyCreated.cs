using Logitar.EventSourcing;
using PokeGame.Core.Species;

namespace PokeGame.Core.Varieties.Events;

public record VarietyCreated(SpeciesId SpeciesId, bool IsDefault, Slug Key) : DomainEvent;
