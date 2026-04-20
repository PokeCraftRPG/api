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
public class DepositPokemonCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IPokemonQuerier> _pokemonQuerier = new();
  private readonly Mock<IPokemonRepository> _pokemonRepository = new();
  private readonly Mock<IRosterRepository> _rosterRepository = new();

  private readonly TestContext _context;
  private readonly DepositPokemonCommandHandler _handler;

  private readonly World _world;
  private readonly Specimen _specimen;
  private readonly Trainer _trainer;
  private readonly Item _pokeBall;

  public DepositPokemonCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(_context, _permissionService.Object, _pokemonQuerier.Object, _pokemonRepository.Object, _rosterRepository.Object);

    Assert.NotNull(_context.World);
    _world = _context.World;
    _specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
    _trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _pokeBall = ItemBuilder.PokeBall(_faker, _world);
  }

  [Fact(DisplayName = "It should deposit the Pokémon.")]
  public async Task Given_Pokemon_When_HandleAsync_Then_Depositd()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
    specimen.Catch(_trainer, _pokeBall, new Location("Viridian Forest"), _world.OwnerId);
    _pokemonRepository.Setup(x => x.LoadAsync(
      It.Is<IEnumerable<PokemonId>>(y => y.SequenceEqual(new PokemonId[] { specimen.Id })),
      _cancellationToken)).ReturnsAsync([specimen]);

    _specimen.Receive(_trainer, _pokeBall, new Location("Mt. Coronet"), _world.OwnerId);
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);

    Roster roster = new(_trainer);
    roster.Add(specimen, _world.OwnerId);
    roster.Add(_specimen, _world.OwnerId);
    _rosterRepository.Setup(x => x.LoadAsync(roster.Id, _cancellationToken)).ReturnsAsync(roster);

    PokemonModel model = new();
    _pokemonQuerier.Setup(x => x.ReadAsync(_specimen, _cancellationToken)).ReturnsAsync(model);

    DepositPokemonCommand command = new(_specimen.EntityId);
    PokemonModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Deposit, _specimen, _cancellationToken), Times.Once());
    _pokemonRepository.Verify(x => x.SaveAsync(
      It.Is<IEnumerable<Specimen>>(y => y.SequenceEqual(new Specimen[] { specimen, _specimen })),
      _cancellationToken), Times.Once());

    Assert.Equal(_trainer.Id, _specimen.OriginalTrainerId);
    Assert.NotNull(_specimen.Ownership);
    Assert.Equal(_trainer.Id, _specimen.Ownership.TrainerId);
    Assert.Equal(new PokemonSlot(0, 0), _specimen.Slot);

    _rosterRepository.Verify(x => x.SaveAsync(roster, _cancellationToken), Times.Once());
    Assert.Equal([specimen.Id], roster.GetParty());
  }

  [Fact(DisplayName = "It should return null when the Pokémon was not found.")]
  public async Task Given_NotFound_When_HandleAsync_Then_NullReturned()
  {
    DepositPokemonCommand command = new(Guid.NewGuid());
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when the Pokémon is wild.")]
  public async Task Given_Wild_When_HandleAsync_Then_PokemonHasNoOwnerException()
  {
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);

    DepositPokemonCommand command = new(_specimen.EntityId);
    var exception = await Assert.ThrowsAsync<PokemonHasNoOwnerException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_specimen.EntityId, exception.PokemonId);
  }
}
