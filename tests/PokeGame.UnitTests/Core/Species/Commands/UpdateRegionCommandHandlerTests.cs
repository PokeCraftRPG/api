using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Species.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdateSpeciesCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<ISpeciesManager> _speciesManager = new();
  private readonly Mock<ISpeciesQuerier> _speciesQuerier = new();
  private readonly Mock<ISpeciesRepository> _speciesRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly UpdateSpeciesCommandHandler _handler;

  public UpdateSpeciesCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _speciesManager.Object, _speciesQuerier.Object, _speciesRepository.Object, _storageService.Object);
  }

  [Fact(DisplayName = "It should return null when the species does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdateSpeciesPayload payload = new();
    UpdateSpeciesCommand command = new(Guid.Empty, payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdateSpeciesPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      CatchRate = 0,
      GrowthRate = (GrowthRate)99,
      EggCycles = 0,
      EggGroups = new EggGroupsModel(EggGroup.Field, (EggGroup)(-1)),
      Url = new Optional<string>("invalid")
    };
    UpdateSpeciesCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(7, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "CatchRate.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "GrowthRate.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "EggCycles.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "EggGroups.Secondary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
  }

  [Fact(DisplayName = "It should update the existing species.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    SpeciesAggregate species = new SpeciesBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _speciesRepository.Setup(x => x.LoadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);

    UpdateSpeciesPayload payload = new()
    {
      Key = "pikachu",
      Name = new Optional<string>("Pikachu"),
      BaseFriendship = 70,
      CatchRate = 190,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 10,
      EggGroups = new EggGroupsModel(EggGroup.Field, EggGroup.Fairy),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)"),
      Notes = new Optional<string>("Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact.")
    };
    payload.RegionalNumbers.Add(new RegionalNumberPayload("kanto", 25));
    payload.RegionalNumbers.Add(new RegionalNumberPayload(Guid.NewGuid().ToString(), 22));
    UpdateSpeciesCommand command = new(species.EntityId, payload);

    Dictionary<RegionId, Number?> regionalNumbers = new()
    {
      [RegionId.NewId(_context.WorldId)] = new Number(25),
      [RegionId.NewId(_context.WorldId)] = new Number(22)
    };
    _speciesManager.Setup(x => x.FindRegionalNumbersAsync(payload.RegionalNumbers, "RegionalNumbers", _cancellationToken)).ReturnsAsync(regionalNumbers);

    SpeciesModel model = new();
    _speciesQuerier.Setup(x => x.ReadAsync(species, _cancellationToken)).ReturnsAsync(model);

    SpeciesModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, species, _cancellationToken), Times.Once());
    _speciesQuerier.Verify(x => x.EnsureUnicityAsync(species, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(species, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }
}
