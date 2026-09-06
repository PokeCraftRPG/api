using Logitar.EventSourcing;

namespace PokeGame.Core.Membership.Events;

public sealed record MemberInvitationDeleted : DomainEvent, IDeleteEvent;
