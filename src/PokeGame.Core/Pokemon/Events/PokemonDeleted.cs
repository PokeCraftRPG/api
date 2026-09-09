using Logitar.EventSourcing;

namespace PokeGame.Core.Pokemon.Events;

public sealed record PokemonDeleted : DomainEvent, IDeleteEvent;
