using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Assets;
using PokeGame.Core.Items;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IItemBuilder
{
  IItemBuilder WithId(ItemId itemId);
  IItemBuilder WithWorld(World? world);
  IItemBuilder WithCategory(ItemCategory category);
  IItemBuilder WithKey(string key);
  IItemBuilder WithName(string? name);
  IItemBuilder WithSummary(string? summary);
  IItemBuilder WithContent(string? content);
  IItemBuilder WithPrice(int? price);
  IItemBuilder WithWeight(int? weight);
  IItemBuilder WithSprite(Asset? sprite);

  Item Build();
}

public class ItemBuilder : IItemBuilder
{
  private readonly Faker _faker;

  private ItemCategory _category = ItemCategory.Medicine;
  private string? _content;
  private ItemId? _itemId;
  private string _key = "potion";
  private string? _name = "Potion";
  private int? _price = 200;
  private Asset? _sprite;
  private string? _summary;
  private int? _weight = 20;
  private World? _world;

  public ItemBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IItemBuilder WithId(ItemId itemId)
  {
    _itemId = itemId;
    return this;
  }

  public IItemBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IItemBuilder WithCategory(ItemCategory category)
  {
    _category = category;
    return this;
  }

  public IItemBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public IItemBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public IItemBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public IItemBuilder WithContent(string? content)
  {
    _content = content;
    return this;
  }

  public IItemBuilder WithPrice(int? price)
  {
    _price = price;
    return this;
  }

  public IItemBuilder WithWeight(int? weight)
  {
    _weight = weight;
    return this;
  }

  public IItemBuilder WithSprite(Asset? sprite)
  {
    _sprite = sprite;
    return this;
  }

  public Item Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    ActorId actorId = world.OwnerId.ActorId;
    Key key = new(_key);

    Item item = _itemId.HasValue
      ? new(_itemId.Value, _category, key, actorId)
      : new(world, _category, key, actorId);

    item.SetDetails(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), actorId);
    item.SetCharacteristics(Price.TryCreate(_price), Weight.TryCreate(_weight), actorId);
    item.SetSprite(_sprite, actorId);

    return item;
  }

  public static Item Potion(Faker? faker = null, World? world = null) => new ItemBuilder(faker)
    .WithWorld(world)
    .WithCategory(ItemCategory.Medicine)
    .WithKey("potion")
    .WithName("Potion")
    .WithSummary("Restores HP.")
    .WithContent("A spray-type medicine that restores 20 HP to a single Pokémon.")
    .WithPrice(200)
    .WithWeight(20)
    .Build();

  public static Item SuperPotion(Faker? faker = null, World? world = null) => new ItemBuilder(faker)
    .WithWorld(world)
    .WithCategory(ItemCategory.Medicine)
    .WithKey("super-potion")
    .WithName("Super Potion")
    .WithSummary("Restores more HP.")
    .WithContent("A spray-type medicine that restores 60 HP to a single Pokémon.")
    .WithPrice(700)
    .WithWeight(30)
    .Build();

  public static Item Antidote(Faker? faker = null, World? world = null) => new ItemBuilder(faker)
    .WithWorld(world)
    .WithCategory(ItemCategory.Medicine)
    .WithKey("antidote")
    .WithName("Antidote")
    .WithSummary("Cures poison.")
    .WithContent("A spray-type medicine that cures poison for a single Pokémon.")
    .WithPrice(200)
    .WithWeight(10)
    .Build();

  public static Item MasterBall(Faker? faker = null, World? world = null) => new ItemBuilder(faker)
    .WithWorld(world)
    .WithCategory(ItemCategory.PokeBall)
    .WithKey("master-ball")
    .WithName("Master Ball")
    .WithSummary("Never fails.")
    .WithContent("The best Poké Ball with the ultimate level of performance. It will catch any wild Pokémon without fail.")
    .WithPrice(null)
    .WithWeight(50)
    .Build();
}
