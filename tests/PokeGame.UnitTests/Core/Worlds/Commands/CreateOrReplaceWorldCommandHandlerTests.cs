using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceWorldCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IContext> _context = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();
  private readonly Mock<IWorldQuerier> _worldQuerier = new();
  private readonly Mock<IWorldRepository> _worldRepository = new();

  private readonly CreateOrReplaceWorldCommandHandler _handler;

  public CreateOrReplaceWorldCommandHandlerTests()
  {
    _handler = new(_context.Object, _permissionService.Object, _storageService.Object, _worldQuerier.Object, _worldRepository.Object);
  }

  [Theory(DisplayName = "It should create a new world.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "the-new-world",
      Name = "The New World",
      Description = "This is the new world."
    };
    CreateOrReplaceWorldCommand command = new(payload, id);

    WorldModel model = new();
    _worldQuerier.Setup(x => x.ReadAsync(It.IsAny<World>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceWorldResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.World);

    if (id.HasValue)
    {
      WorldId worldId = new(id.Value);
      _worldRepository.Verify(x => x.LoadAsync(worldId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateWorld, _cancellationToken), Times.Once());
    _worldQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<World>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<World>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing world.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    World world = new WorldBuilder(_faker).Build();
    world.ClearChanges();
    _worldRepository.Setup(x => x.LoadAsync(world.Id, _cancellationToken)).ReturnsAsync(world);

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "the-new-world",
      Name = "The New World",
      Description = "This is the new world."
    };
    CreateOrReplaceWorldCommand command = new(payload, world.Id.ToGuid());

    WorldModel model = new();
    _worldQuerier.Setup(x => x.ReadAsync(world, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceWorldResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.World);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, world, _cancellationToken), Times.Once());
    _worldQuerier.Verify(x => x.EnsureUnicityAsync(world, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(world, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z')
    };
    CreateOrReplaceWorldCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(2, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
  }
}
