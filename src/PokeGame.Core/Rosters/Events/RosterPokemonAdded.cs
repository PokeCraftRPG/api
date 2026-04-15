using Logitar.EventSourcing;
using PokeGame.Core.Pokemon;

namespace PokeGame.Core.Rosters.Events;

public record RosterPokemonAdded(PokemonId PokemonId, PokemonSlot Slot) : DomainEvent; // TODO(fpion): rename? merge with removed?
