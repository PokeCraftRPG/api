using PokeGame.Core.Moves.Models;

namespace PokeGame.Core.Membership;

public interface IMemberInvitationQuerier
{
  Task<MoveDto> ReadAsync(MemberInvitation invitation, CancellationToken cancellationToken = default);
  Task<MoveDto?> ReadAsync(MemberInvitationId id, CancellationToken cancellationToken = default);
  Task<MoveDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
}
