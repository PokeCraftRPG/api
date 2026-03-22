using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Storages;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Core.Worlds.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateWorldCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IContext> _context = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IStorageService> _storageService = new();
  private readonly Mock<IWorldQuerier> _worldQuerier = new();
  private readonly Mock<IWorldRepository> _worldRepository = new();

  private readonly UpdateWorldCommandHandler _handler;

  public UpdateWorldCommandHandlerTests()
  {
    _handler = new(_context.Object, _permissionService.Object, _storageService.Object, _worldQuerier.Object, _worldRepository.Object);
  }

  [Fact(DisplayName = "It should return null when the world does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateWorldPayload payload = new();
    UpdateWorldCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateWorldPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z'))
    };
    UpdateWorldCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(2, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
  }

  [Fact(DisplayName = "It should update the existing world.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    World world = new WorldBuilder(_faker).Build();
    world.ClearChanges();
    _worldRepository.Setup(x => x.LoadAsync(world.Id, _cancellationToken)).ReturnsAsync(world);

    UpdateWorldPayload payload = new()
    {
      Key = "the-new-world",
      Name = new Optional<string>("The New World"),
      Description = new Optional<string>("This is the new world.")
    };
    UpdateWorldCommand command = new(world.Id.ToGuid(), payload);

    WorldModel model = new();
    _worldQuerier.Setup(x => x.ReadAsync(world, _cancellationToken)).ReturnsAsync(model);

    WorldModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, world, _cancellationToken), Times.Once());
    _worldQuerier.Verify(x => x.EnsureUnicityAsync(world, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(world, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }
}
