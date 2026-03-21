using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldDeleted : DomainEvent, IDeleteEvent;
