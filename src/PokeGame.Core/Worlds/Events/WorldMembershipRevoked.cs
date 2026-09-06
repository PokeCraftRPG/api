using Logitar.EventSourcing;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Worlds.Events;

public sealed record WorldMembershipRevoked(UserId UserId) : DomainEvent;
