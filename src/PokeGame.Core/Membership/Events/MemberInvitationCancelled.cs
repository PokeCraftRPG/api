using Logitar.EventSourcing;

namespace PokeGame.Core.Membership.Events;

public sealed record MemberInvitationCancelled : DomainEvent;
