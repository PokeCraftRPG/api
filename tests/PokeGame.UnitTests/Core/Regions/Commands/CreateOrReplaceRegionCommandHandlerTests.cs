using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Regions.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceRegionCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IRegionQuerier> _regionQuerier = new();
  private readonly Mock<IRegionRepository> _regionRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceRegionCommandHandler _handler;

  public CreateOrReplaceRegionCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _regionQuerier.Object, _regionRepository.Object, _storageService.Object);
  }

  [Theory(DisplayName = "It should create a new region.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "kanto",
      Name = "Kanto",
      Description = "Kanto is the classic region where many Trainer journeys begin: Professor Oak, Pallet Town, 8 Gyms, Team Rocket, and the Indigo League.",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Kanto_(Region)",
      Notes = "Classic, temperate region east of Johto: 10 settlements, 8 Gyms, Team Rocket, Oak in Pallet, and the Indigo League at Indigo Plateau."
    };
    CreateOrReplaceRegionCommand command = new(payload, id);

    RegionModel model = new();
    _regionQuerier.Setup(x => x.ReadAsync(It.IsAny<Region>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceRegionResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Region);

    if (id.HasValue)
    {
      RegionId regionId = new(_context.WorldId, id.Value);
      _regionRepository.Verify(x => x.LoadAsync(regionId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateRegion, _cancellationToken), Times.Once());
    _regionQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Region>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Region>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing region.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Region region = new RegionBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _regionRepository.Setup(x => x.LoadAsync(region.Id, _cancellationToken)).ReturnsAsync(region);

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "kanto",
      Name = "Kanto",
      Description = "Kanto is the classic region where many Trainer journeys begin: Professor Oak, Pallet Town, 8 Gyms, Team Rocket, and the Indigo League.",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Kanto_(Region)",
      Notes = "Classic, temperate region east of Johto: 10 settlements, 8 Gyms, Team Rocket, Oak in Pallet, and the Indigo League at Indigo Plateau."
    };
    CreateOrReplaceRegionCommand command = new(payload, region.EntityId);

    RegionModel model = new();
    _regionQuerier.Setup(x => x.ReadAsync(region, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceRegionResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Region);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, region, _cancellationToken), Times.Once());
    _regionQuerier.Verify(x => x.EnsureUnicityAsync(region, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(region, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Url = "invalid"
    };
    CreateOrReplaceRegionCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(3, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
  }
}
