using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class SpecimenHeldItemTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly Specimen _specimen;
  private readonly Item _item;

  public SpecimenHeldItemTests()
  {
    _world = new WorldBuilder(_faker).Build();
    _specimen = new SpecimenBuilder(_faker, PokemonRandomizer.Instance).WithWorld(_world).Build();
    _item = ItemBuilder.OranBerry(_faker, _world);
  }

  [Fact(DisplayName = "SetHeldItem: it should handle changes correctly.")]
  public void Given_Item_When_SetHeldItem_Then_ChangeHandled()
  {
    Assert.False(_specimen.HeldItemId.HasValue);

    _specimen.SetHeldItem(_item, _world.OwnerId);
    Assert.Equal(_item.Id, _specimen.HeldItemId);
    Assert.True(_specimen.HasChanges);
    Assert.Contains(_specimen.Changes, change => change is PokemonHeldItemChanged changed && changed.ItemId == _item.Id && changed.ActorId == _world.OwnerId.ActorId);

    _specimen.ClearChanges();
    _specimen.SetHeldItem(_item, _world.OwnerId);
    Assert.False(_specimen.HasChanges);
    Assert.Empty(_specimen.Changes);
  }

  [Fact(DisplayName = "SetHeldItem: it should throw WorldMismatchException when the item is not in the same world.")]
  public void Given_WorldMismatch_When_SetHeldItem_Then_WorldMismatchException()
  {
    Item item = ItemBuilder.OranBerry();
    var exception = Assert.Throws<WorldMismatchException>(() => _specimen.SetHeldItem(item, _world.OwnerId));
    Assert.Equal(_specimen.Id.GetEntity(), exception.Expected);
    Assert.Equal(item.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("item", exception.ParamName);
  }
}
