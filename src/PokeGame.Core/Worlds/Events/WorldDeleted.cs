using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public sealed record WorldDeleted : DomainEvent, IDeleteEvent;
