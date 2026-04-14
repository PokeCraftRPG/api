using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Species.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceSpeciesCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<ISpeciesManager> _speciesManager = new();
  private readonly Mock<ISpeciesQuerier> _speciesQuerier = new();
  private readonly Mock<ISpeciesRepository> _speciesRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceSpeciesCommandHandler _handler;

  public CreateOrReplaceSpeciesCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _speciesManager.Object, _speciesQuerier.Object, _speciesRepository.Object, _storageService.Object);
  }

  [Theory(DisplayName = "It should create a new species.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 25,
      Category = PokemonCategory.Standard,
      Key = "pikachu",
      Name = "Pikachu",
      BaseFriendship = 70,
      CatchRate = 190,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 10,
      EggGroups = new EggGroupsModel(EggGroup.Field, EggGroup.Fairy),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact."
    };
    payload.RegionalNumbers.Add(new RegionalNumberPayload("kanto", 25));
    payload.RegionalNumbers.Add(new RegionalNumberPayload(Guid.NewGuid().ToString(), 22));
    CreateOrReplaceSpeciesCommand command = new(payload, id);

    Dictionary<RegionId, Number?> regionalNumbers = new()
    {
      [RegionId.NewId(_context.WorldId)] = new Number(25),
      [RegionId.NewId(_context.WorldId)] = new Number(22)
    };
    _speciesManager.Setup(x => x.FindRegionalNumbersAsync(payload.RegionalNumbers, "RegionalNumbers", _cancellationToken)).ReturnsAsync(regionalNumbers);

    SpeciesModel model = new();
    _speciesQuerier.Setup(x => x.ReadAsync(It.IsAny<PokemonSpecies>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceSpeciesResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Species);

    if (id.HasValue)
    {
      SpeciesId speciesId = new(_context.WorldId, id.Value);
      _speciesRepository.Verify(x => x.LoadAsync(speciesId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateSpecies, _cancellationToken), Times.Once());
    _speciesQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<PokemonSpecies>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<PokemonSpecies>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing species.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    Number number = new(25);
    PokemonCategory category = PokemonCategory.Standard;
    PokemonSpecies species = new SpeciesBuilder(_faker).WithWorld(_context.World).WithNumber(number).WithCategory(category).ClearChanges().Build();
    _speciesRepository.Setup(x => x.LoadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = number.Value,
      Category = category,
      Key = "pikachu",
      Name = "Pikachu",
      BaseFriendship = 70,
      CatchRate = 190,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 10,
      EggGroups = new EggGroupsModel(EggGroup.Field, EggGroup.Fairy),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact."
    };
    payload.RegionalNumbers.Add(new RegionalNumberPayload("kanto", 25));
    payload.RegionalNumbers.Add(new RegionalNumberPayload(Guid.NewGuid().ToString(), 22));
    CreateOrReplaceSpeciesCommand command = new(payload, species.EntityId);

    Dictionary<RegionId, Number?> regionalNumbers = new()
    {
      [RegionId.NewId(_context.WorldId)] = new Number(25),
      [RegionId.NewId(_context.WorldId)] = new Number(22)
    };
    _speciesManager.Setup(x => x.FindRegionalNumbersAsync(payload.RegionalNumbers, "RegionalNumbers", _cancellationToken)).ReturnsAsync(regionalNumbers);

    SpeciesModel model = new();
    _speciesQuerier.Setup(x => x.ReadAsync(species, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceSpeciesResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Species);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, species, _cancellationToken), Times.Once());
    _speciesQuerier.Verify(x => x.EnsureUnicityAsync(species, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(species, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the category has changed.")]
  public async Task Given_CategoryChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    PokemonSpecies species = new SpeciesBuilder(_faker).WithWorld(_context.World).WithCategory(PokemonCategory.Baby).ClearChanges().Build();
    _speciesRepository.Setup(x => x.LoadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = species.Number.Value,
      Category = PokemonCategory.Standard,
      Key = "pikachu",
      Name = "Pikachu",
      BaseFriendship = 70,
      CatchRate = 190,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 10,
      EggGroups = new EggGroupsModel(EggGroup.Field, EggGroup.Fairy),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact."
    };
    CreateOrReplaceSpeciesCommand command = new(payload, species.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<PokemonCategory>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(species.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Species", exception.EntityKind);
    Assert.Equal(species.EntityId, exception.EntityId);
    Assert.Equal(species.Category, exception.ExpectedValue);
    Assert.Equal(payload.Category, exception.AttemptedValue);
    Assert.Equal("Category", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the number has changed.")]
  public async Task Given_NumberChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    PokemonSpecies species = new SpeciesBuilder(_faker).WithWorld(_context.World).WithNumber(new Number(172)).ClearChanges().Build();
    _speciesRepository.Setup(x => x.LoadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 25,
      Category = species.Category,
      Key = "pikachu",
      Name = "Pikachu",
      BaseFriendship = 70,
      CatchRate = 190,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 10,
      EggGroups = new EggGroupsModel(EggGroup.Field, EggGroup.Fairy),
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "Iconic Pokémon: Pikachu has many exclusives (Z-Moves, events), a unique starter role, varied cries/designs, and major cultural and scientific impact."
    };
    CreateOrReplaceSpeciesCommand command = new(payload, species.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<int>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(species.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Species", exception.EntityKind);
    Assert.Equal(species.EntityId, exception.EntityId);
    Assert.Equal(species.Number.Value, exception.ExpectedValue);
    Assert.Equal(payload.Number, exception.AttemptedValue);
    Assert.Equal("Number", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Category = (PokemonCategory)(-1),
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      GrowthRate = (GrowthRate)99,
      EggGroups = new EggGroupsModel(EggGroup.Field, (EggGroup)(-1)),
      Url = "invalid"
    };
    payload.RegionalNumbers.Add(new RegionalNumberPayload());
    payload.RegionalNumbers.Add(new RegionalNumberPayload("kanto", Number.MaximumValue + 1));
    CreateOrReplaceSpeciesCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(12, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Number");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Category");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "CatchRate");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "GrowthRate");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "GreaterThanValidator" && e.PropertyName == "EggCycles");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "EggGroups.Secondary");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "RegionalNumbers[0].Region");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "RegionalNumbers[0].Number");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "RegionalNumbers[1].Number");
  }
}
