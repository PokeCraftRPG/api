using Krakenar.Contracts.Users;
using Logitar.CQRS;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

internal record SendMembershipInvitationCommand(SendMembershipInvitationPayload Payload) : ICommand<MembershipInvitationModel>;

internal class SendMembershipInvitationCommandHandler : ICommandHandler<SendMembershipInvitationCommand, MembershipInvitationModel>
{
  private readonly IContext _context;
  private readonly IMembershipInvitationQuerier _membershipInvitationQuerier;
  private readonly IMembershipInvitationRepository _membershipInvitationRepository;
  private readonly MembershipSettings _membershipSettings;
  private readonly IMessageGateway _messageGateway;
  private readonly IPermissionService _permissionService;
  private readonly IUserGateway _userGateway;
  private readonly IWorldRepository _worldRepository;

  public SendMembershipInvitationCommandHandler(
    IContext context,
    IMembershipInvitationQuerier membershipInvitationQuerier,
    IMembershipInvitationRepository membershipInvitationRepository,
    MembershipSettings membershipSettings,
    IMessageGateway messageGateway,
    IPermissionService permissionService,
    IUserGateway userGateway,
    IWorldRepository worldRepository)
  {
    _context = context;
    _membershipInvitationQuerier = membershipInvitationQuerier;
    _membershipInvitationRepository = membershipInvitationRepository;
    _membershipSettings = membershipSettings;
    _messageGateway = messageGateway;
    _permissionService = permissionService;
    _userGateway = userGateway;
    _worldRepository = worldRepository;
  }

  public async Task<MembershipInvitationModel> HandleAsync(SendMembershipInvitationCommand command, CancellationToken cancellationToken)
  {
    SendMembershipInvitationPayload payload = command.Payload;
    payload.Validate();

    await _permissionService.CheckAsync(Actions.SendMembershipInvitation, cancellationToken);

    ReadOnlyEmail email = new(payload.EmailAddress);
    await _membershipInvitationQuerier.EnsureNonePendingAsync(email, cancellationToken);

    User? invitee = await _userGateway.FindAsync(payload.EmailAddress, cancellationToken);
    UserId? inviteeId = null;
    if (invitee is not null)
    {
      inviteeId = invitee.GetUserId();

      WorldId worldId = _context.WorldId;
      World world = await _worldRepository.LoadAsync(worldId, cancellationToken) ?? throw new InvalidOperationException($"The world 'Id={worldId}' was not loaded.");
      if (world.IsMember(inviteeId.Value))
      {
        throw new MembershipConflictException(world, invitee, payload.EmailAddress, nameof(payload.EmailAddress));
      }
    }

    DateTime expiresOn = DateTime.UtcNow.AddDays(_membershipSettings.InvitationLifetimeDays);
    MembershipInvitation invitation = new(email, inviteeId, expiresOn, _context.UserId, MembershipInvitationId.NewId(_context.WorldId));

    await _membershipInvitationRepository.SaveAsync(invitation, cancellationToken);

    if (invitee is null)
    {
      await _messageGateway.SendMembershipInvitationAsync(email, payload.Locale, cancellationToken);
    }
    else
    {
      await _messageGateway.SendMembershipInvitationAsync(invitee, payload.Locale, cancellationToken);
    }

    return await _membershipInvitationQuerier.ReadAsync(invitation, cancellationToken);
  }
}
