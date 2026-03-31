using Logitar.CQRS;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership.Queries;

internal record ReadMembershipInvitationQuery(Guid Id) : IQuery<MembershipInvitationModel?>;

internal class ReadMembershipInvitationQueryHandler : IQueryHandler<ReadMembershipInvitationQuery, MembershipInvitationModel?>
{
  private readonly IMembershipInvitationQuerier _membershipInvitationQuerier;

  public ReadMembershipInvitationQueryHandler(IMembershipInvitationQuerier membershipInvitationQuerier)
  {
    _membershipInvitationQuerier = membershipInvitationQuerier;
  }

  public async Task<MembershipInvitationModel?> HandleAsync(ReadMembershipInvitationQuery query, CancellationToken cancellationToken)
  {
    return await _membershipInvitationQuerier.ReadAsync(query.Id, cancellationToken);
  }
}
