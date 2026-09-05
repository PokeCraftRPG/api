using Bogus;
using Logitar.EventSourcing;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface IFormBuilder
{
  IFormBuilder WithId(FormId formId);
  IFormBuilder WithVariety(Variety variety);
  IFormBuilder WithWorld(World? world);
  IFormBuilder WithCategory(FormCategory category);
  IFormBuilder WithKey(string key);
  IFormBuilder WithName(string? name);
  IFormBuilder WithSummary(string? summary);
  IFormBuilder WithContent(string? content);
  IFormBuilder WithTypes(PokemonType primary, PokemonType? secondary = null);
  IFormBuilder WithAbilities(Ability primary, Ability? secondary = null, Ability? hidden = null);
  IFormBuilder WithBaseStatistics(int hp, int attack, int defense, int specialAttack, int specialDefense, int speed);
  IFormBuilder WithYield(int experience, int hp, int attack, int defense, int specialAttack, int specialDefense, int speed);
  IFormBuilder WithSize(int? height, int? weight);
  IFormBuilder WithSprites(FormSprites? sprites);

  Form Build();
}

public class FormBuilder : IFormBuilder
{
  private readonly Faker _faker;

  private Ability? _hiddenAbility;
  private Ability? _primaryAbility;
  private Ability? _secondaryAbility;
  private int _attack = 49;
  private int _defense = 49;
  private int _hp = 45;
  private int _specialAttack = 65;
  private int _specialDefense = 65;
  private int _speed = 45;
  private FormCategory _category = FormCategory.Default;
  private string? _content;
  private FormId? _formId;
  private int? _height = 7;
  private string _key = "bulbasaur";
  private string? _name = "Bulbasaur";
  private PokemonType _primaryType = PokemonType.Grass;
  private PokemonType? _secondaryType = PokemonType.Poison;
  private FormSprites? _sprites;
  private string? _summary;
  private Variety? _variety;
  private int? _weight = 69;
  private World? _world;
  private int _yieldAttack;
  private int _yieldDefense;
  private int _yieldExperience = 64;
  private int _yieldHp;
  private int _yieldSpecialAttack = 1;
  private int _yieldSpecialDefense;
  private int _yieldSpeed;

  public FormBuilder(Faker? faker = null)
  {
    _faker = faker ?? new();
  }

  public IFormBuilder WithId(FormId formId)
  {
    _formId = formId;
    return this;
  }

  public IFormBuilder WithVariety(Variety variety)
  {
    _variety = variety;
    return this;
  }

  public IFormBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public IFormBuilder WithCategory(FormCategory category)
  {
    _category = category;
    return this;
  }

  public IFormBuilder WithKey(string key)
  {
    _key = key;
    return this;
  }

  public IFormBuilder WithName(string? name)
  {
    _name = name;
    return this;
  }

  public IFormBuilder WithSummary(string? summary)
  {
    _summary = summary;
    return this;
  }

  public IFormBuilder WithContent(string? content)
  {
    _content = content;
    return this;
  }

  public IFormBuilder WithTypes(PokemonType primary, PokemonType? secondary = null)
  {
    _primaryType = primary;
    _secondaryType = secondary;
    return this;
  }

  public IFormBuilder WithAbilities(Ability primary, Ability? secondary = null, Ability? hidden = null)
  {
    _primaryAbility = primary;
    _secondaryAbility = secondary;
    _hiddenAbility = hidden;
    return this;
  }

  public IFormBuilder WithBaseStatistics(int hp, int attack, int defense, int specialAttack, int specialDefense, int speed)
  {
    _hp = hp;
    _attack = attack;
    _defense = defense;
    _specialAttack = specialAttack;
    _specialDefense = specialDefense;
    _speed = speed;
    return this;
  }

  public IFormBuilder WithYield(int experience, int hp, int attack, int defense, int specialAttack, int specialDefense, int speed)
  {
    _yieldExperience = experience;
    _yieldHp = hp;
    _yieldAttack = attack;
    _yieldDefense = defense;
    _yieldSpecialAttack = specialAttack;
    _yieldSpecialDefense = specialDefense;
    _yieldSpeed = speed;
    return this;
  }

  public IFormBuilder WithSize(int? height, int? weight)
  {
    _height = height;
    _weight = weight;
    return this;
  }

  public IFormBuilder WithSprites(FormSprites? sprites)
  {
    _sprites = sprites;
    return this;
  }

  public Form Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    Variety variety = _variety ?? VarietyBuilder.Bulbasaur(_faker, world: world);
    Ability primaryAbility = _primaryAbility ?? AbilityBuilder.Overgrow(_faker, world);
    ActorId actorId = world.OwnerId.ActorId;
    Key key = new(_key);
    FormTypes types = new(_primaryType, _secondaryType);
    FormAbilities abilities = new(primaryAbility.Id, _secondaryAbility?.Id, _hiddenAbility?.Id);
    BaseStatistics baseStatistics = new(_hp, _attack, _defense, _specialAttack, _specialDefense, _speed);
    FormYield yield = new(_yieldExperience, _yieldHp, _yieldAttack, _yieldDefense, _yieldSpecialAttack, _yieldSpecialDefense, _yieldSpeed);

    Form form = _formId.HasValue
      ? new(_formId.Value, _category, variety.Id, key, types, abilities, baseStatistics, yield, actorId)
      : new(variety, _category, key, types, abilities, baseStatistics, yield, actorId);

    form.SetDetails(Name.TryCreate(_name), Summary.TryCreate(_summary), Content.TryCreate(_content), actorId);

    FormSize? size = _height.HasValue && _weight.HasValue ? new FormSize(_height.Value, _weight.Value) : null;
    form.SetTraits(size, _sprites, actorId);

    return form;
  }

  public static Form Bulbasaur(Faker? faker = null, Variety? variety = null, World? world = null)
  {
    IFormBuilder builder = new FormBuilder(faker).WithWorld(world);
    if (variety is not null)
    {
      builder = builder.WithVariety(variety);
    }

    return builder
      .WithCategory(FormCategory.Default)
      .WithKey("bulbasaur")
      .WithName("Bulbasaur")
      .WithSummary("The default Bulbasaur form.")
      .WithContent("A Seed Pokémon with a plant bulb on its back.")
      .WithTypes(PokemonType.Grass, PokemonType.Poison)
      .WithBaseStatistics(45, 49, 49, 65, 65, 45)
      .WithYield(64, 0, 0, 0, 1, 0, 0)
      .WithSize(7, 69)
      .Build();
  }
}
