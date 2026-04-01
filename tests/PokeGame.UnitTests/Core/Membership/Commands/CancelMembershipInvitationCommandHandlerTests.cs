using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Membership.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Membership.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CancelMembershipInvitationCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMembershipInvitationQuerier> _membershipInvitationQuerier = new();
  private readonly Mock<IMembershipInvitationRepository> _membershipInvitationRepository = new();
  private readonly Mock<IPermissionService> _permissionService = new();

  private readonly TestContext _context;
  private readonly CancelMembershipInvitationCommandHandler _handler;

  private readonly World _world;

  public CancelMembershipInvitationCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _membershipInvitationQuerier.Object, _membershipInvitationRepository.Object, _permissionService.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;
  }

  [Fact(DisplayName = "It should cancel a membership invitation.")]
  public async Task Given_Invitation_When_HandleAsync_Then_Canceld()
  {
    MembershipInvitation invitation = new MembershipInvitationBuilder(_faker).WithWorld(_context.World).Build();
    _membershipInvitationRepository.Setup(x => x.LoadAsync(invitation.Id, _cancellationToken)).ReturnsAsync(invitation);

    MembershipInvitationModel model = new();
    _membershipInvitationQuerier.Setup(x => x.ReadAsync(invitation, _cancellationToken)).ReturnsAsync(model);

    CancelMembershipInvitationCommand command = new(invitation.EntityId);
    MembershipInvitationModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    Assert.Equal(MembershipInvitationStatus.Cancelled, invitation.Status);
    Assert.Empty(_world.Members);

    _permissionService.Verify(x => x.CheckAsync(Actions.Cancel, invitation, _cancellationToken), Times.Once());
    _membershipInvitationRepository.Verify(x => x.SaveAsync(invitation, _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should return null when the invitation was not found.")]
  public async Task Given_NotFound_When_HandleAsync_Then_NullReturned()
  {
    CancelMembershipInvitationCommand command = new(Guid.NewGuid());
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }
}
