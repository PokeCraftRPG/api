using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership.Queries;

internal record SearchMemberInvitationsQuery(SearchMemberInvitationsPayload Payload) : IQuery<SearchResults<MemberInvitationDto>>;

internal class SearchMemberInvitationsQueryHandler : IQueryHandler<SearchMemberInvitationsQuery, SearchResults<MemberInvitationDto>>
{
  private readonly IMemberInvitationQuerier _memberInvitationQuerier;

  public SearchMemberInvitationsQueryHandler(IMemberInvitationQuerier memberInvitationQuerier)
  {
    _memberInvitationQuerier = memberInvitationQuerier;
  }

  public async Task<SearchResults<MemberInvitationDto>> HandleAsync(SearchMemberInvitationsQuery query, CancellationToken cancellationToken)
  {
    SearchMemberInvitationsPayload payload = query.Payload;
    payload.Validate();

    return await _memberInvitationQuerier.SearchAsync(payload, cancellationToken);
  }
}
