namespace PokeGame.Core.Membership;

public interface IMemberInvitationRepository
{
  Task<MemberInvitation?> LoadAsync(MemberInvitationId id, CancellationToken cancellationToken = default);
  Task<IReadOnlyCollection<MemberInvitation>> LoadAsync(IEnumerable<MemberInvitationId> ids, CancellationToken cancellationToken = default);

  Task SaveAsync(MemberInvitation invitation, CancellationToken cancellationToken = default);
  Task SaveAsync(IEnumerable<MemberInvitation> invitations, CancellationToken cancellationToken = default);
}
