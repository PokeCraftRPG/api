using Krakenar.Contracts.Users;
using PokeGame.Core.Membership.Models;

namespace PokeGame.Core.Membership;

public interface IMembershipInvitationQuerier
{
  Task EnsureNonePendingAsync(IEmail email, CancellationToken cancellationToken = default);

  Task<MembershipInvitationModel> ReadAsync(MembershipInvitation membershipInvitation, CancellationToken cancellationToken = default);
  Task<MembershipInvitationModel?> ReadAsync(MembershipInvitationId id, CancellationToken cancellationToken = default);
  Task<MembershipInvitationModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
}
