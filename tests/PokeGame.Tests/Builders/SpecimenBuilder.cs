using Bogus;
using PokeGame.Core;
using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Builders;

public interface ISpecimenBuilder
{
  ISpecimenBuilder WithId(SpecimenId? id);
  ISpecimenBuilder WithWorld(World? world);
  ISpecimenBuilder Is(SpeciesAggregate species, Variety variety, Form form);
  ISpecimenBuilder WithKey(Slug? key);
  ISpecimenBuilder WithName(Name? name);
  ISpecimenBuilder WithGender(PokemonGender? gender);
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

  private bool _clearChanges = false;
  private Form? _form = null;
  private PokemonGender? _gender = null;
  private SpecimenId? _id = null;
  private Slug? _key = null;
  private Name? _name = null;
  private Notes? _notes = null;
  private SpeciesAggregate? _species = null;
  private Url? _sprite = null;
  private Url? _url = null;
  private Variety? _variety = null;
  private World? _world = null;

  public SpecimenBuilder(Faker? faker = null, IPokemonRandomizer? randomizer = null)
  {
    _faker = faker ?? new();
    _randomizer = randomizer ?? PokemonRandomizer.Instance;
  }

  public ISpecimenBuilder WithId(SpecimenId? id)
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
    SpeciesAggregate species = _species ?? SpeciesBuilder.Pikachu(_faker, _world);
    Variety variety = _variety ?? VarietyBuilder.Pikachu(_faker, _world, species);
    Form form = _form ?? FormBuilder.Pikachu(_faker, _world, variety, new Abilities(AbilityBuilder.Static(_faker, _world), secondary: null, AbilityBuilder.LightningRod(_faker, _world)));
    PokemonGender? gender = _gender ?? _randomizer.Gender(variety.GenderRatio);

    Specimen specimen = _id.HasValue
      ? new(species, variety, form, _key, gender, world.OwnerId, _id.Value)
      : new(world, species, variety, form, _key, gender);

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
