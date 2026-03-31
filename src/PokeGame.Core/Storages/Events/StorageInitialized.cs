using Logitar.EventSourcing;

namespace PokeGame.Core.Storages.Events;

public record StorageInitialized(long AllocatedBytes) : DomainEvent;
