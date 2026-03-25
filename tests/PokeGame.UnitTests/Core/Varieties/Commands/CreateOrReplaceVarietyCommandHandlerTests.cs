using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceVarietyCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<ISpeciesManager> _speciesManager = new();
  private readonly Mock<IVarietyManager> _varietyManager = new();
  private readonly Mock<IVarietyQuerier> _varietyQuerier = new();
  private readonly Mock<IVarietyRepository> _varietyRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceVarietyCommandHandler _handler;

  public CreateOrReplaceVarietyCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _speciesManager.Object, _storageService.Object, _varietyManager.Object, _varietyQuerier.Object, _varietyRepository.Object);
  }

  [Theory(DisplayName = "It should create a new variety.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceVarietyPayload payload = new()
    {
      Species = "pikachu",
      IsDefault = true,
      Key = "pikachu",
      Name = "Pikachu",
      Genus = "Mouse",
      Description = "It has small electric sacs on both its cheeks. When in a tough spot, this Pokémon discharges electricity.",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "This is the default variety."
    };
    payload.Moves.Add(new VarietyMovePayload("thunder-shock", 1));
    payload.Moves.Add(new VarietyMovePayload(Guid.NewGuid().ToString(), 0));
    CreateOrReplaceVarietyCommand command = new(payload, id);

    SpeciesAggregate species = SpeciesBuilder.Pikachu(_faker, _context.World);
    _speciesManager.Setup(x => x.FindAsync(payload.Species, "Species", _cancellationToken)).ReturnsAsync(species);

    Dictionary<MoveId, int?> moves = new()
    {
      [MoveId.NewId(_context.WorldId)] = 1,
      [MoveId.NewId(_context.WorldId)] = 0
    };
    _varietyManager.Setup(x => x.FindMovesAsync(payload.Moves, "Moves", _cancellationToken)).ReturnsAsync(moves);

    VarietyModel model = new();
    _varietyQuerier.Setup(x => x.ReadAsync(It.IsAny<Variety>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceVarietyResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Variety);

    if (id.HasValue)
    {
      VarietyId varietyId = new(_context.WorldId, id.Value);
      _varietyRepository.Verify(x => x.LoadAsync(varietyId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateVariety, _cancellationToken), Times.Once());
    _varietyQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Variety>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Variety>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing variety.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    SpeciesAggregate species = SpeciesBuilder.Pikachu(_faker, _context.World);

    Variety variety = new VarietyBuilder(_faker).WithWorld(_context.World).WithSpecies(species).IsDefault().ClearChanges().Build();
    _varietyRepository.Setup(x => x.LoadAsync(variety.Id, _cancellationToken)).ReturnsAsync(variety);

    CreateOrReplaceVarietyPayload payload = new()
    {
      Species = "pikachu",
      IsDefault = true,
      Key = "pikachu",
      Name = "Pikachu",
      Genus = "Mouse",
      Description = "It has small electric sacs on both its cheeks. When in a tough spot, this Pokémon discharges electricity.",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "This is the default variety."
    };
    payload.Moves.Add(new VarietyMovePayload("thunder-shock", 1));
    payload.Moves.Add(new VarietyMovePayload(Guid.NewGuid().ToString(), 0));
    CreateOrReplaceVarietyCommand command = new(payload, variety.EntityId);

    _speciesManager.Setup(x => x.FindAsync(payload.Species, "Species", _cancellationToken)).ReturnsAsync(species);

    Dictionary<MoveId, int?> moves = new()
    {
      [MoveId.NewId(_context.WorldId)] = 1,
      [MoveId.NewId(_context.WorldId)] = 0
    };
    _varietyManager.Setup(x => x.FindMovesAsync(payload.Moves, "Moves", _cancellationToken)).ReturnsAsync(moves);

    VarietyModel model = new();
    _varietyQuerier.Setup(x => x.ReadAsync(variety, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceVarietyResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Variety);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, variety, _cancellationToken), Times.Once());
    _varietyQuerier.Verify(x => x.EnsureUnicityAsync(variety, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(variety, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when the species has changed.")]
  public async Task Given_SpeciesChanged_When_HandleAsync_Then_ImmutablePropertyException()
  {
    SpeciesAggregate eevee = SpeciesBuilder.Eevee(_faker, _context.World);
    Variety variety = new VarietyBuilder(_faker).WithWorld(_context.World).WithSpecies(eevee).IsDefault().ClearChanges().Build();
    _varietyRepository.Setup(x => x.LoadAsync(variety.Id, _cancellationToken)).ReturnsAsync(variety);

    CreateOrReplaceVarietyPayload payload = new()
    {
      Species = "pikachu",
      IsDefault = true,
      Key = "pikachu",
      Name = "Pikachu",
      Genus = "Mouse",
      Description = "It has small electric sacs on both its cheeks. When in a tough spot, this Pokémon discharges electricity.",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)",
      Notes = "This is the default variety."
    };
    CreateOrReplaceVarietyCommand command = new(payload, variety.EntityId);

    SpeciesAggregate pikachu = SpeciesBuilder.Pikachu(_faker, _context.World);
    _speciesManager.Setup(x => x.FindAsync(payload.Species, "Species", _cancellationToken)).ReturnsAsync(pikachu);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(variety.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal("Variety", exception.EntityKind);
    Assert.Equal(variety.EntityId, exception.EntityId);
    Assert.Equal(eevee.EntityId, exception.ExpectedValue);
    Assert.Equal(pikachu.EntityId, exception.AttemptedValue);
    Assert.Equal("Species", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceVarietyPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Genus = _faker.Random.String(Genus.MaximumLength + 1, 'a', 'z'),
      GenderRatio = -1,
      Url = "invalid"
    };
    payload.Moves.Add(new VarietyMovePayload());
    payload.Moves.Add(new VarietyMovePayload("thunder-shock", level: -1));
    payload.Moves.Add(new VarietyMovePayload("sweet-kiss", level: 101));
    CreateOrReplaceVarietyCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(10, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Species");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Genus");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "GenderRatio.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Moves[0].Move");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotNullValidator" && e.PropertyName == "Moves[0].Level");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Moves[1].Level.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "InclusiveBetweenValidator" && e.PropertyName == "Moves[2].Level.Value");
  }
}
