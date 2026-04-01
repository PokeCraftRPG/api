using Logitar.CQRS;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

internal record DeclineMembershipInvitationCommand(Guid Id) : ICommand<MembershipInvitationModel?>;

internal class DeclineMembershipInvitationCommandHandler : ICommandHandler<DeclineMembershipInvitationCommand, MembershipInvitationModel?>
{
  private readonly IContext _context;
  private readonly IMembershipInvitationQuerier _membershipInvitationQuerier;
  private readonly IMembershipInvitationRepository _membershipInvitationRepository;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public DeclineMembershipInvitationCommandHandler(
    IContext context,
    IMembershipInvitationQuerier membershipInvitationQuerier,
    IMembershipInvitationRepository membershipInvitationRepository,
    IPermissionService permissionService,
    IWorldRepository worldRepository)
  {
    _context = context;
    _membershipInvitationQuerier = membershipInvitationQuerier;
    _membershipInvitationRepository = membershipInvitationRepository;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<MembershipInvitationModel?> HandleAsync(DeclineMembershipInvitationCommand command, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    MembershipInvitationId id = new(worldId, command.Id);
    MembershipInvitation? invitation = await _membershipInvitationRepository.LoadAsync(id, cancellationToken);
    if (invitation is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Decline, invitation, cancellationToken);

    invitation.Decline();
    if (!invitation.InviteeId.HasValue)
    {
      throw new InvalidOperationException("The invitation has no invitee.");
    }

    World world = await _worldRepository.LoadAsync(worldId, cancellationToken) ?? throw new InvalidOperationException($"The world 'Id={worldId}' was not loaded.");
    world.GrantMembership(invitation.InviteeId.Value, _context.UserId);

    await _membershipInvitationRepository.SaveAsync(invitation, cancellationToken);
    await _worldRepository.SaveAsync(world, cancellationToken);

    return await _membershipInvitationQuerier.ReadAsync(invitation, cancellationToken);
  }
}
