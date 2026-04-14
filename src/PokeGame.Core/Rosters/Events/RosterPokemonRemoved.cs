using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters.Events;

public record RosterPokemonRemoved(PokemonId PokemonId) : DomainEvent;
