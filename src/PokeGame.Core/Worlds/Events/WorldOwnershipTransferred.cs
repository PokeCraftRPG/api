using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldOwnershipTransferred(UserId OwnerId) : DomainEvent;
