using Logitar.EventSourcing;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Worlds.Events;

public record WorldCreated(UserId OwnerId, Slug Key) : DomainEvent;
