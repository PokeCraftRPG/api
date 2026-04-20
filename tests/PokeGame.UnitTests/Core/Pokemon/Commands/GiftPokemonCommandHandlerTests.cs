using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon.Commands;

[Trait(Traits.Category, Categories.Unit)]
public class GiftPokemonCommandHandlerTests
{
  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IPermissionService> _permissionService = new();
  private readonly Mock<IPokemonQuerier> _pokemonQuerier = new();
  private readonly Mock<IPokemonRepository> _pokemonRepository = new();
  private readonly Mock<IRosterRepository> _rosterRepository = new();
  private readonly Mock<ITrainerManager> _trainerManager = new();

  private readonly TestContext _context;
  private readonly GiftPokemonCommandHandler _handler;

  private readonly World _world;
  private readonly Specimen _specimen;
  private readonly Trainer _trainer;
  private readonly Item _pokeBall;

  public GiftPokemonCommandHandlerTests()
  {
    _context = new(_faker);
    _handler = new(
      _context,
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

  [Fact(DisplayName = "It should gift a boxed Pokémon to another trainer.")]
  public async Task Given_Boxed_When_HandleAsync_Then_Gifted()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _specimen.Receive(trainer, _pokeBall, new Location("Pallet Town"), _world.OwnerId);
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);

    GiftPokemonPayload payload = new(_trainer.Key.Value, "Viridian Forest");
    _trainerManager.Setup(x => x.FindAsync(payload.Trainer, nameof(payload.Trainer), _cancellationToken)).ReturnsAsync(_trainer);

    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Caught(trainer, _pokeBall, new Location("Mt. Coronet")).Build();

    Roster sourceRoster = new(trainer);
    sourceRoster.Add(_specimen, _world.OwnerId);
    sourceRoster.Add(specimen, _world.OwnerId);
    sourceRoster.Deposit(_specimen, new PokemonParty([_specimen, specimen]), _world.OwnerId);
    _rosterRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(sourceRoster);

    Roster destinationRoster = new(_trainer);
    _rosterRepository.Setup(x => x.LoadAsync(_trainer, _cancellationToken)).ReturnsAsync(destinationRoster);

    PokemonModel model = new();
    _pokemonQuerier.Setup(x => x.ReadAsync(_specimen, _cancellationToken)).ReturnsAsync(model);

    GiftPokemonCommand command = new(_specimen.EntityId, payload);
    PokemonModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    Assert.Equal(trainer.Id, _specimen.OriginalTrainerId);

    Assert.NotNull(_specimen.Ownership);
    Assert.Equal(OwnershipKind.Gifted, _specimen.Ownership.Kind);
    Assert.Equal(_trainer.Id, _specimen.Ownership.TrainerId);
    Assert.Equal(_pokeBall.Id, _specimen.Ownership.PokeBallId);
    Assert.Equal(_specimen.Level, _specimen.Ownership.Level.Value);
    Assert.Equal(payload.Location, _specimen.Ownership.Location.Value);
    Assert.Equal(DateTime.Now, _specimen.Ownership.MetOn, TimeSpan.FromSeconds(10));
    Assert.Equal(new PokemonSlot(0), _specimen.Slot);

    Assert.Equal(new PokemonSlot(0), specimen.Slot);

    Assert.Contains(_specimen.Changes, change => change is PokemonGifted moved && moved.ActorId == _world.OwnerId.ActorId);
    Assert.Contains(_specimen.Changes, change => change is PokemonMoved moved && moved.ActorId == _world.OwnerId.ActorId);
    Assert.Contains(sourceRoster.Changes, change => change is RosterPokemonRemoved removed && removed.PokemonId == _specimen.Id && removed.ActorId == _world.OwnerId.ActorId);
    Assert.Contains(destinationRoster.Changes, change => change is RosterPokemonMoved moved && moved.PokemonId == _specimen.Id && moved.ActorId == _world.OwnerId.ActorId);

    _permissionService.Verify(x => x.CheckAsync(Actions.Gift, _specimen, _cancellationToken), Times.Once());
    _pokemonRepository.Verify(x => x.LoadAsync(It.IsAny<IEnumerable<PokemonId>>(), _cancellationToken), Times.Never());
    _pokemonRepository.Verify(x => x.SaveAsync(_specimen, _cancellationToken), Times.Once());
    _rosterRepository.Verify(x => x.SaveAsync(
      It.Is<IEnumerable<Roster>>(rosters => rosters.SequenceEqual(new Roster[] { sourceRoster, destinationRoster })),
      _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should gift a party Pokémon to another trainer.")]
  public async Task Given_Party_When_HandleAsync_Then_Gifted()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _specimen.Receive(trainer, _pokeBall, new Location("Pallet Town"), _world.OwnerId);
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);

    GiftPokemonPayload payload = new(_trainer.Key.Value, "Viridian Forest");
    _trainerManager.Setup(x => x.FindAsync(payload.Trainer, nameof(payload.Trainer), _cancellationToken)).ReturnsAsync(_trainer);

    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Caught(trainer, _pokeBall, new Location("Mt. Coronet")).Build();
    _pokemonRepository.Setup(x => x.LoadAsync(It.Is<IEnumerable<PokemonId>>(ids => ids.Single() == specimen.Id), _cancellationToken)).ReturnsAsync([specimen]);

    Roster sourceRoster = new(trainer);
    sourceRoster.Add(_specimen, _world.OwnerId);
    sourceRoster.Add(specimen, _world.OwnerId);
    _rosterRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(sourceRoster);

    Roster destinationRoster = new(_trainer);
    _rosterRepository.Setup(x => x.LoadAsync(_trainer, _cancellationToken)).ReturnsAsync(destinationRoster);

    PokemonModel model = new();
    _pokemonQuerier.Setup(x => x.ReadAsync(_specimen, _cancellationToken)).ReturnsAsync(model);

    GiftPokemonCommand command = new(_specimen.EntityId, payload);
    PokemonModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    Assert.Equal(trainer.Id, _specimen.OriginalTrainerId);

    Assert.NotNull(_specimen.Ownership);
    Assert.Equal(OwnershipKind.Gifted, _specimen.Ownership.Kind);
    Assert.Equal(_trainer.Id, _specimen.Ownership.TrainerId);
    Assert.Equal(_pokeBall.Id, _specimen.Ownership.PokeBallId);
    Assert.Equal(_specimen.Level, _specimen.Ownership.Level.Value);
    Assert.Equal(payload.Location, _specimen.Ownership.Location.Value);
    Assert.Equal(DateTime.Now, _specimen.Ownership.MetOn, TimeSpan.FromSeconds(10));
    Assert.Equal(new PokemonSlot(0), _specimen.Slot);

    Assert.Equal(new PokemonSlot(0), specimen.Slot);

    Assert.Contains(_specimen.Changes, change => change is PokemonGifted moved && moved.ActorId == _world.OwnerId.ActorId);
    Assert.Contains(_specimen.Changes, change => change is PokemonMoved moved && moved.ActorId == _world.OwnerId.ActorId);
    Assert.Contains(sourceRoster.Changes, change => change is RosterPokemonRemoved removed && removed.PokemonId == _specimen.Id && removed.ActorId == _world.OwnerId.ActorId);
    Assert.Contains(destinationRoster.Changes, change => change is RosterPokemonMoved moved && moved.PokemonId == _specimen.Id && moved.ActorId == _world.OwnerId.ActorId);

    _permissionService.Verify(x => x.CheckAsync(Actions.Gift, _specimen, _cancellationToken), Times.Once());
    _pokemonRepository.Verify(x => x.SaveAsync(_specimen, _cancellationToken), Times.Once());
    _rosterRepository.Verify(x => x.SaveAsync(
      It.Is<IEnumerable<Roster>>(rosters => rosters.SequenceEqual(new Roster[] { sourceRoster, destinationRoster })),
      _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "It should not do anything when the source and destination trainers are the same.")]
  public async Task Given_SameTrainer_When_HandleAsync_Then_DoNothing()
  {
    _specimen.Receive(_trainer, _pokeBall, new Location("Pallet Town"), _world.OwnerId);
    _specimen.Move(new PokemonSlot(0), _world.OwnerId);
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);

    GiftPokemonPayload payload = new(_trainer.Key.Value, "Mt. Coronet");
    _trainerManager.Setup(x => x.FindAsync(payload.Trainer, nameof(payload.Trainer), _cancellationToken)).ReturnsAsync(_trainer);

    PokemonModel model = new();
    _pokemonQuerier.Setup(x => x.ReadAsync(_specimen, _cancellationToken)).ReturnsAsync(model);

    GiftPokemonCommand command = new(_specimen.EntityId, payload);
    PokemonModel? result = await _handler.HandleAsync(command, _cancellationToken);
    Assert.NotNull(result);
    Assert.Same(model, result);

    _permissionService.Verify(x => x.CheckAsync(Actions.Gift, _specimen, _cancellationToken), Times.Once());
    _pokemonRepository.Verify(x => x.LoadAsync(It.IsAny<IEnumerable<PokemonId>>(), _cancellationToken), Times.Never());
    _pokemonRepository.Verify(x => x.SaveAsync(It.IsAny<Specimen>(), _cancellationToken), Times.Never());
    _rosterRepository.Verify(x => x.LoadAsync(It.IsAny<Trainer>(), _cancellationToken), Times.Never());
    _rosterRepository.Verify(x => x.LoadAsync(It.IsAny<TrainerId>(), _cancellationToken), Times.Never());
    _rosterRepository.Verify(x => x.SaveAsync(It.IsAny<IEnumerable<Roster>>(), _cancellationToken), Times.Never());
  }

  [Fact(DisplayName = "It should return null when the Pokémon was not found.")]
  public async Task Given_NotFound_When_HandleAsync_Then_NullReturned()
  {
    GiftPokemonPayload payload = new(_trainer.Key.Value, "Mt. Coronet");
    GiftPokemonCommand command = new(Guid.NewGuid(), payload);
    Assert.Null(await _handler.HandleAsync(command, _cancellationToken));
  }

  [Fact(DisplayName = "It should throw InvalidPartyException when the gifting trainer does not have any other battle-ready Pokémon.")]
  public async Task Given_NoOtherMember_When_HandleAsync_Then_InvalidPartyException()
  {
    Trainer trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _specimen.Receive(trainer, _pokeBall, new Location("Pallet Town"), _world.OwnerId);
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);
    _pokemonRepository.Setup(x => x.LoadAsync(It.Is<IEnumerable<PokemonId>>(ids => !ids.Any()), _cancellationToken)).ReturnsAsync([]);

    Roster roster = new(trainer);
    roster.Add(_specimen, _world.OwnerId);
    _rosterRepository.Setup(x => x.LoadAsync(trainer.Id, _cancellationToken)).ReturnsAsync(roster);

    GiftPokemonPayload payload = new(_trainer.Key.Value, "Mt. Coronet");
    _trainerManager.Setup(x => x.FindAsync(payload.Trainer, nameof(payload.Trainer), _cancellationToken)).ReturnsAsync(_trainer);

    GiftPokemonCommand command = new(_specimen.EntityId, payload);
    var exception = await Assert.ThrowsAsync<InvalidPartyException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(trainer.EntityId, exception.TrainerId);
    Assert.Equal(_specimen.EntityId, Assert.Single(exception.MemberIds));
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when the Pokémon does not have a slot.")]
  public async Task Given_NoSlot_When_HandleAsync_Then_PokemonHasNoOwnerException()
  {
    _specimen.Receive(_trainer, _pokeBall, new Location("Pallet Town"), _world.OwnerId);
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);

    GiftPokemonPayload payload = new(_trainer.Key.Value, "Mt. Coronet");
    GiftPokemonCommand command = new(_specimen.EntityId, payload);

    var exception = await Assert.ThrowsAsync<PokemonHasNoOwnerException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_specimen.EntityId, exception.PokemonId);
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when the Pokémon is wild.")]
  public async Task Given_Wild_When_HandleAsync_Then_PokemonHasNoOwnerException()
  {
    _pokemonRepository.Setup(x => x.LoadAsync(_specimen.Id, _cancellationToken)).ReturnsAsync(_specimen);

    GiftPokemonPayload payload = new(_trainer.Key.Value, "Mt. Coronet");
    GiftPokemonCommand command = new(_specimen.EntityId, payload);

    var exception = await Assert.ThrowsAsync<PokemonHasNoOwnerException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_specimen.EntityId, exception.PokemonId);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is not valid.")]
  public async Task Given_InvalidPayload_When_HandleAsync_Then_ValidationException()
  {
    GiftPokemonPayload payload = new()
    {
      Location = _faker.Random.String(Location.MaximumLength + 1, 'a', 'z')
    };
    GiftPokemonCommand command = new(Guid.NewGuid(), payload);

    var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(async () => await _handler.HandleAsync(command, _cancellationToken));
    Assert.Equal(2, exception.Errors.Count());
    Assert.Contains(exception.Errors, e => e.ErrorCode == "NotEmptyValidator" && e.PropertyName == "Trainer");
    Assert.Contains(exception.Errors, e => e.ErrorCode == "MaximumLengthValidator" && e.PropertyName == "Location");
  }
}
