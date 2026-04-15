using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonWithdrawn(int Position) : DomainEvent;
