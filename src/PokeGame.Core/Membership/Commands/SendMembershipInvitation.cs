using Krakenar.Contracts.Actors;
using Krakenar.Contracts.Users;
using Logitar.CQRS;
using Logitar.EventSourcing;
using PokeGame.Core.Actors;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;

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

  public SendMembershipInvitationCommandHandler(
    IContext context,
    IMembershipInvitationQuerier membershipInvitationQuerier,
    IMembershipInvitationRepository membershipInvitationRepository,
    MembershipSettings membershipSettings,
    IMessageGateway messageGateway,
    IPermissionService permissionService,
    IUserGateway userGateway)
  {
    _context = context;
    _membershipInvitationQuerier = membershipInvitationQuerier;
    _membershipInvitationRepository = membershipInvitationRepository;
    _membershipSettings = membershipSettings;
    _messageGateway = messageGateway;
    _permissionService = permissionService;
    _userGateway = userGateway;
  }

  public async Task<MembershipInvitationModel> HandleAsync(SendMembershipInvitationCommand command, CancellationToken cancellationToken)
  {
    SendMembershipInvitationPayload payload = command.Payload;
    payload.Validate();

    await _permissionService.CheckAsync(Actions.SendMembershipInvitation, cancellationToken);

    ReadOnlyEmail email = new(payload.EmailAddress);
    await _membershipInvitationQuerier.EnsureNonePendingAsync(email, cancellationToken);

    User? user = await _userGateway.FindAsync(payload.EmailAddress, cancellationToken);
    UserId? inviteeId = null;
    if (user is not null)
    {
      // TODO(fpion): load world and ensure user is not already a member (or retrieve from the context)

      Actor actor = new(user);
      ActorId actorId = actor.GetActorId();
      inviteeId = new(actorId);
    }

    DateTime expiresOn = DateTime.UtcNow.AddDays(_membershipSettings.InvitationLifetimeDays);
    MembershipInvitation invitation = new(email, inviteeId, expiresOn, _context.UserId, MembershipInvitationId.NewId(_context.WorldId));

    await _membershipInvitationRepository.SaveAsync(invitation, cancellationToken);

    if (user is null)
    {
      await _messageGateway.SendMembershipInvitationAsync(email, payload.Locale, cancellationToken);
    }
    else
    {
      await _messageGateway.SendMembershipInvitationAsync(user, payload.Locale, cancellationToken);
    }

    return await _membershipInvitationQuerier.ReadAsync(invitation, cancellationToken);
  }
}
