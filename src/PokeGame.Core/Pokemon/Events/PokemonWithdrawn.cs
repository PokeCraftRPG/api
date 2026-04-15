using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonWithdrawn(PokemonSlot Slot) : DomainEvent; // TODO(fpion): is this necessary? Should the box be null?
