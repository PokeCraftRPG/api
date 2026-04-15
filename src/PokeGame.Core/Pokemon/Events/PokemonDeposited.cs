using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonDeposited(PokemonSlot Slot) : DomainEvent; // TODO(fpion): is this necessary? Should the box be necessary?
