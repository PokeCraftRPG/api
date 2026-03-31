using Logitar.EventSourcing;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Membership.Events;

public record MembershipInvitationCreated(Email Email, UserId? InviteeId, DateTime? ExpiresOn) : DomainEvent;
