using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters.Events;

public record RosterPokemonMoved(PokemonId PokemonId, PokemonSlot Slot) : DomainEvent;
