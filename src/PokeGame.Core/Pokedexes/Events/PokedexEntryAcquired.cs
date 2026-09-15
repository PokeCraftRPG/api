using Logitar.EventSourcing;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokedexes.Events;

public sealed record PokedexEntryAcquired(VarietyId VarietyId) : DomainEvent;
