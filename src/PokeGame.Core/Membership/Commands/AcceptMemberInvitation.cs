using Logitar.CQRS;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

internal record AcceptMemberInvitationCommand(Guid Id) : ICommand<MemberInvitationDto?>;

internal class AcceptMemberInvitationCommandHandler : ICommandHandler<AcceptMemberInvitationCommand, MemberInvitationDto?>
{
  private readonly IContext _context;
  private readonly IMemberInvitationQuerier _memberInvitationQuerier;
  private readonly IMemberInvitationRepository _memberInvitationRepository;
  private readonly IPermissionService _permissionService;
  private readonly IWorldRepository _worldRepository;

  public AcceptMemberInvitationCommandHandler(
    IContext context,
    IMemberInvitationQuerier memberInvitationQuerier,
    IMemberInvitationRepository memberInvitationRepository,
    IPermissionService permissionService,
    IWorldRepository worldRepository)
  {
    _context = context;
    _memberInvitationQuerier = memberInvitationQuerier;
    _memberInvitationRepository = memberInvitationRepository;
    _permissionService = permissionService;
    _worldRepository = worldRepository;
  }

  public async Task<MemberInvitationDto?> HandleAsync(AcceptMemberInvitationCommand command, CancellationToken cancellationToken)
  {
    MemberInvitationId invitationId = new(_context.WorldId, command.Id);
    MemberInvitation? invitation = await _memberInvitationRepository.LoadAsync(invitationId, cancellationToken);
    if (invitation is null)
    {
      return null;
    }
    await _permissionService.CheckAsync(Actions.Accept, invitation, cancellationToken);

    World world = await _worldRepository.LoadAsync(_context.WorldId, cancellationToken)
      ?? throw new InvalidOperationException($"The world 'Id={_context.WorldId}' was not loaded.");
    UserId userId = invitation.UserId ?? throw new InvalidOperationException($"The member invitation 'Id={invitation.Id}' has no user identifier.");

    invitation.Accept(_context.ActorId);
    world.GrantMembership(userId, invitation.CreatedBy);

    await _memberInvitationRepository.SaveAsync(invitation, cancellationToken);
    await _worldRepository.SaveAsync(world, cancellationToken);

    return await _memberInvitationQuerier.ReadAsync(invitation, cancellationToken);
  }
}
