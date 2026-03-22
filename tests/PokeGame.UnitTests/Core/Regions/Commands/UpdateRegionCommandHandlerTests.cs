using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Regions.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateRegionCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IRegionQuerier> _regionQuerier = new();
  private readonly Mock<IRegionRepository> _regionRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly UpdateRegionCommandHandler _handler;

  public UpdateRegionCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _regionQuerier.Object, _regionRepository.Object, _storageService.Object);
  }

  [Fact(DisplayName = "It should return null when the region does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateRegionPayload payload = new();
    UpdateRegionCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateRegionPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      Url = new Optional<string>("invalid")
    };
    UpdateRegionCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(3, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
  }

  [Fact(DisplayName = "It should update the existing region.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Region region = new RegionBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _regionRepository.Setup(x => x.LoadAsync(region.Id, _cancellationToken)).ReturnsAsync(region);

    UpdateRegionPayload payload = new()
    {
      Key = "kanto",
      Name = new Optional<string>("Kanto"),
      Description = new Optional<string>("Kanto is the classic region where many Trainer journeys begin: Professor Oak, Pallet Town, 8 Gyms, Team Rocket, and the Indigo League."),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Kanto_(Region)"),
      Notes = new Optional<string>("Classic, temperate region east of Johto: 10 settlements, 8 Gyms, Team Rocket, Oak in Pallet, and the Indigo League at Indigo Plateau.")
    };
    UpdateRegionCommand command = new(region.EntityId, payload);

    RegionModel model = new();
    _regionQuerier.Setup(x => x.ReadAsync(region, _cancellationToken)).ReturnsAsync(model);

    RegionModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, region, _cancellationToken), Times.Once());
    _regionQuerier.Verify(x => x.EnsureUnicityAsync(region, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(region, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }
}
