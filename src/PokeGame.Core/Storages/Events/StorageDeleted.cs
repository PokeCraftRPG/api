using Logitar.EventSourcing;

namespace PokeGame.Core.Storages.Events;

public record StorageDeleted : DomainEvent, IDeleteEvent;
