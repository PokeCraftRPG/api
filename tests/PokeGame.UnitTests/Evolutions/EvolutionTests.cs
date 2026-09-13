using PokeGame.Builders;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Forms;
using PokeGame.Core.Varieties;

namespace PokeGame.Evolutions;

public class EvolutionTests : UnitTests
{
  [Fact(DisplayName = "It should create an evolution between forms of different species.")]
  public void Given_DifferentSpecies_When_ctor_Then_Created()
  {
    Evolution evolution = new(EvolutionId.NewId(Catalog.World.Id), Catalog.Form, Catalog.CharmanderForm, EvolutionTrigger.LeveledUp);

    Assert.Equal(Catalog.Form.Id, evolution.SourceId);
    Assert.Equal(Catalog.CharmanderForm.Id, evolution.TargetId);
    Assert.Equal(EvolutionTrigger.LeveledUp, evolution.Trigger);
  }

  [Fact(DisplayName = "It should throw InvalidEvolutionFormsException when the source and target are the same form.")]
  public void Given_SameForm_When_ctor_Then_InvalidEvolutionFormsException()
  {
    InvalidEvolutionFormsException exception = Assert.Throws<InvalidEvolutionFormsException>(
      () => new Evolution(EvolutionId.NewId(Catalog.World.Id), Catalog.Form, Catalog.Form, EvolutionTrigger.LeveledUp));

    AssertInvalidEvolutionForms(exception, Catalog.Form, Catalog.Form);
  }

  [Fact(DisplayName = "It should throw InvalidEvolutionFormsException when the forms belong to the same species.")]
  public void Given_SameSpecies_When_ctor_Then_InvalidEvolutionFormsException()
  {
    Variety otherVariety = new VarietyBuilder(Faker)
      .WithWorld(Catalog.World)
      .WithSpecies(Catalog.Species)
      .WithKey("ivysaur")
      .WithName("Ivysaur")
      .WithIsDefault(false)
      .Build();
    Form otherForm = new FormBuilder(Faker)
      .WithWorld(Catalog.World)
      .WithVariety(otherVariety)
      .WithAbilities(Catalog.Ability)
      .WithKey("ivysaur")
      .WithName("Ivysaur")
      .Build();

    InvalidEvolutionFormsException exception = Assert.Throws<InvalidEvolutionFormsException>(
      () => new Evolution(EvolutionId.NewId(Catalog.World.Id), Catalog.Form, otherForm, EvolutionTrigger.LeveledUp));

    AssertInvalidEvolutionForms(exception, Catalog.Form, otherForm);
  }

  private static void AssertInvalidEvolutionForms(InvalidEvolutionFormsException exception, Form source, Form target)
  {
    Assert.Equal(source.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(source.EntityId, exception.Data["SourceFormId"]);
    Assert.Equal(source.SpeciesId.EntityId, exception.Data["SourceSpeciesId"]);
    Assert.Equal(target.EntityId, exception.Data["TargetFormId"]);
    Assert.Equal(target.SpeciesId.EntityId, exception.Data["TargetSpeciesId"]);
  }
}
