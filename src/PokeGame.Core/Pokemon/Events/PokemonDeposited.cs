using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public record PokemonDeposited(int Position, int Box) : DomainEvent;
