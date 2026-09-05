using Logitar.EventSourcing;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Membership.Events;

public sealed record MemberInvitationSent(EmailAddress? EmailAddress, UserId? UserId, DateTime? ExpiresOn) : DomainEvent;
