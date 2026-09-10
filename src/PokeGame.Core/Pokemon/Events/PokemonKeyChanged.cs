using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonKeyChanged(Key Key) : DomainEvent;
