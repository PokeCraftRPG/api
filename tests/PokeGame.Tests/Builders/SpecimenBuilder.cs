using Bogus;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ISpecimenBuilder
{
  ISpecimenBuilder WithId(PokemonId? id);
  ISpecimenBuilder WithWorld(World? world);
  ISpecimenBuilder Is(SpeciesAggregate species, Variety variety, Form form);
  ISpecimenBuilder WithKey(Slug? key);
  ISpecimenBuilder WithName(Name? name);
  ISpecimenBuilder WithGender(PokemonGender? gender);
  ISpecimenBuilder IsShiny(bool isShiny = true);
  ISpecimenBuilder WithTeraType(PokemonType? teraType);
  ISpecimenBuilder WithSize(PokemonSize? size);
  ISpecimenBuilder WithAbilitySlot(AbilitySlot? abilitySlot);
  ISpecimenBuilder WithNature(PokemonNature? nature);
  ISpecimenBuilder IsEgg(bool isEgg = true);
  ISpecimenBuilder WithEggCycles(EggCycles? eggCycles);
  ISpecimenBuilder IsLevel(int level);
  ISpecimenBuilder HasExperience(int experience);
  ISpecimenBuilder WithIndividualValues(IndividualValues? individualValues);
  ISpecimenBuilder WithEffortValues(EffortValues? effortValues);
  ISpecimenBuilder WithVitality(int? vitality);
  ISpecimenBuilder WithStamina(int? stamina);
  ISpecimenBuilder WithFriendship(Friendship? friendship);
  ISpecimenBuilder WithSprite(Url? sprite);
  ISpecimenBuilder WithUrl(Url? url);
  ISpecimenBuilder WithNotes(Notes? notes);
  ISpecimenBuilder ClearChanges(bool clearChanges = true);

  Specimen Build();
}

public class SpecimenBuilder : ISpecimenBuilder
{
  private readonly Faker _faker;
  private readonly IPokemonRandomizer _randomizer;

  private AbilitySlot? _abilitySlot = null;
  private bool _clearChanges = false;
  private EffortValues? _effortValues = null;
  private EggCycles? _eggCycles = null;
  private int _experience = 0;
  private Form? _form = null;
  private Friendship? _friendship = null;
  private PokemonGender? _gender = null;
  private PokemonId? _id = null;
  private IndividualValues? _individualValues = null;
  private bool _isEgg = false;
  private bool _isShiny = false;
  private Slug? _key = null;
  private int _level = 0;
  private Name? _name = null;
  private PokemonNature? _nature = null;
  private Notes? _notes = null;
  private PokemonSize? _size = null;
  private SpeciesAggregate? _species = null;
  private Url? _sprite = null;
  private int? _stamina = null;
  private PokemonType? _teraType = null;
  private Url? _url = null;
  private Variety? _variety = null;
  private int? _vitality = null;
  private World? _world = null;

  public SpecimenBuilder(Faker? faker = null, IPokemonRandomizer? randomizer = null)
  {
    _faker = faker ?? new();
    _randomizer = randomizer ?? PokemonRandomizer.Instance;
  }

  public ISpecimenBuilder WithId(PokemonId? id)
  {
    _id = id;
    return this;
  }

  public ISpecimenBuilder WithWorld(World? world)
  {
    _world = world;
    return this;
  }

  public ISpecimenBuilder Is(SpeciesAggregate species, Variety variety, Form form)
  {
    _species = species;
    _variety = variety;
    _form = form;
    return this;
  }

  public ISpecimenBuilder WithKey(Slug? key)
  {
    _key = key;
    return this;
  }

  public ISpecimenBuilder WithName(Name? name)
  {
    _name = name;
    return this;
  }

  public ISpecimenBuilder WithGender(PokemonGender? gender)
  {
    _gender = gender;
    return this;
  }

  public ISpecimenBuilder WithSprite(Url? sprite)
  {
    _sprite = sprite;
    return this;
  }

  public ISpecimenBuilder IsShiny(bool isShiny = true)
  {
    _isShiny = isShiny;
    return this;
  }

  public ISpecimenBuilder WithTeraType(PokemonType? teraType)
  {
    _teraType = teraType;
    return this;
  }

  public ISpecimenBuilder WithSize(PokemonSize? size)
  {
    _size = size;
    return this;
  }

  public ISpecimenBuilder WithAbilitySlot(AbilitySlot? abilitySlot)
  {
    _abilitySlot = abilitySlot;
    return this;
  }

  public ISpecimenBuilder WithNature(PokemonNature? nature)
  {
    _nature = nature;
    return this;
  }

  public ISpecimenBuilder IsEgg(bool isEgg = true)
  {
    _isEgg = isEgg;
    return this;
  }

  public ISpecimenBuilder WithEggCycles(EggCycles? eggCycles)
  {
    _eggCycles = eggCycles;
    return this;
  }

  public ISpecimenBuilder IsLevel(int level)
  {
    _level = level;
    return this;
  }

  public ISpecimenBuilder HasExperience(int experience)
  {
    _experience = experience;
    return this;
  }

  public ISpecimenBuilder WithIndividualValues(IndividualValues? individualValues)
  {
    _individualValues = individualValues;
    return this;
  }

  public ISpecimenBuilder WithEffortValues(EffortValues? effortValues)
  {
    _effortValues = effortValues;
    return this;
  }

  public ISpecimenBuilder WithVitality(int? vitality)
  {
    _vitality = vitality;
    return this;
  }

  public ISpecimenBuilder WithStamina(int? stamina)
  {
    _stamina = stamina;
    return this;
  }

  public ISpecimenBuilder WithFriendship(Friendship? friendship)
  {
    _friendship = friendship;
    return this;
  }

  public ISpecimenBuilder WithUrl(Url? url)
  {
    _url = url;
    return this;
  }

  public ISpecimenBuilder WithNotes(Notes? notes)
  {
    _notes = notes;
    return this;
  }

  public ISpecimenBuilder ClearChanges(bool clearChanges = true)
  {
    _clearChanges = clearChanges;
    return this;
  }

  public Specimen Build()
  {
    World world = _world ?? new WorldBuilder(_faker).Build();
    SpeciesAggregate species = _species ?? SpeciesBuilder.Pikachu(_faker, world);
    Variety variety = _variety ?? VarietyBuilder.Pikachu(_faker, world, species);
    Form form = _form ?? FormBuilder.Pikachu(_faker, world, variety, new FormAbilities(AbilityBuilder.Static(_faker, world), secondary: null, AbilityBuilder.LightningRod(_faker, world)));
    PokemonGender? gender = _gender ?? _randomizer.Gender(variety.GenderRatio);
    PokemonSize size = _size ?? _randomizer.PokemonSize();
    AbilitySlot abilitySlot = _abilitySlot ?? _randomizer.AbilitySlot(form.Abilities);
    PokemonNature nature = _nature ?? _randomizer.PokemonNature();
    EggCycles? eggCycles = _eggCycles ?? (_isEgg ? species.EggCycles : null);
    IndividualValues individualValues = _individualValues ?? _randomizer.IndividualValues();

    int experience = _experience;
    if (experience <= 0 && _level > 0)
    {
      int minimumExperience = _level > 1 ? 1 + ExperienceTable.Instance.GetMaximumExperience(species.GrowthRate, _level - 1) : 0;
      int maximumExperience = ExperienceTable.Instance.GetMaximumExperience(species.GrowthRate, _level);
      experience = _faker.Random.Int(minimumExperience, maximumExperience);
    }

    Specimen specimen = _id.HasValue
      ? new(species, variety, form, _key, gender, _isShiny, _teraType, size, abilitySlot, nature, eggCycles, experience, individualValues, _effortValues, _vitality, _stamina, _friendship, world.OwnerId, _id.Value)
      : new(world, species, variety, form, _key, gender, _isShiny, _teraType, size, abilitySlot, nature, eggCycles, experience, individualValues, _effortValues, _vitality, _stamina, _friendship);

    specimen.Nickname(_name, world.OwnerId);

    specimen.Sprite = _sprite;
    specimen.Url = _url;
    specimen.Notes = _notes;
    specimen.Update(world.OwnerId);

    if (_clearChanges)
    {
      specimen.ClearChanges();
    }

    return specimen;
  }
}
