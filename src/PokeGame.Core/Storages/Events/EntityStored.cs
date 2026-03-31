using Logitar.EventSourcing;

namespace PokeGame.Core.Storages.Events;

public record EntityStored(string Key, long Size) : DomainEvent;
