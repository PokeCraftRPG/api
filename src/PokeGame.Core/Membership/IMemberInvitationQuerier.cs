using Krakenar.Contracts.Search;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership;

public interface IMemberInvitationQuerier
{
  Task<MemberInvitationId?> GetIdAsync(MemberInvitation invitation, MemberInvitationStatus status = MemberInvitationStatus.Pending, CancellationToken cancellationToken = default);

  Task<MemberInvitationDto> ReadAsync(MemberInvitation invitation, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> ReadAsync(MemberInvitationId id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);

  Task<SearchResults<MemberInvitationDto>> SearchReceivedAsync(SearchMemberInvitationsPayload payload, CancellationToken cancellationToken = default);
  Task<SearchResults<MemberInvitationDto>> SearchWorldAsync(World world, SearchMemberInvitationsPayload payload, CancellationToken cancellationToken = default);
}
