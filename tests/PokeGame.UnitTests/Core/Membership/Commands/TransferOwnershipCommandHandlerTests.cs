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
public class TransferOwnershipCommandHandlerTests
{
  private readonly Faker _faker = new();
  private readonly CancellationToken _cancellationToken = default;

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IWorldQuerier> _worldQuerier = new();
  private readonly Mock<IWorldRepository> _worldRepository = new();

  private readonly TestContext _context;
  private readonly TransferOwnershipCommandHandler _handler;

  private readonly World _world;

  public TransferOwnershipCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _worldQuerier.Object, _worldRepository.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;
  }

  [Fact(DisplayName = "It should throw InvalidOperationException when the world was not loaded.")]
  public async Task Given_WorldNotLoaded_When_HandleAsync_Then_InvalidOperationException()
  {
    TransferOwnershipCommand command = new(Guid.NewGuid());
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal($"The world 'Id={_world.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "It should throw MemberNotFoundException when the user is not a member.")]
  public async Task Given_NotMember_When_HandleAsync_Then_MemberNotFoundException()
  {
    _worldRepository.Setup(x => x.LoadAsync(_world.Id, _cancellationToken)).ReturnsAsync(_world);
    Assert.Empty(_world.Members);

    TransferOwnershipCommand command = new(Guid.NewGuid());
    var exception = await Assert.ThrowsAsync<MemberNotFoundException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(command.UserId, exception.UserId);
    Assert.Equal("UserId", exception.PropertyName);
  }

  [Fact(DisplayName = "It should transfer the world ownership.")]
  public async Task Given_Member_When_HandleAsync_Then_OwnershipTransferred()
  {
    _worldRepository.Setup(x => x.LoadAsync(_world.Id, _cancellationToken)).ReturnsAsync(_world);

    User member = new UserBuilder().Build();
    UserId memberId = member.GetUserId();

    _world.GrantMembership(memberId, _world.OwnerId);
    Assert.Contains(memberId, _world.Members);

    WorldModel model = new();
    _worldQuerier.Setup(x => x.ReadAsync(_world, _cancellationToken)).ReturnsAsync(model);

    UserId ownerId = _world.OwnerId;

    TransferOwnershipCommand command = new(member.Id);
    WorldModel result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Same(model, result);

    Assert.Equal(memberId, _world.OwnerId);
    Assert.True(_world.IsMember(ownerId));
    Assert.False(_world.IsMember(memberId));
    Assert.Contains(_world.Changes, change => change is WorldOwnershipTransferred transferred && transferred.OwnerId == memberId && transferred.ActorId == ownerId.ActorId);
  }
}
