using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Evolutions;

[Trait(Traits.Category, Categories.Unit)]
public class EvolutionTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly Form _source;
  private readonly Form _target;
  private readonly Item _item;

  public EvolutionTests()
  {
    _world = new WorldBuilder(_faker).Build();
    _source = FormBuilder.Pikachu(_faker, _world);
    _target = FormBuilder.Raichu(_faker, _world);
    _item = ItemBuilder.ThunderStone(_faker, _world);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentException when the trigger is not Item and an item is provided.")]
  public void Given_NotItemTriggerWithItem_When_ctor_Then_ArgumentException()
  {
    EvolutionTrigger trigger = _faker.PickRandom(EvolutionTrigger.Level, EvolutionTrigger.Trade);
    var exception = Assert.Throws<ArgumentException>(() => new Evolution(_world, _source, _target, trigger, _item));
    Assert.Equal("item", exception.ParamName);
    Assert.StartsWith("The item should be null when the evolution is not triggered by an item.", exception.Message);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentNullException when the trigger is Item and no item was provided.")]
  public void Given_ItemTriggerWithoutItem_When_ctor_Then_ArgumentNullException()
  {
    var exception = Assert.Throws<ArgumentNullException>(() => new Evolution(_world, _source, _target, EvolutionTrigger.Item, item: null));
    Assert.Equal("item", exception.ParamName);
    Assert.StartsWith("The item should not be null when the evolution is triggered by an item.", exception.Message);
  }

  [Fact(DisplayName = "ctor: it should throw InvalidOperationException when the source and target varieties are the same.")]
  public void Given_SameVarieties_When_ctor_Then_InvalidOperationException()
  {
    Variety variety = VarietyBuilder.Raichu(_faker, _world);
    Form source = FormBuilder.Raichu(_faker, _world, variety);
    Form target = FormBuilder.RaichuAlola(_faker, _world, variety);
    var exception = Assert.Throws<InvalidOperationException>(() => new Evolution(_world, source, target, EvolutionTrigger.Item, _item));
    Assert.Equal("The source and target form varieties should be different.", exception.Message);
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the item is not in the same world.")]
  public void Given_ItemWorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    Item item = ItemBuilder.ThunderStone();
    EvolutionId evolutionId = EvolutionId.NewId(_world.Id);
    var exception = Assert.Throws<WorldMismatchException>(() => new Evolution(_source, _target, EvolutionTrigger.Item, item, _world.OwnerId, evolutionId));
    Assert.Equal(evolutionId.GetEntity(), exception.Expected);
    Assert.Equal(item.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("item", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the source form is not in the same world.")]
  public void Given_SourceWorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    Form source = FormBuilder.Pikachu();
    EvolutionId evolutionId = EvolutionId.NewId(_world.Id);
    var exception = Assert.Throws<WorldMismatchException>(() => new Evolution(source, _target, EvolutionTrigger.Item, _item, _world.OwnerId, evolutionId));
    Assert.Equal(evolutionId.GetEntity(), exception.Expected);
    Assert.Equal(source.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("source", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the target form is not in the same world.")]
  public void Given_TargetWorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    Form target = FormBuilder.Raichu();
    EvolutionId evolutionId = EvolutionId.NewId(_world.Id);
    var exception = Assert.Throws<WorldMismatchException>(() => new Evolution(_source, target, EvolutionTrigger.Item, _item, _world.OwnerId, evolutionId));
    Assert.Equal(evolutionId.GetEntity(), exception.Expected);
    Assert.Equal(target.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("target", exception.ParamName);
  }

  [Fact(DisplayName = "HeldItemId: it should throw WorldMismatchException when the item is not in the same world.")]
  public void Given_WorldMismatch_When_HeldItemId_Then_WorldMismatchException()
  {
    Evolution evolution = EvolutionBuilder.PikachuToRaichu(_faker, _world);
    Item item = ItemBuilder.ThunderStone();
    var exception = Assert.Throws<WorldMismatchException>(() => evolution.HeldItemId = item.Id);
    Assert.Equal(evolution.Id.GetEntity(), exception.Expected);
    Assert.Equal(item.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("HeldItemId", exception.ParamName);
  }

  [Fact(DisplayName = "KnownMoveId: it should throw WorldMismatchException when the move is not in the same world.")]
  public void Given_WorldMismatch_When_KnownMoveId_Then_WorldMismatchException()
  {
    Evolution evolution = EvolutionBuilder.PikachuToRaichu(_faker, _world);
    Move move = MoveBuilder.ThunderPunch();
    var exception = Assert.Throws<WorldMismatchException>(() => evolution.KnownMoveId = move.Id);
    Assert.Equal(evolution.Id.GetEntity(), exception.Expected);
    Assert.Equal(move.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("KnownMoveId", exception.ParamName);
  }
}
