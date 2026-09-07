using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership.Queries;

internal record SearchReceivedMemberInvitationsQuery(SearchMemberInvitationsPayload Payload) : IQuery<SearchResults<MemberInvitationDto>>;

internal class SearchReceivedMemberInvitationsQueryHandler : IQueryHandler<SearchReceivedMemberInvitationsQuery, SearchResults<MemberInvitationDto>>
{
  private readonly IMemberInvitationQuerier _memberInvitationQuerier;

  public SearchReceivedMemberInvitationsQueryHandler(IMemberInvitationQuerier memberInvitationQuerier)
  {
    _memberInvitationQuerier = memberInvitationQuerier;
  }

  public async Task<SearchResults<MemberInvitationDto>> HandleAsync(SearchReceivedMemberInvitationsQuery query, CancellationToken cancellationToken)
  {
    SearchMemberInvitationsPayload payload = query.Payload;
    payload.Validate();

    return await _memberInvitationQuerier.SearchReceivedAsync(payload, cancellationToken);
  }
}
