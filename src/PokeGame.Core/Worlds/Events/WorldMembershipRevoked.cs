using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldMembershipRevoked(UserId UserId) : DomainEvent;
