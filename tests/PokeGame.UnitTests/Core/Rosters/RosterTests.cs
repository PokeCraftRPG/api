using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;
using PokeGame.Core.Rosters.Events;
using PokeGame.Core.Trainers;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Rosters;

[Trait(Traits.Category, Categories.Unit)]
public class RosterTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly Trainer _trainer;
  private readonly Item _pokeBall;
  private readonly Location _location = new("Pallet Town");
  private readonly Roster _roster;
  private readonly Specimen _specimen;

  public RosterTests()
  {
    _world = new WorldBuilder(_faker).Build();
    _trainer = new TrainerBuilder(_faker).WithWorld(_world).Build();
    _pokeBall = ItemBuilder.PokeBall(_faker, _world);
    _roster = new Roster(_trainer);
    _specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
  }

  [Fact(DisplayName = "Add: it should add the Pokémon to the roster.")]
  public void Given_Specimen_When_Add_Then_Added()
  {
    _specimen.Catch(_trainer, _pokeBall, _location, _world.OwnerId);

    _roster.Add(_specimen, _world.OwnerId);
    Assert.Equal(new PokemonSlot(0), _specimen.Slot);
    Assert.Equal([_specimen.Id], _roster.GetParty());
    Assert.True(_roster.HasChanges);
    Assert.Contains(_roster.Changes, change => change is RosterPokemonMoved moved && moved.ActorId == _world.OwnerId.ActorId);

    _roster.ClearChanges();
    _roster.Add(_specimen, _world.OwnerId);
    Assert.False(_roster.HasChanges);
    Assert.Empty(_roster.Changes);
  }

  [Theory(DisplayName = "Add: it should throw ArgumentException when the ownership is not valid.")]
  [InlineData(false)]
  [InlineData(true)]
  public void Given_InvalidOwnership_When_Add_Then_ArgumentException(bool withOwnership)
  {
    Trainer? trainer = withOwnership ? new TrainerBuilder().WithWorld(_world).Build() : null;
    if (trainer is not null)
    {
      _specimen.Receive(trainer, _pokeBall, _location, _world.OwnerId);
    }

    var exception = Assert.Throws<ArgumentException>(() => _roster.Add(_specimen, _world.OwnerId));
    Assert.Equal("specimen", exception.ParamName);

    string message = trainer is null
      ? $"The Pokémon current trainer 'Id=<null>' must be '{_trainer.Id}'."
      : $"The Pokémon current trainer 'Id={trainer.Id}' must be '{_trainer.Id}'.";
    Assert.StartsWith(message, exception.Message);
  }

  [Fact(DisplayName = "Add: it should throw RosterIsFullException when the roster is full.")]
  public void Given_RosterIsFull_When_Add_Then_RosterIsFullException()
  {
    int limit = PokemonSlot.PartySize + (PokemonSlot.BoxCount * PokemonSlot.BoxSize);
    for (int i = 0; i < limit; i++)
    {
      Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Build();
      specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
      _roster.Add(specimen, _world.OwnerId);
    }

    _specimen.Receive(_trainer, _pokeBall, _location, _world.OwnerId);
    var exception = Assert.Throws<RosterIsFullException>(() => _roster.Add(_specimen, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
  }

  [Fact(DisplayName = "Add: it should throw WorldMismatchException when the Pokémon is not in the same world.")]
  public void Given_WorldMismatch_When_Add_Then_WorldMismatchException()
  {
    Specimen specimen = new SpecimenBuilder().Build();

    var exception = Assert.Throws<WorldMismatchException>(() => _roster.Add(specimen, _world.OwnerId));
    Assert.Equal(_roster.Id.GetEntity(), exception.Expected);
    Assert.Equal(specimen.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("specimen", exception.ParamName);
  }
}
