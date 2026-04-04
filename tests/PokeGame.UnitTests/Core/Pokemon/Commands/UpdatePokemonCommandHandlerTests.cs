using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Storages;

namespace PokeGame.Core.Pokemon.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class UpdatePokemonCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IPokemonQuerier> _pokemonQuerier = new();
  private readonly Mock<IPokemonRepository> _pokemonRepository = new();
  private readonly Mock<IStorageService> _storageService = new();

  private readonly TestContext _context;
  private readonly UpdatePokemonCommandHandler _handler;

  private readonly Specimen _specimen;

  public UpdatePokemonCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _pokemonQuerier.Object, _pokemonRepository.Object, _storageService.Object);

    _specimen = new SpecimenBuilder(_faker).WithWorld(_context.World).Build();
  }

  [Fact(DisplayName = "It should return null when the Pokémon does not exist.")]
  public async Task Given_DoesNotExist_When_HandleAsync_Then_NullReturned()
  {
    UpdatePokemonPayload payload = new()
    {
      Key = "briquet",
      Name = new Optional<string>("Briquet")
    };

    UpdatePokemonCommand command = new(_specimen.EntityId, payload);
    PokemonModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.Null(result);
  }

  [Fact(DisplayName = "It should update an existing Pokémon.")]
  public async Task Given_DoesExist_When_HandleAsync_Then_Updated()
  {
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);

    UpdatePokemonPayload payload = new()
    {
      Key = "briquet",
      Name = new Optional<string>("Briquet")
    };

    PokemonModel model = new();
    _pokemonQuerier.Setup(x => x.ReadAsync(_specimen, _cancellationToken)).ReturnsAsync(model);

    UpdatePokemonCommand command = new(_specimen.EntityId, payload);
    PokemonModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Update, _specimen, _cancellationToken), Times.Once());
    _pokemonQuerier.Verify(x => x.EnsureUnicityAsync(It.IsAny<Specimen>(), _cancellationToken), Times.Once());
    _storageService.Verify(x => x.ExecuteWithQuotaAsync(It.IsAny<Specimen>(), It.IsAny<Func<Task>>(), _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    UpdatePokemonPayload payload = new()
    {
      Key = "in--val!d",
      Name = new Optional<string>(_faker.Random.String(Name.MaximumLength + 1, 'a', 'z')),
      Sprite = new Optional<string>("invalid"),
      Url = new Optional<string>("invalid")
    };
    UpdatePokemonCommand command = new(Guid.Empty, payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(4, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "SlugValidator" && e.PropertyName == "Key");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Name.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Sprite.Value");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "UrlValidator" && e.PropertyName == "Url.Value");
  }
}
