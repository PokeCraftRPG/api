using Logitar.EventSourcing;
using PokeGame.Core.Membership;

namespace PokeGame.Infrastructure.Repositories;

internal class MemberInvitationRepository : Repository, IMemberInvitationRepository
{
  public MemberInvitationRepository(IEventStore eventStore) : base(eventStore)
  {
  }

  public async Task<MemberInvitation?> LoadAsync(MemberInvitationId id, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<MemberInvitation>(id.StreamId, cancellationToken);
  }
  public async Task<IReadOnlyCollection<MemberInvitation>> LoadAsync(IEnumerable<MemberInvitationId> ids, CancellationToken cancellationToken)
  {
    return await base.LoadAsync<MemberInvitation>(ids.Select(id => id.StreamId), cancellationToken);
  }

  public async Task SaveAsync(MemberInvitation invitation, CancellationToken cancellationToken)
  {
    await base.SaveAsync(invitation, cancellationToken);
  }
  public async Task SaveAsync(IEnumerable<MemberInvitation> invitations, CancellationToken cancellationToken)
  {
    await base.SaveAsync(invitations, cancellationToken);
  }
}
