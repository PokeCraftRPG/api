using Bogus;
using Krakenar.Contracts.Users;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Actors;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class DeclineMembershipInvitationCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMembershipInvitationQuerier> _membershipInvitationQuerier = new();
  private readonly Mock<IMembershipInvitationRepository> _membershipInvitationRepository = new();
  private readonly Mock<IPermissionService> _permissionService = new();

  private readonly TestContext _context;
  private readonly DeclineMembershipInvitationCommandHandler _handler;

  private readonly World _world;
  private readonly User _invitee;

  public DeclineMembershipInvitationCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _membershipInvitationQuerier.Object, _membershipInvitationRepository.Object, _permissionService.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;
    _invitee = new UserBuilder().Build();
  }

  [Fact(DisplayName = "It should decline a membership invitation.")]
  public async Task Given_Invitation_When_HandleAsync_Then_Declined()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithWorld(_context.World).WithInvitee(_invitee).Build();
    _membershipInvitationRepository.Setup(x => x.LoadAsync(invitation.Id, _cancellationToken)).ReturnsAsync(invitation);

    MembershipInvitationModel model = new();
    _membershipInvitationQuerier.Setup(x => x.ReadAsync(invitation, _cancellationToken)).ReturnsAsync(model);

    DeclineMembershipInvitationCommand command = new(invitation.EntityId);
    MembershipInvitationModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    Assert.Equal(MembershipInvitationStatus.Declined, invitation.Status);
    Assert.False(_world.IsMember(_invitee.GetUserId()));

    _permissionService.Verify(x => x.CheckAsync(Actions.Decline, invitation, _cancellationToken), Times.Once());
    _membershipInvitationRepository.Verify(x => x.SaveAsync(invitation, _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should return null when the invitation was not found.")]
  public async Task Given_NotFound_When_HandleAsync_Then_NullReturned()
  {
    DeclineMembershipInvitationCommand command = new(Guid.NewGuid());
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }
}
