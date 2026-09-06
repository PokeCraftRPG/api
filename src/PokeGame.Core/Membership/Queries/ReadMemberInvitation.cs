using Logitar.CQRS;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership.Queries;

internal record ReadMemberInvitationQuery(Guid Id) : IQuery<MemberInvitationDto?>;

internal class ReadMemberInvitationQueryHandler : IQueryHandler<ReadMemberInvitationQuery, MemberInvitationDto?>
{
  private readonly IMemberInvitationQuerier _memberInvitationQuerier;

  public ReadMemberInvitationQueryHandler(IMemberInvitationQuerier memberInvitationQuerier)
  {
    _memberInvitationQuerier = memberInvitationQuerier;
  }

  public async Task<MemberInvitationDto?> HandleAsync(ReadMemberInvitationQuery query, CancellationToken cancellationToken)
  {
    return await _memberInvitationQuerier.ReadAsync(query.Id, cancellationToken);
  }
}
