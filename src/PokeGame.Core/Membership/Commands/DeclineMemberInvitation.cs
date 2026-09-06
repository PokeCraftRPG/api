using Logitar.CQRS;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Core.Membership.Commands;

internal record DeclineMemberInvitationCommand(Guid Id) : ICommand<MemberInvitationDto?>;

internal class DeclineMemberInvitationCommandHandler : ICommandHandler<DeclineMemberInvitationCommand, MemberInvitationDto?>
{
  private readonly IContext _context;
  private readonly IMemberInvitationQuerier _memberInvitationQuerier;
  private readonly IMemberInvitationRepository _memberInvitationRepository;
  private readonly IPermissionService _permissionService;

  public DeclineMemberInvitationCommandHandler(
    IContext context,
    IMemberInvitationQuerier memberInvitationQuerier,
    IMemberInvitationRepository memberInvitationRepository,
    IPermissionService permissionService)
  {
    _context = context;
    _memberInvitationQuerier = memberInvitationQuerier;
    _memberInvitationRepository = memberInvitationRepository;
    _permissionService = permissionService;
  }

  public async Task<MemberInvitationDto?> HandleAsync(DeclineMemberInvitationCommand command, CancellationToken cancellationToken)
  {
    MemberInvitationId invitationId = new(_context.WorldId, command.Id);
    MemberInvitation? invitation = await _memberInvitationRepository.LoadAsync(invitationId, cancellationToken);
    if (invitation is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Decline, invitation, cancellationToken);

    invitation.Decline(_context.ActorId);

    await _memberInvitationRepository.SaveAsync(invitation, cancellationToken);

    return await _memberInvitationQuerier.ReadAsync(invitation, cancellationToken);
  }
}
