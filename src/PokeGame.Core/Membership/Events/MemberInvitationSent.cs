using Logitar.EventSourcing;
using PokeGame.Core.Identity;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Events;

public sealed record MemberInvitationSent(WorldId WorldId, EmailAddress EmailAddress, UserId? UserId, DateTime? ExpiresOn) : DomainEvent;
