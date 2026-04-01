using Logitar.EventSourcing;

namespace PokeGame.Core.Worlds.Events;

public record WorldMembershipGranted(UserId UserId) : DomainEvent;
