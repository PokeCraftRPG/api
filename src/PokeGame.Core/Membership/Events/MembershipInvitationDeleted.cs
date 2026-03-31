using Logitar.EventSourcing;

namespace PokeGame.Core.Membership.Events;

public record MembershipInvitationDeleted : DomainEvent, IDeleteEvent;
