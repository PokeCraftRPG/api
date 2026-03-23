using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Species.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreateOrReplaceSpeciesCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<ISpeciesQuerier> _speciesQuerier = new();
  private readonly Mock<ISpeciesRepository> _speciesRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly CreateOrReplaceSpeciesCommandHandler _handler;

  public CreateOrReplaceSpeciesCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _speciesQuerier.Object, _speciesRepository.Object, _storageService.Object);
  }

  [Theory(DisplayName = "It should create a new species.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Key = "pikachu",
      Name = "Pikachu",
      Category = PokemonCategory.Standard
    };
    CreateOrReplaceSpeciesCommand command = new(payload, id);

    SpeciesModel model = new();
    _speciesQuerier.Setup(x => x.ReadAsync(It.IsAny<SpeciesAggregate>(), _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceSpeciesResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.True(result.Created);
    Assert.Same(model, result.Species);

    if (id.HasValue)
    {
      SpeciesId speciesId = new(_context.WorldId, id.Value);
      _speciesRepository.Verify(x => x.LoadAsync(speciesId, _cancellationToken), Times.Once());
    }
    _permissionService.Verify(x => x.CheckAsync(Actions.CreateSpecies, _cancellationToken), Times.Once());
    _speciesQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<SpeciesAggregate>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<SpeciesAggregate>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should replace the existing species.")]
  public async Task Given_Exists_When_HandleAsync_Then_Replaced()
  {
    SpeciesAggregate species = new SpeciesBuilder(_faker).WithWorld(_context.World).ClearChanges().Build();
    _speciesRepository.Setup(x => x.LoadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Key = "pikachu",
      Name = "Pikachu",
      Category = species.Category
    };
    CreateOrReplaceSpeciesCommand command = new(payload, species.EntityId);

    SpeciesModel model = new();
    _speciesQuerier.Setup(x => x.ReadAsync(species, _cancellationToken)).ReturnsAsync(model);

    CreateOrReplaceSpeciesResult result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.False(result.Created);
    Assert.Same(model, result.Species);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, species, _cancellationToken), Times.Once());
    _speciesQuerier.Verify(x => x.EnsureUnicityAsync(species, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(species, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when category changes on replace.")]
  public async Task Given_CategoryChange_When_HandleAsync_Then_ImmutablePropertyException()
  {
    SpeciesAggregate species = new SpeciesBuilder(_faker).WithWorld(_context.World).WithCategory(PokemonCategory.Standard).ClearChanges().Build();
    _speciesRepository.Setup(x => x.LoadAsync(species.Id, _cancellationToken)).ReturnsAsync(species);

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Key = "pikachu",
      Category = PokemonCategory.Legendary
    };
    CreateOrReplaceSpeciesCommand command = new(payload, species.EntityId);

    var exception = await Assert.ThrowsAsync<ImmutablePropertyException<PokemonCategory>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(PokemonCategory.Standard, exception.ExpectedValue);
    Assert.Equal(PokemonCategory.Legendary, exception.AttemptedValue);
    Assert.Equal(nameof(CreateOrReplaceSpeciesPayload.Category), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Category = (PokemonCategory)999
    };
    CreateOrReplaceSpeciesCommand command = new(payload, Guid.Empty);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(3, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Category");
  }
}
