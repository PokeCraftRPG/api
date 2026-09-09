using Logitar.EventSourcing;

namespace PokeGame.Core.Specimens.Events;

public sealed record PokemonDeleted : DomainEvent, IDeleteEvent;
