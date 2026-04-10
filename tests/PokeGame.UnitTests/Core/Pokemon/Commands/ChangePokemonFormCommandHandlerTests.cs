using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Forms;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Storages;
using PokeGame.Core.Varieties;

namespace PokeGame.Core.Pokemon.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class ChangePokemonFormCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IFormManager> _formManager = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IPokemonQuerier> _pokemonQuerier = new();
  private readonly Mock<IPokemonRepository> _pokemonRepository = new();
  private readonly Mock<IStorageService> _storageService = new();
  private readonly Mock<IVarietyRepository> _varietyRepository = new();

  private readonly TestContext _context;
  private readonly ChangePokemonFormCommandHandler _handler;

  public ChangePokemonFormCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(
      _context,
      _formManager.Object,
      _permissionService.Object,
      _pokemonQuerier.Object,
      _pokemonRepository.Object,
      _storageService.Object,
      _varietyRepository.Object);
  }

  [Fact(DisplayName = "It should change the Pokémon form.")]
  public async Task Given_CanChangeForm_When_HandleAsync_Then_FormChanged()
  {
    SpeciesAggregate species = SpeciesBuilder.Darmanitan(_faker, _context.World);
    Variety variety = VarietyBuilder.Darmanitan(_faker, _context.World, species);
    Form form = FormBuilder.Darmanitan(_faker, _context.World, variety);
    Form zen = FormBuilder.DarmanitanZen(_faker, _context.World, variety);

    Specimen pokemon = new SpecimenBuilder(_faker).WithWorld(_context.World).Is(species, variety, form).Build();
    _pokemonRepository.Setup(x => x.LoadAsync(pokemon.Id, _cancellationToken)).ReturnsAsync(pokemon);

    _varietyRepository.Setup(x => x.LoadAsync(variety.Id, _cancellationToken)).ReturnsAsync(variety);
    _formManager.Setup(x => x.FindAsync(zen.Id.Value, "Form", _cancellationToken)).ReturnsAsync(zen);

    PokemonModel model = new();
    _pokemonQuerier.Setup(x => x.ReadAsync(pokemon, _cancellationToken)).ReturnsAsync(model);

    ChangePokemonFormPayload payload = new(zen.Id.Value);
    ChangePokemonFormCommand command = new(pokemon.EntityId, payload);

    PokemonModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    Assert.Equal(zen.Id, pokemon.FormId);
    Assert.Equal(zen.BaseStatistics, pokemon.BaseStatistics);

    _permissionService.Verify(x => x.CheckAsync(Actions.ChangeForm, pokemon, _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(pokemon, It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should return null when the Pokémon was not found.")]
  public async Task Given_NotFound_When_HandleAsync_Then_NullReturned()
  {
    ChangePokemonFormPayload payload = new("a-form");
    ChangePokemonFormCommand command = new(Guid.NewGuid(), payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw InvalidOperationException when the variety was not loaded.")]
  public async Task Given_VarietyNotLoaded_When_HandleAsync_Then_InvalidOperationException()
  {
    SpeciesAggregate species = SpeciesBuilder.Groudon(_faker, _context.World);
    Variety variety = VarietyBuilder.Groudon(_faker, _context.World, species);
    Form form = FormBuilder.Groudon(_faker, _context.World, variety);
    Form primal = FormBuilder.GroudonPrimal(_faker, _context.World, variety);

    Specimen pokemon = new SpecimenBuilder(_faker).WithWorld(_context.World).Is(species, variety, form).Build();
    _pokemonRepository.Setup(x => x.LoadAsync(pokemon.Id, _cancellationToken)).ReturnsAsync(pokemon);

    ChangePokemonFormPayload payload = new(primal.Id.Value);
    ChangePokemonFormCommand command = new(pokemon.EntityId, payload);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal($"The variety 'Id={variety.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "It should throw PokemonCannotChangeFormException when the variety cannot change form.")]
  public async Task Given_CannotChangeForm_When_HandleAsync_Then_PokemonCannotChangeFormException()
  {
    SpeciesAggregate species = SpeciesBuilder.Groudon(_faker, _context.World);
    Variety variety = VarietyBuilder.Groudon(_faker, _context.World, species);
    Form form = FormBuilder.Groudon(_faker, _context.World, variety);
    Form primal = FormBuilder.GroudonPrimal(_faker, _context.World, variety);

    Specimen pokemon = new SpecimenBuilder(_faker).WithWorld(_context.World).Is(species, variety, form).Build();
    _pokemonRepository.Setup(x => x.LoadAsync(pokemon.Id, _cancellationToken)).ReturnsAsync(pokemon);

    _varietyRepository.Setup(x => x.LoadAsync(variety.Id, _cancellationToken)).ReturnsAsync(variety);
    _formManager.Setup(x => x.FindAsync(primal.Id.Value, "Form", _cancellationToken)).ReturnsAsync(primal);

    ChangePokemonFormPayload payload = new(primal.Id.Value);
    ChangePokemonFormCommand command = new(pokemon.EntityId, payload);

    var exception = await Assert.ThrowsAsync<PokemonCannotChangeFormException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(pokemon.WorldId.ToGuid(), exception.WorldId);
    Assert.Equal(variety.EntityId, exception.VarietyId);
    Assert.Equal(pokemon.EntityId, exception.PokemonId);
  }

  [Fact(DisplayName = "It should throw ValidationException when they payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    ChangePokemonFormPayload payload = new();
    ChangePokemonFormCommand command = new(Guid.NewGuid(), payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Single(exception.Errors);
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Form");
  }
}
