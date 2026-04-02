using Bogus;
using PokeGame.Core;
using PokeGame.Core.Items;
using PokeGame.Core.Items.Properties;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IItemBuilder
{
  IItemBuilder WithId(ItemId? id);
  IItemBuilder WithWorld(World? world);
  IItemBuilder WithKey(Slug? key);
  IItemBuilder WithName(Name? name);
  IItemBuilder WithDescription(Description? description);
  IItemBuilder WithPrice(Price? price);
  IItemBuilder WithSprite(Url? sprite);
  IItemBuilder WithUrl(Url? url);
  IItemBuilder WithNotes(Notes? notes);
  IItemBuilder WithProperties(ItemProperties? properties);
  IItemBuilder ClearChanges(bool clearChanges = true);

  Item Build();
}

public class ItemBuilder : IItemBuilder
{
  private readonly Faker _faker;

  private bool _clearChanges = false;
  private Description? _description = null;
  private ItemId? _id = null;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private Price? _price = null;
  private ItemProperties? _properties = null;
  private Url? _sprite = null;
  private Url? _url = null;
  private World? _world = null;

  public ItemBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IItemBuilder WithId(ItemId? id)
  {
    _id = id;
    return this;
  }

  public IItemBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IItemBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public IItemBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public IItemBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public IItemBuilder WithPrice(Price? price)
  {
    _price = price;
    return this;
  }

  public IItemBuilder WithSprite(Url? sprite)
  {
    _sprite = sprite;
    return this;
  }

  public IItemBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public IItemBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public IItemBuilder WithProperties(ItemProperties? properties)
  {
    _properties = properties;
    return this;
  }

  public IItemBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Item Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Slug key = _key ?? new("an-item");
    ItemProperties properties = _properties ?? new OtherItemProperties();

    Item item = _id.HasValue ? new(key, properties, world.OwnerId, _id.Value) : new(world, key, properties);
    item.Name = _name;
    item.Description = _description;
    item.Price = _price;
    item.Sprite = _sprite;
    item.Url = _url;
    item.Notes = _notes;
    item.Update(world.OwnerId);

    if (_clearChanges)
    {
      item.ClearChanges();
    }

    return item;
  }

  public static Item Potion(Faker? faker = null, World? world = null) => new ItemBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("potion"))
    .WithName(new Name("Potion"))
    .WithDescription(new Description("A spray-type medicine for treating wounds. It can be used to restore 20 HP to a Pokémon."))
    .WithPrice(new Price(200))
    .WithSprite(new Url("https://archives.bulbagarden.net/media/upload/9/99/Potion_SV.png"))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Potion"))
    .WithNotes(new Notes("When used from the Bag on a Pokémon, it heals the Pokémon by 20 HP."))
    .WithProperties(new MedicineProperties(false, healing: 20, false, false, null, false, 0, false, false))
    .Build();

  public static Item ThunderStone(Faker? faker = null, World? world = null) => new ItemBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("thunder-stone"))
    .WithName(new Name("Thunder Stone"))
    .WithDescription(new Description("A peculiar stone that can make certain species of Pokémon evolve. It has a distinct thunderbolt pattern."))
    .WithPrice(new Price(3000))
    .WithSprite(new Url("https://archives.bulbagarden.net/media/upload/a/a5/Dream_Thunder_Stone_Sprite.png"))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Thunder_Stone"))
    .WithNotes(new Notes("Evolution stone that evolves Electric-related Pokémon (e.g., Pikachu→Raichu, Eevee→Jolteon); consumed on use, with some special-form exceptions."))
    .WithProperties(new OtherItemProperties())
    .Build();
}
