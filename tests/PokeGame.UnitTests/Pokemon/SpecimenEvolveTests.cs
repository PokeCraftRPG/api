using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Regions;

namespace PokeGame.Pokemon;

public class SpecimenEvolveTests : UnitTests
{
  [Fact(DisplayName = "It should evolve a Pokémon that meets a level-up evolution.")]
  public void Given_LevelUpRequirementsMet_When_Evolve_Then_Evolved()
  {
    Specimen pokemon = CreateOwnedAtLevel(16);
    pokemon.SetNickname(new Name("Bulba"), Catalog.World.OwnerId.ActorId);
    int previousVitality = pokemon.Vitality;
    PokemonStatistics previous = new(pokemon);

    Evolution evolution = CreateEvolution();
    evolution.SetConditions(new Level(16), friendship: false, gender: null, item: null, move: null, Catalog.PalletTown, TimeOfDay.Day);

    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety, Catalog.PalletTown, TimeOfDay.Day);

    AssertEvolved(pokemon, previous, previousVitality);
    Assert.Equal("Bulba", pokemon.Nickname?.Value);
    Assert.Equal(Catalog.Red.Id, pokemon.OriginalTrainerId);
    PokemonEvolved @event = pokemon.LastChange<PokemonEvolved>();
    Assert.False(@event.ConsumeHeldItem);

    Specimen replayed = pokemon.Replay();
    Assert.Equal(Catalog.CharmanderForm.Id, replayed.FormId);
    Assert.Equal(Catalog.CharmanderVariety.Id, replayed.VarietyId);
    Assert.Equal(Catalog.CharmanderSpecies.Id, replayed.SpeciesId);
  }

  [Fact(DisplayName = "It should consume the held item when a non-item evolution requires it.")]
  public void Given_HeldItemRequired_When_Evolve_Then_HeldItemConsumed()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    pokemon.SetHeldItem(Catalog.Potion);

    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: false, gender: null, Catalog.Potion, move: null, location: null, timeOfDay: null);

    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    Assert.Null(pokemon.HeldItemId);
    Assert.True(pokemon.LastChange<PokemonEvolved>().ConsumeHeldItem);
  }

  [Fact(DisplayName = "It should evolve by item without requiring the Pokémon to hold it.")]
  public void Given_ItemUsed_When_Evolve_Then_HeldItemUnchanged()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    pokemon.SetHeldItem(Catalog.Potion);

    Evolution evolution = new(EvolutionId.NewId(Catalog.World.Id), Catalog.Form, Catalog.CharmanderForm, EvolutionTrigger.ItemUsed, Catalog.MasterBall);

    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    Assert.Equal(Catalog.Potion.Id, pokemon.HeldItemId);
    Assert.False(pokemon.LastChange<PokemonEvolved>().ConsumeHeldItem);
  }

  [Fact(DisplayName = "It should evolve after a trade.")]
  public void Given_TradedPokemon_When_Evolve_Then_Evolved()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Specimen other = Catalog.CreateOwnedPokemon(Catalog.Blue, "other");
    pokemon.Trade(other, Catalog.PokemonCenter);

    Evolution evolution = CreateEvolution(EvolutionTrigger.Traded);
    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    Assert.Equal(Catalog.CharmanderForm.Id, pokemon.FormId);
    Assert.Equal(Catalog.Blue.Id, pokemon.Ownership!.TrainerId);
    Assert.Equal(Catalog.Red.Id, pokemon.OriginalTrainerId);
  }

  [Fact(DisplayName = "It should throw PokemonEggCannotEvolveException when the Pokémon is an egg.")]
  public void Given_Egg_When_Evolve_Then_PokemonEggCannotEvolveException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, "egg", eggCycles: 5);
    Evolution evolution = CreateEvolution();

    PokemonEggCannotEvolveException exception = Assert.Throws<PokemonEggCannotEvolveException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    Assert.Equal(Catalog.World.Id.EntityId, exception.Data["WorldId"]);
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
    Assert.Equal((byte)5, exception.Data["EggCycles"]);
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when the Pokémon is wild.")]
  public void Given_WildPokemon_When_Evolve_Then_PokemonHasNoOwnerException()
  {
    Specimen pokemon = Catalog.CreatePokemon();
    Evolution evolution = CreateEvolution();

    PokemonHasNoOwnerException exception = Assert.Throws<PokemonHasNoOwnerException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    Assert.Equal(Catalog.World.Id.EntityId, exception.Data["WorldId"]);
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw InvalidEvolutionSourceException when the form does not match.")]
  public void Given_WrongSourceForm_When_Evolve_Then_InvalidEvolutionSourceException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = new(EvolutionId.NewId(Catalog.World.Id), Catalog.CharmanderForm, Catalog.Form, EvolutionTrigger.LeveledUp);

    InvalidEvolutionSourceException exception = Assert.Throws<InvalidEvolutionSourceException>(
      () => pokemon.Evolve(evolution, Catalog.Form, Catalog.Variety));
    Assert.Equal(Catalog.World.Id.EntityId, exception.Data["WorldId"]);
    Assert.Equal(pokemon.EntityId, exception.Data["PokemonId"]);
    Assert.Equal(evolution.EntityId, exception.Data["EvolutionId"]);
    Assert.Equal(Catalog.CharmanderForm.EntityId, exception.Data["ExpectedFormId"]);
    Assert.Equal(Catalog.Form.EntityId, exception.Data["AttemptedFormId"]);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the level is too low.")]
  public void Given_LevelTooLow_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();
    evolution.SetConditions(new Level(16), friendship: false, gender: null, item: null, move: null, location: null, timeOfDay: null);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.Level);
    Assert.Equal(16, failure.Required);
    Assert.Equal(1, failure.Actual);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the Pokémon has not been traded.")]
  public void Given_NotTraded_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution(EvolutionTrigger.Traded);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.Trade);
    Assert.Equal(true, failure.Required);
    Assert.Equal(false, failure.Actual);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the gender does not match.")]
  public void Given_WrongGender_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, gender: Gender.Male);
    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: false, Gender.Female, item: null, move: null, location: null, timeOfDay: null);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.Gender);
    Assert.Equal(Gender.Female, failure.Required);
    Assert.Equal(Gender.Male, failure.Actual);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when friendship is too low.")]
  public void Given_LowFriendship_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: true, gender: null, item: null, move: null, location: null, timeOfDay: null);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.Friendship);
    Assert.Equal(Friendship.HighValue, failure.Required);
    Assert.Equal(pokemon.Friendship.Value, failure.Actual);
  }

  [Fact(DisplayName = "It should evolve when friendship is high.")]
  public void Given_HighFriendship_When_Evolve_Then_Evolved()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    pokemon.SetStatus(pokemon.Vitality, pokemon.Stamina, condition: null, new Friendship(Friendship.HighValue));

    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: true, gender: null, item: null, move: null, location: null, timeOfDay: null);

    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    Assert.Equal(Catalog.CharmanderForm.Id, pokemon.FormId);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the held item does not match.")]
  public void Given_WrongHeldItem_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: false, gender: null, Catalog.Potion, move: null, location: null, timeOfDay: null);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.HeldItem);
    Assert.Equal(Catalog.Potion.EntityId, failure.Required);
    Assert.Null(failure.Actual);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the required move is not in the moveset.")]
  public void Given_UnknownMove_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Move tackle = MoveBuilder.Tackle(Faker, Catalog.World);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: false, gender: null, item: null, tackle, location: null, timeOfDay: null);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.KnownMove);
    Assert.Equal(tackle.Id, failure.Required);
    Assert.Null(failure.Actual);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the required move is only in the movepool.")]
  public void Given_RequiredMoveOnlyInMovepool_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Move tackle = MoveBuilder.Tackle(Faker, Catalog.World);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    for (int index = 0; index < Specimen.MoveLimit; index++)
    {
      Move filler = new MoveBuilder(Faker)
        .WithWorld(Catalog.World)
        .WithKey($"filler-{index}")
        .WithName($"Filler {index}")
        .Build();
      pokemon.LearnMove(filler.Id, LearningMethod.LevelUp);
    }
    pokemon.LearnMove(tackle.Id, LearningMethod.LevelUp);
    Assert.Contains(tackle.Id, pokemon.Movepool.Keys);
    Assert.DoesNotContain(tackle.Id, pokemon.Moveset);

    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: false, gender: null, item: null, tackle, location: null, timeOfDay: null);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.KnownMove);
    Assert.Equal(tackle.Id, failure.Required);
    Assert.Null(failure.Actual);
  }

  [Fact(DisplayName = "It should evolve when the required move is in the moveset.")]
  public void Given_RequiredMoveInMoveset_When_Evolve_Then_Evolved()
  {
    Move tackle = MoveBuilder.Tackle(Faker, Catalog.World);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    pokemon.LearnMove(tackle.Id, LearningMethod.LevelUp);
    Assert.Contains(tackle.Id, pokemon.Moveset);

    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: false, gender: null, item: null, tackle, location: null, timeOfDay: null);

    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    Assert.Equal(Catalog.CharmanderForm.Id, pokemon.FormId);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the location does not match.")]
  public void Given_WrongLocation_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: false, gender: null, item: null, move: null, Catalog.PalletTown, timeOfDay: null);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety, Catalog.CeruleanCity));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.Location);
    Assert.Equal(Catalog.PalletTown.Value, failure.Required);
    Assert.Equal(Catalog.CeruleanCity.Value, failure.Actual);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the time of day does not match.")]
  public void Given_WrongTimeOfDay_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();
    evolution.SetConditions(level: null, friendship: false, gender: null, item: null, move: null, location: null, TimeOfDay.Night);

    EvolutionRequirementsNotMetException exception = Assert.Throws<EvolutionRequirementsNotMetException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety, timeOfDay: TimeOfDay.Day));
    EvolutionConditionFailure failure = AssertSingleFailure(exception, EvolutionCondition.TimeOfDay);
    Assert.Equal(TimeOfDay.Night, failure.Required);
    Assert.Equal(TimeOfDay.Day, failure.Actual);
  }

  [Fact(DisplayName = "It should throw ArgumentOutOfRangeException when the time of day is invalid.")]
  public void Given_InvalidTimeOfDay_When_Evolve_Then_ArgumentOutOfRangeException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();

    Assert.Throws<ArgumentOutOfRangeException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety, timeOfDay: (TimeOfDay)99));
  }

  [Fact(DisplayName = "It should throw WorldMismatchException when the evolution belongs to another world.")]
  public void Given_DifferentWorld_When_Evolve_Then_WorldMismatchException()
  {
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    DomainCatalog other = new(Faker);
    Evolution evolution = new(EvolutionId.NewId(other.World.Id), other.Form, other.CharmanderForm, EvolutionTrigger.LeveledUp);

    Assert.Throws<WorldMismatchException>(
      () => pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety));
  }

  private Evolution CreateEvolution(EvolutionTrigger trigger = EvolutionTrigger.LeveledUp)
    => new(EvolutionId.NewId(Catalog.World.Id), Catalog.Form, Catalog.CharmanderForm, trigger);

  private Specimen CreateOwnedAtLevel(int level)
  {
    int experience = level <= 1 ? 0 : ExperienceTable.GetThreshold(Catalog.Species.GrowthRate, level - 1);
    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red, experience: experience);
    Assert.Equal(level, pokemon.Level);
    return pokemon;
  }

  private void AssertEvolved(Specimen pokemon, PokemonStatistics previous, int previousVitality)
  {
    Assert.Equal(Catalog.CharmanderSpecies.Id, pokemon.SpeciesId);
    Assert.Equal(Catalog.CharmanderVariety.Id, pokemon.VarietyId);
    Assert.Equal(Catalog.CharmanderForm.Id, pokemon.FormId);

    PokemonStatistics changed = new(pokemon);
    int delta = changed.HP - previous.HP;
    Assert.Equal(Math.Clamp(previousVitality + delta, 0, changed.HP), pokemon.Vitality);
    Assert.Equal(Math.Clamp(previousVitality + delta, 0, changed.HP), pokemon.Stamina);
  }

  private static EvolutionConditionFailure AssertSingleFailure(EvolutionRequirementsNotMetException exception, EvolutionCondition condition)
  {
    IReadOnlyList<EvolutionConditionFailure> failures = Assert.IsAssignableFrom<IReadOnlyList<EvolutionConditionFailure>>(exception.Data["Failures"]);
    EvolutionConditionFailure failure = Assert.Single(failures);
    Assert.Equal(condition, failure.Condition);
    return failure;
  }
}
