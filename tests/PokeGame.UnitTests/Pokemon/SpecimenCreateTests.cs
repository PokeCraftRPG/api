using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

public class SpecimenCreateTests : UnitTests
{
  [Fact(DisplayName = "It should create a Pokémon and raise PokemonCreated.")]
  public void Given_ValidForm_When_Create_Then_Created()
  {
    Specimen pokemon = Catalog.CreatePokemon("bulbasaur");

    Assert.Equal(Catalog.Form.Id, pokemon.FormId);
    Assert.Equal("bulbasaur", pokemon.Key.Value);
    Assert.Equal(AbilitySlot.Primary, pokemon.AbilitySlot);
    Assert.False(pokemon.IsEgg);
    Assert.Equal(Catalog.Red.Id.WorldId, pokemon.WorldId);
    Assert.IsType<PokemonCreated>(pokemon.Changes.First());
  }

  [Theory(DisplayName = "It should allow any ability slot regardless of the form abilities.")]
  [InlineData(AbilitySlot.Secondary)]
  [InlineData(AbilitySlot.Hidden)]
  public void Given_SlotWithoutFormAbility_When_Create_Then_Created(AbilitySlot slot)
  {
    Specimen pokemon = Catalog.CreatePokemon(abilitySlot: slot);

    Assert.Equal(slot, pokemon.AbilitySlot);
  }

  [Fact(DisplayName = "It should create an egg.")]
  public void Given_EggCycles_When_Create_Then_EggCreated()
  {
    Specimen pokemon = Catalog.CreatePokemon(eggCycles: 10);

    Assert.Equal((byte)10, pokemon.EggCycles);
    Assert.True(pokemon.IsEgg);
  }

  [Fact(DisplayName = "It should throw InvalidEggCyclesException when egg cycles exceed the species maximum.")]
  public void Given_EggCyclesAboveSpecies_When_Create_Then_InvalidEggCyclesException()
  {
    byte attempted = (byte)(Catalog.Species.Eggs.Cycles + 1);

    InvalidEggCyclesException exception = Assert.Throws<InvalidEggCyclesException>(
      () => Catalog.CreatePokemon(eggCycles: attempted));
    Assert.Equal(Catalog.Species.EntityId, exception.Data["SpeciesId"]);
    Assert.Equal(Catalog.Species.Eggs.Cycles, exception.Data["MaximumEggCycles"]);
    Assert.Equal(attempted, exception.Data["AttemptedEggCycles"]);
  }

  [Fact(DisplayName = "It should throw InvalidPokemonFormCategoryException when the form category is invalid.")]
  public void Given_MegaForm_When_Create_Then_InvalidPokemonFormCategoryException()
  {
    Form mega = new FormBuilder(Faker)
      .WithWorld(Catalog.World)
      .WithVariety(Catalog.Variety)
      .WithAbilities(Catalog.Ability)
      .WithCategory(FormCategory.Mega)
      .WithKey("mega-bulb")
      .Build();

    InvalidPokemonFormCategoryException exception = Assert.Throws<InvalidPokemonFormCategoryException>(
      () => new Specimen(Catalog.Randomizer, PokemonId.NewId(Catalog.World.Id), Catalog.Species, Catalog.Variety, mega));
    Assert.Equal(FormCategory.Mega, exception.Data["AttemptedCategory"]);
  }

  [Fact(DisplayName = "It should throw InvalidPokemonGenderException when the gender is not allowed.")]
  public void Given_GenderlessVariety_When_Create_Then_InvalidPokemonGenderException()
  {
    Variety genderless = new VarietyBuilder(Faker)
      .WithWorld(Catalog.World)
      .WithSpecies(Catalog.Species)
      .WithKey("genderless-bulb")
      .WithIsDefault(false)
      .WithGenderRatio(null)
      .Build();
    Form form = new FormBuilder(Faker)
      .WithWorld(Catalog.World)
      .WithVariety(genderless)
      .WithAbilities(Catalog.Ability)
      .WithKey("genderless-bulb-form")
      .Build();

    InvalidPokemonGenderException exception = Assert.Throws<InvalidPokemonGenderException>(
      () => new Specimen(Catalog.Randomizer, PokemonId.NewId(Catalog.World.Id), Catalog.Species, genderless, form, gender: Gender.Male));
    Assert.Equal(genderless.EntityId, exception.Data["VarietyId"]);
    Assert.Equal(Gender.Male, exception.Data["AttemptedGender"]);
  }

  [Fact(DisplayName = "It should throw InvalidOperationException when egg cycles and experience are both set.")]
  public void Given_EggAndExperience_When_Create_Then_InvalidOperationException()
  {
    Assert.Throws<InvalidOperationException>(
      () => new Specimen(Catalog.Randomizer, PokemonId.NewId(Catalog.World.Id), Catalog.Species, Catalog.Variety, Catalog.Form, eggCycles: 5, experience: 100));
  }
}
