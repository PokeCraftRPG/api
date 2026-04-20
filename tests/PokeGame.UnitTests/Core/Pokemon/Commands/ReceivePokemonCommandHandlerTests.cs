using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class ReceivePokemonCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IItemManager> _itemManager = new();
  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IPokemonQuerier> _pokemonQuerier = new();
  private readonly Mock<IPokemonRepository> _pokemonRepository = new();
  private readonly Mock<IRosterRepository> _rosterRepository = new();
  private readonly Mock<ITrainerManager> _trainerManager = new();

  private readonly TestContext _context;
  private readonly ReceivePokemonCommandHandler _handler;

  private readonly World _world;
  private readonly Specimen _specimen;
  private readonly Trainer _trainer;
  private readonly Item _pokeBall;

  public ReceivePokemonCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(
      _context,
      _itemManager.Object,
      _permissionService.Object,
      _pokemonQuerier.Object,
      _pokemonRepository.Object,
      _rosterRepository.Object,
      _trainerManager.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;
    _specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
    _trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _pokeBall = ItemBuilder.PokeBall(_faker, _world);
  }

  [Fact(DisplayName = "It should receive a wild Pokémon.")]
  public async Task Given_Wild_When_HandleAsync_Then_Caught()
  {
    ReceivePokemonPayload payload = new(_trainer.Key.Value, _pokeBall.Key.Value, "Mt. Coronet");
    ReceivePokemonCommand command = new(_specimen.EntityId, payload);

    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);
    _trainerManager.Setup(x => x.FindAsync(payload.Trainer, "Trainer", _cancellationToken)).ReturnsAsync(_trainer);
    _itemManager.Setup(x => x.FindAsync(payload.PokeBall, "PokeBall", _cancellationToken)).ReturnsAsync(_pokeBall);

    Roster roster = new(_trainer);
    _rosterRepository.Setup(x => x.LoadAsync(_trainer, _cancellationToken)).ReturnsAsync(roster);

    PokemonModel model = new();
    _pokemonQuerier.Setup(x => x.ReadAsync(_specimen, _cancellationToken)).ReturnsAsync(model);

    PokemonModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Receive, _specimen, _cancellationToken), Times.Once());
    _pokemonRepository.Verify(x => x.SaveAsync(_specimen, _cancellationToken), Times.Once());
    _rosterRepository.Verify(x => x.SaveAsync(roster, _cancellationToken), Times.Once());

    Assert.Equal(_trainer.Id, _specimen.OriginalTrainerId);
    Assert.NotNull(_specimen.Ownership);
    Assert.Equal(new PokemonSlot(0), _specimen.Slot);
    Assert.Equal(_specimen.Id, Assert.Single(roster.GetParty()));
  }

  [Fact(DisplayName = "It should return null when the Pokémon was not found.")]
  public async Task Given_NotFound_When_HandleAsync_Then_NullReturned()
  {
    ReceivePokemonPayload payload = new(_trainer.Key.Value, _pokeBall.Key.Value, "Mt. Coronet");
    ReceivePokemonCommand command = new(Guid.NewGuid(), payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    ReceivePokemonPayload payload = new()
    {
      Location = _faker.Random.String(Location.MaximumLength + 1, 'a', 'z')
    };
    ReceivePokemonCommand command = new(Guid.NewGuid(), payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(3, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Trainer");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "PokeBall");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Location");
  }
}
