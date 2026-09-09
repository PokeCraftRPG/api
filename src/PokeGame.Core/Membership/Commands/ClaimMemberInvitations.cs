using Logitar.CQRS;
using PokeGame.Core.Identity;

namespace PokeGame.Core.Membership.Commands;

internal record ClaimMemberInvitationsCommand(UserId UserId, EmailAddress EmailAddress) : ICommand;

internal class ClaimMemberInvitationsCommandHandler : ICommandHandler<ClaimMemberInvitationsCommand, Unit>
{
  private readonly IMemberInvitationQuerier _memberInvitationQuerier;
  private readonly IMemberInvitationRepository _memberInvitationRepository;

  public ClaimMemberInvitationsCommandHandler(IMemberInvitationQuerier memberInvitationQuerier, IMemberInvitationRepository memberInvitationRepository)
  {
    _memberInvitationQuerier = memberInvitationQuerier;
    _memberInvitationRepository = memberInvitationRepository;
  }

  public async Task<Unit> HandleAsync(ClaimMemberInvitationsCommand command, CancellationToken cancellationToken)
  {
    IReadOnlyCollection<MemberInvitationId> invitationIds = await _memberInvitationQuerier.GetUnassignedPendingActiveIdsAsync(command.EmailAddress, cancellationToken);
    IReadOnlyCollection<MemberInvitation> invitations = await _memberInvitationRepository.LoadAsync(invitationIds, cancellationToken);
    foreach (MemberInvitation invitation in invitations)
    {
      invitation.Claim(command.UserId);
    }
    await _memberInvitationRepository.SaveAsync(invitations, cancellationToken);

    return Unit.Value;
  }
}
