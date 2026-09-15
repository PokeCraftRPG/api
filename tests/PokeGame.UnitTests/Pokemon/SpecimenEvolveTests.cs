using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Regions;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

public class SpecimenEvolveTests : UnitTests
{
  [Fact(DisplayName = "It should evolve a Pokémon that meets a level-up evolution.")]
  public void Given_LevelUpRequirementsMet_When_Evolve_Then_Evolved()
  {
    Specimen pokemon = CreateOwnedAtLevel(16);
    pokemon.SetNickname(new Name("Bulba"), Catalog.World.OwnerId.ActorId);
    int previousVitality = pokemon.Vitality;
    int previousStamina = pokemon.Stamina;
    PokemonStatistics previous = CreateStatistics(pokemon);

    Evolution evolution = CreateEvolution();
    evolution.SetConditions(new Level(16), friendship: false, gender: null, item: null, move: null, Catalog.PalletTown, TimeOfDay.Day);

    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety, Catalog.PalletTown, TimeOfDay.Day);

    AssertEvolved(pokemon, previous, previousVitality, previousStamina);
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

  [Fact(DisplayName = "It should learn evolution moves into the movepool and moveset when slots remain.")]
  public void Given_EvolutionMovesAndRoom_When_Evolve_Then_MovesAddedToMoveset()
  {
    Move ember = MoveBuilder.Ember(Faker, Catalog.World);
    Move tackle = MoveBuilder.Tackle(Faker, Catalog.World);
    Catalog.CharmanderVariety.AddMove(new VarietyMove(ember.Id, LearningMethod.Evolution));
    Catalog.CharmanderVariety.AddMove(new VarietyMove(tackle.Id, LearningMethod.Evolution));
    Move[] expectedOrder = new[] { ember, tackle }.OrderBy(move => move.Id.Value).ToArray();

    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();

    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    PokemonEvolved @event = pokemon.LastChange<PokemonEvolved>();
    Assert.Equal(expectedOrder.Select(move => move.Id), @event.Moves.Select(move => move.MoveId));
    Assert.All(@event.Moves, move => Assert.True(move.IsInMoveset));

    Assert.Equal(expectedOrder.Select(move => move.Id), pokemon.Moveset);
    foreach (Move move in expectedOrder)
    {
      AssertEvolutionMove(pokemon.Movepool[move.Id], pokemon.Level);
    }
  }

  [Fact(DisplayName = "It should add evolution moves only to the movepool when the moveset is full.")]
  public void Given_FullMoveset_When_Evolve_Then_EvolutionMovesOnlyInMovepool()
  {
    Move evolutionMove = MoveBuilder.Ember(Faker, Catalog.World);
    Catalog.CharmanderVariety.AddMove(new VarietyMove(evolutionMove.Id, LearningMethod.Evolution));

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
    Assert.Equal(Specimen.MoveLimit, pokemon.Moveset.Count);

    Evolution evolution = CreateEvolution();
    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    PokemonEvolved @event = pokemon.LastChange<PokemonEvolved>();
    LearnedMove learned = Assert.Single(@event.Moves);
    Assert.Equal(evolutionMove.Id, learned.MoveId);
    Assert.False(learned.IsInMoveset);

    Assert.DoesNotContain(evolutionMove.Id, pokemon.Moveset);
    AssertEvolutionMove(pokemon.Movepool[evolutionMove.Id], pokemon.Level);
  }

  [Fact(DisplayName = "It should fill remaining moveset slots then put extra evolution moves in the movepool only.")]
  public void Given_OneMovesetSlot_When_Evolve_Then_FirstEvolutionMoveInMoveset()
  {
    Move first = MoveBuilder.Ember(Faker, Catalog.World);
    Move second = MoveBuilder.Tackle(Faker, Catalog.World);
    Catalog.CharmanderVariety.AddMove(new VarietyMove(first.Id, LearningMethod.Evolution));
    Catalog.CharmanderVariety.AddMove(new VarietyMove(second.Id, LearningMethod.Evolution));
    Move[] expectedOrder = new[] { first, second }.OrderBy(move => move.Id.Value).ToArray();

    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    for (int index = 0; index < Specimen.MoveLimit - 1; index++)
    {
      Move filler = new MoveBuilder(Faker)
        .WithWorld(Catalog.World)
        .WithKey($"slot-filler-{index}")
        .WithName($"Slot Filler {index}")
        .Build();
      pokemon.LearnMove(filler.Id, LearningMethod.LevelUp);
    }

    Evolution evolution = CreateEvolution();
    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    Assert.Contains(expectedOrder[0].Id, pokemon.Moveset);
    Assert.DoesNotContain(expectedOrder[1].Id, pokemon.Moveset);
    AssertEvolutionMove(pokemon.Movepool[expectedOrder[0].Id], pokemon.Level);
    AssertEvolutionMove(pokemon.Movepool[expectedOrder[1].Id], pokemon.Level);

    PokemonEvolved @event = pokemon.LastChange<PokemonEvolved>();
    Assert.Equal(2, @event.Moves.Count);
    Assert.True(@event.Moves.ElementAt(0).IsInMoveset);
    Assert.False(@event.Moves.ElementAt(1).IsInMoveset);
  }

  [Fact(DisplayName = "It should not re-learn an evolution move already in the movepool.")]
  public void Given_AlreadyKnownEvolutionMove_When_Evolve_Then_NotAddedAgain()
  {
    Move ember = MoveBuilder.Ember(Faker, Catalog.World);
    Catalog.CharmanderVariety.AddMove(new VarietyMove(ember.Id, LearningMethod.Evolution));

    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    pokemon.LearnMove(ember.Id, LearningMethod.LevelUp);

    Evolution evolution = CreateEvolution();
    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    PokemonEvolved @event = pokemon.LastChange<PokemonEvolved>();
    Assert.Empty(@event.Moves);
    Assert.Equal(LearningMethod.LevelUp, pokemon.Movepool[ember.Id].LearningMethod);
  }

  [Fact(DisplayName = "It should ignore Level-Up variety moves when evolving.")]
  public void Given_LevelUpTargetMoves_When_Evolve_Then_NotLearned()
  {
    Move tackle = MoveBuilder.Tackle(Faker, Catalog.World);
    Catalog.CharmanderVariety.AddMove(new VarietyMove(tackle.Id, LearningMethod.LevelUp, new Level(1)));

    Specimen pokemon = Catalog.CreateOwnedPokemon(Catalog.Red);
    Evolution evolution = CreateEvolution();

    pokemon.Evolve(evolution, Catalog.CharmanderForm, Catalog.CharmanderVariety);

    Assert.Empty(pokemon.LastChange<PokemonEvolved>().Moves);
    Assert.DoesNotContain(tackle.Id, pokemon.Movepool.Keys);
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

  private void AssertEvolved(Specimen pokemon, PokemonStatistics previous, int previousVitality, int previousStamina)
  {
    Assert.Equal(Catalog.CharmanderSpecies.Id, pokemon.SpeciesId);
    Assert.Equal(Catalog.CharmanderVariety.Id, pokemon.VarietyId);
    Assert.Equal(Catalog.CharmanderForm.Id, pokemon.FormId);

    PokemonStatistics changed = CreateStatistics(pokemon);
    int vitalityDelta = changed.Vitality - previous.Vitality;
    int staminaDelta = changed.Stamina - previous.Stamina;
    Assert.Equal(Math.Clamp(previousVitality + vitalityDelta, 0, changed.Vitality), pokemon.Vitality);
    Assert.Equal(Math.Clamp(previousStamina + staminaDelta, 0, changed.Stamina), pokemon.Stamina);
  }

  private static PokemonStatistics CreateStatistics(Specimen pokemon) => new(
    pokemon.BaseStatistics,
    pokemon.IndividualValues,
    new EffortValues(pokemon.Skills),
    pokemon.Level,
    pokemon.Nature);

  private static void AssertEvolutionMove(PokemonMove move, int learnedAtLevel)
  {
    Assert.Equal(learnedAtLevel, move.LearnedAtLevel.Value);
    Assert.Equal(LearningMethod.Evolution, move.LearningMethod);
    Assert.False(move.IsMastered);
    Assert.Equal(0, move.PowerPointUpgrades);
  }

  private static EvolutionConditionFailure AssertSingleFailure(EvolutionRequirementsNotMetException exception, EvolutionCondition condition)
  {
    IReadOnlyList<EvolutionConditionFailure> failures = Assert.IsAssignableFrom<IReadOnlyList<EvolutionConditionFailure>>(exception.Data["Failures"]);
    EvolutionConditionFailure failure = Assert.Single(failures);
    Assert.Equal(condition, failure.Condition);
    return failure;
  }
}
