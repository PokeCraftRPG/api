using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Forms;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class CreatePokemonCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IFormManager> _formManager = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IPokemonQuerier> _pokemonQuerier = new();
  private readonly Mock<IPokemonRepository> _pokemonRepository = new();
  private readonly Mock<ISpeciesRepository> _speciesRepository = new();
  private readonly Mock<IStorageService> _storageService = new();
  private readonly Mock<IVarietyRepository> _varietyRepository = new();

  private readonly TestContext _context;
  private readonly CreatePokemonCommandHandler _handler;

  private readonly World _world;
  private readonly SpeciesAggregate _species;
  private readonly Variety _variety;
  private readonly Form _form;

  public CreatePokemonCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(
      _context,
      _formManager.Object,
      _permissionService.Object,
      _pokemonQuerier.Object,
      _pokemonRepository.Object,
      _speciesRepository.Object,
      _storageService.Object,
      _varietyRepository.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;
    _species = SpeciesBuilder.Tepig(_faker, _world);
    _variety = VarietyBuilder.Tepig(_faker, _world, _species);
    _form = FormBuilder.Tepig(_faker, _world, _variety);
    _formManager.Setup(x => x.FindAsync(_form.Key.Value, "Form", _cancellationToken)).ReturnsAsync(_form);
  }

  [Theory(DisplayName = "It should create a new Pokémon.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_Created(bool withId)
  {
    _speciesRepository.Setup(x => x.LoadAsync(_species.Id, _cancellationToken)).ReturnsAsync(_species);
    _varietyRepository.Setup(x => x.LoadAsync(_variety.Id, _cancellationToken)).ReturnsAsync(_variety);

    CreatePokemonPayload payload = new()
    {
      Id = withId ? Guid.NewGuid() : null,
      Form = "tepig",
      Key = withId ? "briquet" : null,
      Name = "Briquet"
    };

    PokemonModel model = new();
    _pokemonQuerier.Setup(x => x.ReadAsync(It.IsAny<Specimen>(), _cancellationToken)).ReturnsAsync(model);

    CreatePokemonCommand command = new(payload);
    PokemonModel result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.CreatePokemon, _cancellationToken), Times.Once());
    _pokemonQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Specimen>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Specimen>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw InvalidOperationException when the species was not loaded.")]
  public async Task Given_SpeciesNotLoaded_When_HandleAsync_Then_InvalidOperationException()
  {
    _varietyRepository.Setup(x => x.LoadAsync(_variety.Id, _cancellationToken)).ReturnsAsync(_variety);

    CreatePokemonPayload payload = new()
    {
      Form = "tepig",
      Gender = _faker.PickRandom<PokemonGender>()
    };
    CreatePokemonCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal($"The species 'Id={_species.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "It should throw InvalidOperationException when the variety was not loaded.")]
  public async Task Given_VarietyNotLoaded_When_HandleAsync_Then_InvalidOperationException()
  {
    CreatePokemonPayload payload = new()
    {
      Form = "tepig",
      Gender = _faker.PickRandom<PokemonGender>()
    };
    CreatePokemonCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal($"The variety 'Id={_variety.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when the ID is conflicting.")]
  public async Task Given_IdConflict_When_HandleAsync_Then_PropertyConflictException()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Is(_species, _variety, _form).Build();
    _pokemonRepository.Setup(x => x.LoadAsync(specimen.Id, _cancellationToken)).ReturnsAsync(specimen);

    CreatePokemonPayload payload = new()
    {
      Id = specimen.EntityId,
      Form = "tepig"
    };
    CreatePokemonCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<PropertyConflictException<Guid>>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Specimen", exception.EntityKind);
    Assert.Equal(specimen.EntityId, exception.EntityId);
    Assert.Equal(specimen.EntityId, exception.ConflictId);
    Assert.Equal(specimen.EntityId, exception.AttemptedValue);
    Assert.Equal("Id", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    CreatePokemonPayload payload = new()
    {
      Key = "in--val!d",
      Name = _faker.Random.String(Name.MaximumLength + 1, 'a', 'z'),
      Gender = (PokemonGender)999,
      Sprite = "invalid",
      Url = "invalid"
    };
    CreatePokemonCommand command = new(payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(6, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Form");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "EnumValidator" && e.PropertyName == "Gender.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Sprite");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url");
  }
}
