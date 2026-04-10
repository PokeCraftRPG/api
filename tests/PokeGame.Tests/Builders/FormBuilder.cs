using Bogus;
using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IFormBuilder
{
  IFormBuilder WithId(FormId? id);
  IFormBuilder WithWorld(World? world);
  IFormBuilder WithVariety(Variety? variety);
  IFormBuilder IsDefault(bool isDefault = true);
  IFormBuilder WithKey(Slug? key);
  IFormBuilder WithName(Name? name);
  IFormBuilder WithDescription(Description? description);
  IFormBuilder IsBattleOnly(bool isBattleOnly = true);
  IFormBuilder IsMega(bool isMega = true);
  IFormBuilder WithHeight(Height? height);
  IFormBuilder WithWeight(Weight? weight);
  IFormBuilder WithTypes(FormTypes? types);
  IFormBuilder WithAbilities(FormAbilities? abilities);
  IFormBuilder WithBaseStatistics(BaseStatistics? baseStatistics);
  IFormBuilder WithYield(Yield? yield);
  IFormBuilder WithSprites(Sprites? sprites);
  IFormBuilder WithUrl(Url? url);
  IFormBuilder WithNotes(Notes? notes);
  IFormBuilder ClearChanges(bool clearChanges = true);

  Form Build();
}

public class FormBuilder : IFormBuilder
{
  private readonly Faker _faker;

  private FormAbilities? _abilities = null;
  private BaseStatistics? _baseStatistics = null;
  private bool _clearChanges = false;
  private Description? _description = null;
  private Height? _height = null;
  private FormId? _id = null;
  private bool _isBattleOnly = false;
  private bool _isDefault = false;
  private bool _isMega = false;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private Sprites? _sprites = null;
  private FormTypes? _types = null;
  private Url? _url = null;
  private Variety? _variety = null;
  private Weight? _weight;
  private World? _world = null;
  private Yield? _yield = null;

  public IFormBuilder WithId(FormId? id)
  {
    _id = id;
    return this;
  }

  public IFormBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IFormBuilder WithVariety(Variety? variety)
  {
    _variety = variety;
    return this;
  }

  public IFormBuilder IsDefault(bool isDefault = true)
  {
    _isDefault = isDefault;
    return this;
  }

  public IFormBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public IFormBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public IFormBuilder WithDescription(Description? description)
  {
    _description = description;
    return this;
  }

  public IFormBuilder IsBattleOnly(bool isBattleOnly = true)
  {
    _isBattleOnly = isBattleOnly;
    return this;
  }

  public IFormBuilder IsMega(bool isMega = true)
  {
    _isMega = isMega;
    return this;
  }

  public IFormBuilder WithHeight(Height? height)
  {
    _height = height;
    return this;
  }

  public IFormBuilder WithWeight(Weight? weight)
  {
    _weight = weight;
    return this;
  }

  public IFormBuilder WithTypes(FormTypes? types)
  {
    _types = types;
    return this;
  }

  public IFormBuilder WithAbilities(FormAbilities? abilities)
  {
    _abilities = abilities;
    return this;
  }

  public IFormBuilder WithBaseStatistics(BaseStatistics? baseStatistics)
  {
    _baseStatistics = baseStatistics;
    return this;
  }

  public IFormBuilder WithYield(Yield? yield)
  {
    _yield = yield;
    return this;
  }

  public IFormBuilder WithSprites(Sprites? sprites)
  {
    _sprites = sprites;
    return this;
  }

  public IFormBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public IFormBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public IFormBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public FormBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public Form Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Variety variety = _variety ?? new VarietyBuilder(_faker).WithWorld(world).Build();
    Slug key = _key ?? new("a-form");
    Height height = _height ?? new(1);
    Weight weight = _weight ?? new(1);
    FormTypes types = _types ?? new();
    FormAbilities abilities = _abilities ?? new(new AbilityBuilder(_faker).WithWorld(world).Build());
    BaseStatistics baseStatistics = _baseStatistics ?? new(35, 55, 40, 50, 50, 90);
    Yield yield = _yield ?? new(112, 0, 0, 0, 0, 0, 2);
    Sprites sprites = _sprites ?? new(
      new Url("https://archives.bulbagarden.net/media/upload/8/85/HOME0025.png"),
      new Url("https://archives.bulbagarden.net/media/upload/1/1a/HOME0025_f.png"),
      new Url("https://archives.bulbagarden.net/media/upload/0/0b/HOME0025_s.png"),
      new Url("https://archives.bulbagarden.net/media/upload/0/05/HOME0025_f_s.png"));

    Form form = _id.HasValue
      ? new(variety, _isDefault, key, height, weight, types, abilities, baseStatistics, yield, sprites, world.OwnerId, _id.Value)
      : new(world, variety, _isDefault, key, height, weight, types, abilities, baseStatistics, yield, sprites);

    form.Name = _name;
    form.Description = _description;

    form.IsBattleOnly = _isBattleOnly;
    form.IsMega = _isMega;

    form.Url = _url;
    form.Notes = _notes;

    form.Update(world.OwnerId);

    if (_clearChanges)
    {
      variety.ClearChanges();
    }

    return form;
  }

  public static Form Darmanitan(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Darmanitan(faker, world))
      .IsDefault()
      .WithKey(new Slug("darmanitan"))
      .WithName(new Name("Darmanitan"))
      .WithHeight(new Height(13))
      .WithWeight(new Weight(929))
      .WithTypes(new FormTypes(PokemonType.Fire))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.SheerForce(faker, world), secondary: null, AbilityBuilder.ZenMode(faker, world)))
      .WithBaseStatistics(new BaseStatistics(105, 140, 55, 30, 55, 95))
      .WithYield(new Yield(168, 0, 2, 0, 0, 0, 0))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/a/a8/HOME0555.png"),
        new Url("https://archives.bulbagarden.net/media/upload/c/c0/HOME0555_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Darmanitan_(Pok%C3%A9mon)"))
      .Build();
  }

  public static Form DarmanitanZen(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Darmanitan(faker, world))
      .IsDefault()
      .WithKey(new Slug("darmanitan-zen"))
      .WithName(new Name("Zen Darmanitan"))
      .IsBattleOnly()
      .WithHeight(new Height(13))
      .WithWeight(new Weight(929))
      .WithTypes(new FormTypes(PokemonType.Fire, PokemonType.Psychic))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.SheerForce(faker, world), secondary: null, AbilityBuilder.ZenMode(faker, world)))
      .WithBaseStatistics(new BaseStatistics(105, 140, 55, 30, 55, 95))
      .WithYield(new Yield(168, 0, 0, 0, 2, 0, 0))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/8/89/HOME0555Z.png"),
        new Url("https://archives.bulbagarden.net/media/upload/8/8a/HOME0555Z_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Darmanitan_(Pok%C3%A9mon)"))
      .Build();
  }

  public static Form Groudon(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Groudon(faker, world))
      .IsDefault()
      .WithKey(new Slug("groudon"))
      .WithName(new Name("Groudon"))
      .WithHeight(new Height(35))
      .WithWeight(new Weight(9500))
      .WithTypes(new FormTypes(PokemonType.Ground))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.Drought(faker, world)))
      .WithBaseStatistics(new BaseStatistics(100, 150, 140, 100, 90, 90))
      .WithYield(new Yield(302, 0, 3, 0, 0, 0, 0))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/0/0e/HOME0383.png"),
        new Url("https://archives.bulbagarden.net/media/upload/4/42/HOME0383_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Groudon_(Pok%C3%A9mon)"))
      .Build();
  }

  public static Form GroudonPrimal(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Groudon(faker, world))
      .IsDefault()
      .WithKey(new Slug("groudon-primal"))
      .WithName(new Name("Primal Groudon"))
      .WithHeight(new Height(50))
      .WithWeight(new Weight(9997))
      .WithTypes(new FormTypes(PokemonType.Ground, PokemonType.Fire))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.DesolateLand(faker, world)))
      .WithBaseStatistics(new BaseStatistics(100, 180, 160, 150, 90, 90))
      .WithYield(new Yield(302, 0, 3, 0, 0, 0, 0))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/8/8c/HOME0383P.png"),
        new Url("https://archives.bulbagarden.net/media/upload/7/79/HOME0383P_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Groudon_(Pok%C3%A9mon)"))
      .Build();
  }

  public static Form Pichu(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Pichu(faker, world))
      .IsDefault()
      .WithKey(new Slug("pichu"))
      .WithName(new Name("Pichu"))
      .WithHeight(new Height(3))
      .WithWeight(new Weight(20))
      .WithTypes(new FormTypes(PokemonType.Electric))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.Static(faker, world), secondary: null, AbilityBuilder.LightningRod(faker, world)))
      .WithBaseStatistics(new BaseStatistics(20, 40, 15, 35, 35, 60))
      .WithYield(new Yield(41, 0, 0, 0, 0, 0, 1))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/5/5f/HOME0172.png"),
        new Url("https://archives.bulbagarden.net/media/upload/d/df/HOME0172_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Pichu_(Pok%C3%A9mon)"))
      .Build();
  }

  public static Form Pikachu(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Pikachu(faker, world))
      .IsDefault()
      .WithKey(new Slug("pikachu"))
      .WithName(new Name("Pikachu"))
      .WithHeight(new Height(4))
      .WithWeight(new Weight(60))
      .WithTypes(new FormTypes(PokemonType.Electric))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.Static(faker, world), secondary: null, AbilityBuilder.LightningRod(faker, world)))
      .WithBaseStatistics(new BaseStatistics(35, 55, 40, 50, 50, 90))
      .WithYield(new Yield(112, 0, 0, 0, 0, 0, 2))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/8/85/HOME0025.png"),
        new Url("https://archives.bulbagarden.net/media/upload/0/0b/HOME0025_s.png"),
        new Url("https://archives.bulbagarden.net/media/upload/1/1a/HOME0025_f.png"),
        new Url("https://archives.bulbagarden.net/media/upload/0/05/HOME0025_f_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Pikachu_(Pok%C3%A9mon)"))
      .Build();
  }

  public static Form Raichu(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Raichu(faker, world))
      .IsDefault()
      .WithKey(new Slug("raichu"))
      .WithName(new Name("Raichu"))
      .WithHeight(new Height(8))
      .WithWeight(new Weight(300))
      .WithTypes(new FormTypes(PokemonType.Electric))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.Static(faker, world), secondary: null, AbilityBuilder.LightningRod(faker, world)))
      .WithBaseStatistics(new BaseStatistics(60, 90, 55, 90, 80, 110))
      .WithYield(new Yield(218, 0, 0, 0, 0, 0, 3))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/e/ed/HOME0026.png"),
        new Url("https://archives.bulbagarden.net/media/upload/f/f8/HOME0026_s.png"),
        new Url("https://archives.bulbagarden.net/media/upload/7/77/HOME0026_f.png"),
        new Url("https://archives.bulbagarden.net/media/upload/7/7f/HOME0026_f_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)"))
      .Build();
  }

  public static Form RaichuAlola(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Raichu(faker, world))
      .IsDefault()
      .WithKey(new Slug("raichu-alola"))
      .WithName(new Name("Alolan Raichu"))
      .WithHeight(new Height(7))
      .WithWeight(new Weight(210))
      .WithTypes(new FormTypes(PokemonType.Electric, PokemonType.Fairy))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.SurgeSurfer(faker, world), secondary: null, AbilityBuilder.LightningRod(faker, world)))
      .WithBaseStatistics(new BaseStatistics(60, 85, 50, 95, 85, 110))
      .WithYield(new Yield(218, 0, 0, 0, 0, 0, 3))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png"),
        new Url("https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Raichu_(Pok%C3%A9mon)"))
      .Build();
  }

  public static Form Tepig(Faker? faker = null, World? world = null, Variety? variety = null, FormAbilities? abilities = null)
  {
    world ??= new WorldBuilder(faker).Build();
    return new FormBuilder(faker)
      .WithWorld(world)
      .WithVariety(variety ?? VarietyBuilder.Tepig(faker, world))
      .IsDefault()
      .WithKey(new Slug("tepig"))
      .WithName(new Name("Tepig"))
      .WithHeight(new Height(5))
      .WithWeight(new Weight(99))
      .WithTypes(new FormTypes(PokemonType.Fire))
      .WithAbilities(abilities ?? new FormAbilities(AbilityBuilder.Blaze(faker, world), secondary: null, AbilityBuilder.ThickFat(faker, world)))
      .WithBaseStatistics(new BaseStatistics(65, 63, 45, 45, 45, 45))
      .WithYield(new Yield(62, 1, 0, 0, 0, 0, 0))
      .WithSprites(new Sprites(
        new Url("https://archives.bulbagarden.net/media/upload/5/56/HOME0026A.png"),
        new Url("https://archives.bulbagarden.net/media/upload/d/dd/HOME0026A_s.png")))
      .WithUrl(new Url("https://bulbapedia.bulbagarden.net/wiki/Tepig_(Pok%C3%A9mon)"))
      .Build();
  }
}
