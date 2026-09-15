using Logitar.EventSourcing;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokedexes.Events;

public sealed record PokedexEntrySeen(VarietyId VarietyId) : DomainEvent;
