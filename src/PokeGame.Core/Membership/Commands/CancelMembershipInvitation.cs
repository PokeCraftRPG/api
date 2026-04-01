using Logitar.CQRS;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Membership.Commands;

internal record CancelMembershipInvitationCommand(Guid Id) : ICommand<MembershipInvitationModel?>;

internal class CancelMembershipInvitationCommandHandler : ICommandHandler<CancelMembershipInvitationCommand, MembershipInvitationModel?>
{
  private readonly IContext _context;
  private readonly IMembershipInvitationQuerier _membershipInvitationQuerier;
  private readonly IMembershipInvitationRepository _membershipInvitationRepository;
  private readonly IPermissionService _permissionService;

  public CancelMembershipInvitationCommandHandler(
    IContext context,
    IMembershipInvitationQuerier membershipInvitationQuerier,
    IMembershipInvitationRepository membershipInvitationRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _membershipInvitationQuerier = membershipInvitationQuerier;
    _membershipInvitationRepository = membershipInvitationRepository;
    _permissionService = permissionService;
  }

  public async Task<MembershipInvitationModel?> HandleAsync(CancelMembershipInvitationCommand command, CancellationToken cancellationToken)
  {
    MembershipInvitationId id = new(_context.WorldId, command.Id);
    MembershipInvitation? invitation = await _membershipInvitationRepository.LoadAsync(id, cancellationToken);
    if (invitation is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Cancel, invitation, cancellationToken);

    invitation.Cancel(_context.UserId);

    await _membershipInvitationRepository.SaveAsync(invitation, cancellationToken);

    return await _membershipInvitationQuerier.ReadAsync(invitation, cancellationToken);
  }
}
