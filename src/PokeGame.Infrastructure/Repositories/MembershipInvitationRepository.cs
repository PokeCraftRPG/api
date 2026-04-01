using Logitar.EventSourcing;
using PokeGame.Core.Membership;

namespace PokeGame.Infrastructure.Repositories;

internal class MembershipInvitationRepository : Repository, IMembershipInvitationRepository
{
  public MembershipInvitationRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<MembershipInvitation?> LoadAsync(MembershipInvitationId id, CancellationToken cancellationToken)
  {
    return await LoadAsync<MembershipInvitation>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<MembershipInvitation>> LoadAsync(IEnumerable<MembershipInvitationId> ids, CancellationToken cancellationToken)
  {
    return await LoadAsync<MembershipInvitation>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(MembershipInvitation membershipInvitation, CancellationToken cancellationToken)
  {
    await base.SaveAsync(membershipInvitation, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<MembershipInvitation> membershipInvitations, CancellationToken cancellationToken)
  {
    await base.SaveAsync(membershipInvitations, cancellationToken);
  }
}
