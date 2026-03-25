using Bogus;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IRegionBuilder
{
  IRegionBuilder WithId(RegionId? id);
  IRegionBuilder WithWorld(World? world);
  IRegionBuilder WithKey(Slug? key);
  IRegionBuilder WithName(Name? name);
  IRegionBuilder WithDescription(Description? description);
  IRegionBuilder WithUrl(Url? url);
  IRegionBuilder WithNotes(Notes? notes);
  IRegionBuilder ClearChanges(bool clearChanges = true);

  Region Build();
}

public class RegionBuilder : IRegionBuilder
{
  private readonly Faker _faker;

  private Description? _description = null;
  private RegionId? _id = null;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private Url? _url = null;
  private World? _world = null;
  private bool _clearChanges = false;

  public RegionBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IRegionBuilder WithId(RegionId? id)
  {
    _id = id;
    return this;
  }

  public IRegionBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IRegionBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public IRegionBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public IRegionBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public IRegionBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public IRegionBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public IRegionBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Region Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Slug key = _key ?? new("a-region");

    Region region = _id.HasValue ? new(key, world.OwnerId, _id.Value) : new(world, key);
    region.Name = _name;
    region.Description = _description;
    region.Url = _url;
    region.Notes = _notes;
    region.Update(world.OwnerId);

    if (_clearChanges)
    {
      region.ClearChanges();
    }

    return region;
  }

  public static Region Hoenn(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("hoenn"))
    .WithName(new Name("Hoenn"))
    .WithDescription(new Description("Explore Hoenn, a tropical, water-rich region. Start in Littleroot, face Team Magma/Aqua, calm Groudon/Kyogre, and become Champion."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Hoenn"))
    .WithNotes(new Notes("Hoenn is a Kyushu-inspired region of sea and land balance, shaped by primal legends. Features dual villain arcs, climate variety, and strong nature themes."))
    .Build();

  public static Region Johto(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("johto"))
    .WithName(new Name("Johto"))
    .WithDescription(new Description("Explore Johto, a historic, rural region west of Kanto. Start in New Bark Town, earn 8 Badges, face Team Rocket, and encounter Legendary Pokémon."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Johto"))
    .WithNotes(new Notes("Johto is a Kansai-inspired region tied to Kanto, rich in myth and landmarks. Features shared League, Team Rocket plot, and Legendary-driven lore arcs."))
    .Build();

  public static Region Kanto(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("kanto"))
    .WithName(new Name("Kanto"))
    .WithDescription(new Description("Kanto is the classic region where many Trainer journeys begin: Professor Oak, Pallet Town, 8 Gyms, Team Rocket, and the Indigo League."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Kanto_(Region)"))
    .WithNotes(new Notes("Classic, temperate region east of Johto: 10 settlements, 8 Gyms, Team Rocket, Oak in Pallet, and the Indigo League at Indigo Plateau."))
    .Build();

  public static Region Sinnoh(Faker? faker = null, World? world = null) => new RegionBuilder(faker)
    .WithWorld(world)
    .WithKey(new Slug("sinnoh"))
    .WithName(new Name("Sinnoh"))
    .WithDescription(new Description("Explore Sinnoh, a cold, mountainous region split by Mt. Coronet. Face Team Galactic, stop reality warping, and become Champion."))
    .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Hoenn"))
    .WithNotes(new Notes("Sinnoh is a myth-rich northern region tied to creation lore, with varied climates, strong geography, and a central conflict over time, space, and reality."))
    .Build();
}
