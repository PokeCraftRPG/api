using Logitar.EventSourcing;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Membership.Events;

public record MembershipInvitationCreated(ReadOnlyEmail Email, UserId? InviteeId, DateTime? ExpiresOn) : DomainEvent;
