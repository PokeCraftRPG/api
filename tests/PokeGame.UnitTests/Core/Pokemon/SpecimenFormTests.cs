using Bogus;
using PokeGame.Builders;
using PokeGame.Core.Forms;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Pokemon;

[Trait(Traits.Category, Categories.Unit)]
public class SpecimenFormTests
{
  private readonly Faker _faker = new();

  private readonly World _world;
  private readonly PokemonSpecies _species;
  private readonly Variety _variety;
  private readonly Form _form;

  public SpecimenFormTests()
  {
    _world = new WorldBuilder(_faker).Build();

    _species = SpeciesBuilder.Pikachu(_faker, _world);
    _variety = VarietyBuilder.Pikachu(_faker, _world, _species);
    _form = FormBuilder.Pikachu(_faker, _world, _variety);
  }

  [Fact(DisplayName = "ChangeForm: it should throw InvalidFormException when the form does not belong to the variety.")]
  public void Given_InvalidForm_When_ChangeForm_Then_InvalidFormException()
  {
    Specimen specimen = new SpecimenBuilder(_faker).WithWorld(_world).Is(_species, _variety, _form).Build();
    Form target = FormBuilder.Raichu(_faker, _world);

    var exception = Assert.Throws<InvalidFormException>(() => specimen.ChangeForm(target, _world.OwnerId));
    Assert.Equal(_world.Id.ToGuid(), exception.WorldId);
    Assert.Equal(_variety.EntityId, exception.VarietyId);
    Assert.Equal(_form.EntityId, exception.SourceId);
    Assert.Equal(target.EntityId, exception.TargetId);
    Assert.Equal(specimen.EntityId, exception.PokemonId);
  }

  [Fact(DisplayName = "ChangeForm: it should throw WorldMismatchException when the form is not in the same world.")]
  public void Given_WorldMismatch_When_ChangeForm_Then_WorldMismatchException()
  {
    Specimen specimen = new SpecimenBuilder().Build();
    var exception = Assert.Throws<WorldMismatchException>(() => specimen.ChangeForm(_form, _world.OwnerId));
    Assert.Equal(specimen.Id.GetEntity(), exception.Expected);
    Assert.Equal(_form.Id.GetEntity(), Assert.Single(exception.Mismatched));
    Assert.Equal("form", exception.ParamName);
  }
}
