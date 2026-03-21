using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldCreated(UserId OwnerId, Name Name) : DomainEvent;
