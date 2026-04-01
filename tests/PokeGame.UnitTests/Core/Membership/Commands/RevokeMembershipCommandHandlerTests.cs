using Bogus;
using Krakenar.Contracts.Users;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Actors;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Events;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Membership.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class RevokeMembershipCommandHandlerTests
{
  private readonly Faker _faker = new();
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IWorldQuerier> _worldQuerier = new();
  private readonly Mock<IWorldRepository> _worldRepository = new();

  private readonly TestContext _context;
  private readonly RevokeMembershipCommandHandler _handler;

  public RevokeMembershipCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _worldQuerier.Object, _worldRepository.Object);
  }

  [Fact(DisplayName = "It should not do anything when the user is not a member.")]
  public async Task Given_NotMember_When_HandleAsync_Then_DoNothing()
  {
    Assert.NotNull(_context.World);
    _worldRepository.Setup(x => x.LoadAsync(_context.World.Id, _cancellationToken)).ReturnsAsync(_context.World);

    Assert.Empty(_context.World.Members);

    WorldModel model = new();
    _worldQuerier.Setup(x => x.ReadAsync(_context.World, _cancellationToken)).ReturnsAsync(model);

    RevokeMembershipCommand command = new(Guid.NewGuid());
    WorldModel result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Same(model, result);

    Assert.Empty(_context.World.Members);
  }

  [Fact(DisplayName = "It should revoke the user membership.")]
  public async Task Given_Member_When_HandleAsync_Then_Revoked()
  {
    Assert.NotNull(_context.World);
    _worldRepository.Setup(x => x.LoadAsync(_context.World.Id, _cancellationToken)).ReturnsAsync(_context.World);

    User member = new UserBuilder().Build();
    UserId memberId = member.GetUserId();

    _context.World.GrantMembership(memberId, _context.World.OwnerId);
    Assert.Contains(memberId, _context.World.Members);

    WorldModel model = new();
    _worldQuerier.Setup(x => x.ReadAsync(_context.World, _cancellationToken)).ReturnsAsync(model);

    RevokeMembershipCommand command = new(member.Id);
    WorldModel result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Same(model, result);

    Assert.Empty(_context.World.Members);
    Assert.Contains(_context.World.Changes, change => change is WorldMembershipRevoked revoked && revoked.UserId == memberId && revoked.ActorId == _context.World.OwnerId.ActorId);
  }
}
