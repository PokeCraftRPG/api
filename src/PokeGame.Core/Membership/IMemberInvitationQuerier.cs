using Krakenar.Contracts.Search;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership;

public interface IMemberInvitationQuerier
{
  Task<MemberInvitationId?> FindIdAsync(EmailAddress emailAddress, MemberInvitationStatus status = MemberInvitationStatus.Pending, CancellationToken cancellationToken = default);
  Task<MemberInvitationId?> FindIdAsync(UserId userId, MemberInvitationStatus status = MemberInvitationStatus.Pending, CancellationToken cancellationToken = default);

  Task<MemberInvitationDto> ReadAsync(MemberInvitation invitation, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> ReadAsync(MemberInvitationId id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);

  Task<SearchResults<MemberInvitationDto>> SearchAsync(SearchMemberInvitationsPayload payload, CancellationToken cancellationToken = default);
}
