using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership;

public interface IMemberInvitationQuerier
{
  Task<MemberInvitationDto> ReadAsync(MemberInvitation invitation, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> ReadAsync(MemberInvitationId id, CancellationToken cancellationToken = default);
  Task<MemberInvitationDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
}
