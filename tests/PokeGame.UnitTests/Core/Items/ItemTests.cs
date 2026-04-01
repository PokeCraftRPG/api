using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Items.Events;
using PokeGame.Core.Items.Properties;
using PokeGame.Core.Moves;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Items;

[Trait(Traits.Category, Categories.Unit)]
public class ItemTests
{
  private readonly Faker _faker = new();

  private readonly World _world;

  public ItemTests()
  {
    _world = new WorldBuilder(_faker).Build();
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the category is not defined.")]
  public void Given_CategoryNotDefined_When_ctor_Then_ArgumentOutOfRangeException()
  {
    UndefinedCategoryProperties properties = new();
    var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new Item(_world, new Slug("potion"), properties));
    Assert.Equal("properties", exception.ParamName);
  }

  [Fact(DisplayName = "SetProperties: it should handle properties changes.")]
  public void Given_Properties_When_SetProperties_Then_Changed()
  {
    Item potion = new ItemBuilder(_faker).WithWorld(_world).WithProperties(new MedicineProperties()).ClearChanges().Build();

    MedicineProperties properties = new(false, healing: 20, false, false, null, false, 0, false, false);
    potion.SetProperties(properties, _world.OwnerId);
    Assert.Equal(properties, potion.Properties);
    Assert.True(potion.HasChanges);
    Assert.Contains(potion.Changes, change => change is MedicinePropertiesChanged changed && changed.Properties == properties && changed.ActorId == _world.OwnerId.ActorId);

    potion.ClearChanges();
    potion.SetProperties(properties, _world.OwnerId);
    Assert.False(potion.HasChanges);
    Assert.Empty(potion.Changes);
  }

  [Fact(DisplayName = "SetProperties: it should throw ArgumentException when the properties category does not match the item.")]
  public void Given_DifferentCategory_When_SetProperties_Then_ArgumentException()
  {
    Item potion = ItemBuilder.Potion(_faker, _world);
    TreasureProperties properties = new();

    var exception = Assert.Throws<ArgumentException>(() => potion.SetProperties(properties, _world.OwnerId));
    Assert.Equal("properties", exception.ParamName);
    Assert.StartsWith("Cannot set properties of category 'Treasure' on an item in category 'Medicine'.", exception.Message);
  }

  [Fact(DisplayName = "SetProperties: it should throw WorldMismatchException when the move is from another world.")]
  public void Given_MoveFromAnotherWorld_When_SetProperties_Then_WorldMismatchException()
  {
    Move thunderShock = MoveBuilder.ThunderShock(_faker, _world);
    Item technicalMachine = new ItemBuilder(_faker).WithWorld(_world).WithProperties(new TechnicalMachineProperties(thunderShock)).Build();

    Move thunderPunch = MoveBuilder.ThunderPunch();
    var exception = Assert.Throws<WorldMismatchException>(() => technicalMachine.SetProperties(new TechnicalMachineProperties(thunderPunch), _world.OwnerId));
    Assert.Equal(technicalMachine.Id.GetEntity(), exception.Expected);
    Assert.Equal(thunderPunch.Id.GetEntity(), Assert.Single(exception.Mismatched));
  }
}
