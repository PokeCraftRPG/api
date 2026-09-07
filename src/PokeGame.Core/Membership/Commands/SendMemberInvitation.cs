using Krakenar.Contracts.Users;
using Logitar.CQRS;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

internal record SendMemberInvitationCommand(Guid WorldId, SendMemberInvitationPayload Payload) : ICommand<MemberInvitationDto?>;

internal class SendMemberInvitationCommandHandler : ICommandHandler<SendMemberInvitationCommand, MemberInvitationDto?>
{
  private const int MemberInvitationLifetimeDays = 7;

  private readonly IContext _context;
  private readonly IMemberInvitationQuerier _memberInvitationQuerier;
  private readonly IMemberInvitationRepository _memberInvitationRepository;
  private readonly IMessageGateway _messageGateway;
  private readonly IPermissionService _permissionService;
  private readonly IUserGateway _userGateway;
  private readonly IWorldRepository _worldRepository;

  public SendMemberInvitationCommandHandler(
    IContext context,
    IMemberInvitationQuerier memberInvitationQuerier,
    IMemberInvitationRepository memberInvitationRepository,
    IMessageGateway messageGateway,
    IPermissionService permissionService,
    IUserGateway userGateway,
    IWorldRepository worldRepository)
  {
    _context = context;
    _memberInvitationQuerier = memberInvitationQuerier;
    _memberInvitationRepository = memberInvitationRepository;
    _messageGateway = messageGateway;
    _permissionService = permissionService;
    _userGateway = userGateway;
    _worldRepository = worldRepository;
  }

  public async Task<MemberInvitationDto?> HandleAsync(SendMemberInvitationCommand command, CancellationToken cancellationToken)
  {
    SendMemberInvitationPayload payload = command.Payload;
    payload.Validate();

    WorldId worldId = new(command.WorldId);
    World world = await _worldRepository.LoadAsync(worldId, cancellationToken) ?? throw new EntityNotFoundException(worldId, nameof(command.WorldId));
    await _permissionService.CheckAsync(Actions.InviteMember, world, cancellationToken);

    User? user = await _userGateway.FindAsync(payload.EmailAddress, cancellationToken);
    UserId? userId = user is null ? null : new(user);
    if (userId.HasValue && (world.OwnerId == userId.Value || world.IsMember(userId.Value)))
    {
      return null;
    }

    MemberInvitationId invitationId = MemberInvitationId.NewId(world.Id);
    DateTime expiresOn = DateTime.Now.AddDays(MemberInvitationLifetimeDays);
    MemberInvitation invitation;

    if (user is null)
    {
      EmailAddress emailAddress = new(payload.EmailAddress);
      MemberInvitationId? existingId = await _memberInvitationQuerier.FindIdAsync(emailAddress, MemberInvitationStatus.Pending, cancellationToken);
      if (existingId.HasValue)
      {
        throw new MemberInvitationAlreadyPendingException(existingId.Value);
      }

      invitation = new MemberInvitation(invitationId, emailAddress, expiresOn, _context.ActorId);
    }
    else
    {
      userId ??= new UserId(user);
      MemberInvitationId? existingId = await _memberInvitationQuerier.FindIdAsync(userId.Value, MemberInvitationStatus.Pending, cancellationToken);
      if (existingId.HasValue)
      {
        throw new MemberInvitationAlreadyPendingException(existingId.Value);
      }

      invitation = new MemberInvitation(invitationId, userId.Value, expiresOn, _context.ActorId);
    }

    await _memberInvitationRepository.SaveAsync(invitation, cancellationToken);
    await _messageGateway.SendMemberInvitationAsync(invitation, payload.Locale, cancellationToken);

    return await _memberInvitationQuerier.ReadAsync(invitation, cancellationToken);
  }
}
