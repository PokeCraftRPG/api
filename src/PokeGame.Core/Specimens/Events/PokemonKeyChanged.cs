using Logitar.EventSourcing;

namespace PokeGame.Core.Specimens.Events;

public sealed record PokemonKeyChanged(Key Key) : DomainEvent;
