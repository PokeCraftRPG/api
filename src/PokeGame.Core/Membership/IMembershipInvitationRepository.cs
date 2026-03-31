namespace PokeGame.Core.Membership;

public interface IMembershipInvitationRepository
{
  Task<MembershipInvitation?> LoadAsync(MembershipInvitationId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<MembershipInvitation>> LoadAsync(IEnumerable<MembershipInvitationId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(MembershipInvitation membershipInvitation, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<MembershipInvitation> membershipInvitations, CancellationToken cancellationToken = default);
}
