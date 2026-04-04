using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class SpecimenTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly SpeciesAggregate _species;
  private readonly Variety _variety;
  private readonly Form _form;
  private readonly Slug _key;
  private readonly PokemonGender _gender;
  private readonly bool _isShiny;
  private readonly PokemonType _teraType;
  private readonly PokemonSize _size;
  private readonly AbilitySlot _abilitySlot;
  private readonly PokemonNature _nature;

  public SpecimenTests()
  {
    _world = new WorldBuilder(_faker).Build();

    _species = SpeciesBuilder.Pikachu(_faker, _world);
    _variety = VarietyBuilder.Pikachu(_faker, _world, _species);
    _form = FormBuilder.Pikachu(_faker, _world, _variety);
    _key = _species.Key;
    _gender = _faker.PickRandom<PokemonGender>();
    _isShiny = _faker.Random.Bool();
    _teraType = _faker.PickRandom<PokemonType>();
    _size = PokemonRandomizer.Instance.PokemonSize();
    _abilitySlot = _faker.PickRandom(AbilitySlot.Primary, AbilitySlot.Hidden);
    _nature = _faker.PickRandom(PokemonNatures.Instance.ToList().ToArray());
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentException when the form does not belong to the variety.")]
  public void Given_InvalidForm_When_ctor_Then_ArgumentException()
  {
    Form form = FormBuilder.Raichu(_faker, _world);
    var exception = Assert.Throws<ArgumentException>(
      () => new Specimen(_world, _species, _variety, form, _key, _gender, _isShiny, _teraType, _size, _abilitySlot, _nature));
    Assert.Equal("form", exception.ParamName);
    Assert.StartsWith("The form should belong to the variety.", exception.Message);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentException when the variety does not belong to the species.")]
  public void Given_InvalidVariety_When_ctor_Then_ArgumentException()
  {
    Variety variety = VarietyBuilder.Raichu(_faker, _world);
    var exception = Assert.Throws<ArgumentException>(
      () => new Specimen(_world, _species, variety, _form, _key, _gender, _isShiny, _teraType, _size, _abilitySlot, _nature));
    Assert.Equal("variety", exception.ParamName);
    Assert.StartsWith("The variety should belong to the species.", exception.Message);
  }

  [Fact(DisplayName = "ctor: it should throw ArgumentOutOfRangeException when the gender is not defined.")]
  public void Given_GenderNotDefined_When_ctor_Then_ArgumentOutOfRangeException()
  {
    var exception = Assert.Throws<ArgumentOutOfRangeException>(
      () => new Specimen(_world, _species, _variety, _form, key: null, (PokemonGender)(-1), _isShiny, _teraType, _size, _abilitySlot, _nature));
    Assert.Equal("gender", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw InvalidGenderException when the gender is not valid.")]
  public void Given_GenderNotValid_When_ctor_Then_InvalidGenderException()
  {
    var exception = Assert.Throws<InvalidGenderException>(
      () => new Specimen(_world, _species, _variety, _form, key: null, gender: null, _isShiny, _teraType, _size, _abilitySlot, _nature));
    Assert.Equal(_variety.GenderRatio?.Value, exception.GenderRatio);
    Assert.Null(exception.Gender);
    Assert.Equal("Gender", exception.PropertyName);
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the form is not in the same world.")]
  public void Given_FormWorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    Form form = FormBuilder.Pikachu();
    SpecimenId specimenId = SpecimenId.NewId(_world.Id);
    var exception = Assert.Throws<WorldMismatchException>(
      () => new Specimen(_species, _variety, form, _key, _gender, _isShiny, _teraType, _size, _abilitySlot, _nature, _world.OwnerId, specimenId));
    Assert.Equal(specimenId.GetEntity(), exception.Expected);
    Assert.Equal(form.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("form", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the species is not in the same world.")]
  public void Given_SpeciesWorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    SpeciesAggregate species = SpeciesBuilder.Pikachu();
    SpecimenId specimenId = SpecimenId.NewId(_world.Id);
    var exception = Assert.Throws<WorldMismatchException>(
      () => new Specimen(species, _variety, _form, _key, _gender, _isShiny, _teraType, _size, _abilitySlot, _nature, _world.OwnerId, specimenId));
    Assert.Equal(specimenId.GetEntity(), exception.Expected);
    Assert.Equal(species.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("species", exception.ParamName);
  }

  [Fact(DisplayName = "ctor: it should throw WorldMismatchException when the variety is not in the same world.")]
  public void Given_VarietyWorldMismatch_When_ctor_Then_WorldMismatchException()
  {
    Variety variety = VarietyBuilder.Pikachu();
    SpecimenId specimenId = SpecimenId.NewId(_world.Id);
    var exception = Assert.Throws<WorldMismatchException>(
      () => new Specimen(_species, variety, _form, _key, _gender, _isShiny, _teraType, _size, _abilitySlot, _nature, _world.OwnerId, specimenId));
    Assert.Equal(specimenId.GetEntity(), exception.Expected);
    Assert.Equal(variety.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("variety", exception.ParamName);
  }
}
