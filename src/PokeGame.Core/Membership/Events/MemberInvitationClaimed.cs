using Logitar.EventSourcing;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Membership.Events;

public sealed record MemberInvitationClaimed(UserId UserId) : DomainEvent;
