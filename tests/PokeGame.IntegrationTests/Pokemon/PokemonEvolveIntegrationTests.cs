using Logitar.EventSourcing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Forms;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

[Trait(Traits.Category, Categories.Integration)]
public class PokemonEvolveIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IFormRepository _formRepository;
  private readonly IInventoryService _inventoryService;
  private readonly IItemRepository _itemRepository;
  private readonly IMoveRepository _moveRepository;
  private readonly IPokemonService _pokemonService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ITrainerRepository _trainerRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Form _source = null!;
  private Form _target = null!;
  private Item _leafStone = null!;
  private Item _masterBall = null!;
  private Trainer _trainer = null!;

  public PokemonEvolveIntegrationTests()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _evolutionRepository = ServiceProvider.GetRequiredService<IEvolutionRepository>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _inventoryService = ServiceProvider.GetRequiredService<IInventoryService>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _source = await CreateFormAsync("bulbasaur");
    _target = await CreateFormAsync("charmander");

    _trainer = TrainerBuilder.Red(Faker, Context.World);
    await _trainerRepository.SaveAsync(_trainer);

    _masterBall = ItemBuilder.MasterBall(Faker, Context.World);
    await _itemRepository.SaveAsync(_masterBall);

    _leafStone = new ItemBuilder(Faker)
      .WithWorld(Context.World)
      .WithKey("leaf-stone")
      .WithName("Leaf Stone")
      .Build();
    await _itemRepository.SaveAsync(_leafStone);
  }

  [Fact(DisplayName = "It should evolve a Pokémon by leveling up.")]
  public async Task Given_LevelUpEvolution_When_Evolve_Then_Evolved()
  {
    Evolution evolution = await CreateEvolutionAsync(EvolutionTrigger.LeveledUp, new Level(16), location: "Route 1", timeOfDay: TimeOfDay.Day);
    PokemonDto created = await CreateOwnedPokemonAsync("starter-bulbasaur", experience: ExperienceForLevel(16));

    EvolvePokemonPayload payload = new()
    {
      EvolutionId = evolution.EntityId,
      Location = " Route 1 ",
      TimeOfDay = TimeOfDay.Day
    };

    PokemonDto? pokemon = await _pokemonService.EvolveAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    AssertEvolved(created, pokemon);
    AssertPokemonAcquired(pokemon, _trainer);

    PokemonDto? read = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(read);
    Assert.Equal(_target.EntityId, read.Form.Id);
    Assert.Equal(_target.VarietyId.EntityId, read.Form.Variety.Id);
  }

  [Fact(DisplayName = "It should evolve a Pokémon by using an item and consume it from the inventory.")]
  public async Task Given_ItemUsed_When_Evolve_Then_EvolvedAndItemConsumed()
  {
    Evolution evolution = new(EvolutionId.NewId(Context.WorldId), _source, _target, EvolutionTrigger.ItemUsed, _leafStone, Context.ActorId);
    await _evolutionRepository.SaveAsync(evolution);
    await _inventoryService.SetAsync(_trainer.EntityId, _leafStone.EntityId, new SetInventoryItemPayload { Quantity = 2 });

    PokemonDto created = await CreateOwnedPokemonAsync("stone-evolution");
    PokemonDto? pokemon = await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId });
    Assert.NotNull(pokemon);
    AssertEvolved(created, pokemon);

    InventoryItemDto? inventoryItem = await _inventoryService.ReadAsync(_trainer.EntityId, _leafStone.EntityId);
    Assert.NotNull(inventoryItem);
    Assert.Equal(1, inventoryItem.Quantity);
  }

  [Fact(DisplayName = "It should evolve a Pokémon after it has been traded.")]
  public async Task Given_TradedPokemon_When_Evolve_Then_Evolved()
  {
    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    Evolution evolution = await CreateEvolutionAsync(EvolutionTrigger.Traded);
    PokemonDto source = await CreateOwnedPokemonAsync("trade-source");
    PokemonDto target = await CreateOwnedPokemonAsync("trade-target", blue);

    await _pokemonService.TradeAsync(new TradePokemonPayload
    {
      PokemonIds = [source.Id, target.Id],
      Location = "Pokémon Center"
    });

    PokemonDto? pokemon = await _pokemonService.EvolveAsync(source.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId });
    Assert.NotNull(pokemon);
    Assert.Equal(_target.EntityId, pokemon.Form.Id);
    Assert.Equal(blue.EntityId, pokemon.Ownership!.Trainer.Id);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer!.Id);
    AssertPokemonAcquired(pokemon, blue);
  }

  [Fact(DisplayName = "It should consume the held item when a level-up evolution requires it.")]
  public async Task Given_HeldItemEvolution_When_Evolve_Then_HeldItemConsumed()
  {
    Evolution evolution = new(EvolutionId.NewId(Context.WorldId), _source, _target, EvolutionTrigger.LeveledUp, actorId: Context.ActorId);
    evolution.SetConditions(level: null, friendship: false, gender: null, _leafStone, move: null, location: null, timeOfDay: null, Context.ActorId);
    await _evolutionRepository.SaveAsync(evolution);

    PokemonDto created = await CreateOwnedPokemonAsync("held-item-evolution");
    PokemonDto? equipped = await _pokemonService.UpdateAsync(created.Id, new UpdatePokemonPayload
    {
      HeldItemId = new Optional<Guid?>(_leafStone.EntityId)
    });
    Assert.NotNull(equipped);
    Assert.Equal(_leafStone.EntityId, equipped.HeldItem!.Id);

    PokemonDto? pokemon = await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId });
    Assert.NotNull(pokemon);
    Assert.Equal(_target.EntityId, pokemon.Form.Id);
    Assert.Null(pokemon.HeldItem);
  }

  [Fact(DisplayName = "It should return null when the Pokémon was not found.")]
  public async Task Given_NotFound_When_Evolve_Then_NullReturned()
  {
    Evolution evolution = await CreateEvolutionAsync();
    Assert.Null(await _pokemonService.EvolveAsync(Guid.NewGuid(), new EvolvePokemonPayload { EvolutionId = evolution.EntityId }));
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the evolution does not exist.")]
  public async Task Given_MissingEvolution_When_Evolve_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreateOwnedPokemonAsync("missing-evolution");
    Guid missingEvolutionId = Guid.NewGuid();

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = missingEvolutionId }));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Evolution.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingEvolutionId, exception.Data["EntityId"]);
    Assert.Equal(nameof(EvolvePokemonPayload.EvolutionId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_Evolve_Then_InvalidCommandException()
  {
    PokemonDto created = await CreateOwnedPokemonAsync("invalid-payload");
    EvolvePokemonPayload payload = new()
    {
      EvolutionId = Guid.NewGuid(),
      TimeOfDay = (TimeOfDay)99
    };

    await Assert.ThrowsAsync<InvalidCommandException>(async () => await _pokemonService.EvolveAsync(created.Id, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when evolving a Pokémon.")]
  public async Task Given_NotAllowed_When_Evolve_Then_PermissionDeniedException()
  {
    Evolution evolution = await CreateEvolutionAsync();
    PokemonDto created = await CreateOwnedPokemonAsync("denied-evolve");
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId }));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Evolve", exception.Data["Action"]);
    Assert.Equal(new Entity(Specimen.EntityKind, created.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when the Pokémon is wild.")]
  public async Task Given_WildPokemon_When_Evolve_Then_PokemonHasNoOwnerException()
  {
    Evolution evolution = await CreateEvolutionAsync();
    PokemonDto created = await CreatePokemonAsync("wild");

    PokemonHasNoOwnerException exception = await Assert.ThrowsAsync<PokemonHasNoOwnerException>(
      async () => await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId }));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw PokemonEggCannotEvolveException when the Pokémon is an egg.")]
  public async Task Given_Egg_When_Evolve_Then_PokemonEggCannotEvolveException()
  {
    Evolution evolution = await CreateEvolutionAsync();
    PokemonDto created = await CreateOwnedPokemonAsync("egg", eggCycles: 5);

    PokemonEggCannotEvolveException exception = await Assert.ThrowsAsync<PokemonEggCannotEvolveException>(
      async () => await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId }));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal((byte)5, exception.Data["EggCycles"]);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the conditions are not met.")]
  public async Task Given_RequirementsNotMet_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Evolution evolution = new(EvolutionId.NewId(Context.WorldId), _source, _target, EvolutionTrigger.LeveledUp, actorId: Context.ActorId);
    evolution.SetConditions(level: null, friendship: false, Gender.Female, item: null, move: null, location: null, timeOfDay: null, Context.ActorId);
    await _evolutionRepository.SaveAsync(evolution);

    PokemonDto created = await CreateOwnedPokemonAsync("male", gender: Gender.Male);

    EvolutionRequirementsNotMetException exception = await Assert.ThrowsAsync<EvolutionRequirementsNotMetException>(
      async () => await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId }));
    IReadOnlyList<EvolutionConditionFailure> failures = Assert.IsAssignableFrom<IReadOnlyList<EvolutionConditionFailure>>(exception.Data["Failures"]);
    EvolutionConditionFailure failure = Assert.Single(failures);
    Assert.Equal(EvolutionCondition.Gender, failure.Condition);
  }

  [Fact(DisplayName = "It should throw EvolutionRequirementsNotMetException when the required move is not known.")]
  public async Task Given_UnknownMove_When_Evolve_Then_EvolutionRequirementsNotMetException()
  {
    Move tackle = MoveBuilder.Tackle(Faker, Context.World);
    await _moveRepository.SaveAsync(tackle);

    Evolution evolution = new(EvolutionId.NewId(Context.WorldId), _source, _target, EvolutionTrigger.LeveledUp, actorId: Context.ActorId);
    evolution.SetConditions(level: null, friendship: false, gender: null, item: null, tackle, location: null, timeOfDay: null, Context.ActorId);
    await _evolutionRepository.SaveAsync(evolution);

    PokemonDto created = await CreateOwnedPokemonAsync("no-tackle");

    EvolutionRequirementsNotMetException exception = await Assert.ThrowsAsync<EvolutionRequirementsNotMetException>(
      async () => await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId }));
    IReadOnlyList<EvolutionConditionFailure> failures = Assert.IsAssignableFrom<IReadOnlyList<EvolutionConditionFailure>>(exception.Data["Failures"]);
    EvolutionConditionFailure failure = Assert.Single(failures);
    Assert.Equal(EvolutionCondition.KnownMove, failure.Condition);
    Assert.Equal(tackle.Id, failure.Required);
    Assert.Null(failure.Actual);
  }

  [Fact(DisplayName = "It should evolve when the required move is in the moveset.")]
  public async Task Given_RequiredMoveInMoveset_When_Evolve_Then_Evolved()
  {
    Move tackle = MoveBuilder.Tackle(Faker, Context.World);
    await _moveRepository.SaveAsync(tackle);

    Variety variety = await _varietyRepository.LoadAsync(_source.VarietyId) ?? throw new InvalidOperationException("The source variety was not found.");
    variety.AddMove(new VarietyMove(tackle.Id, LearningMethod.LevelUp, new Level(1)), Context.ActorId);
    await _varietyRepository.SaveAsync(variety);

    Evolution evolution = new(EvolutionId.NewId(Context.WorldId), _source, _target, EvolutionTrigger.LeveledUp, actorId: Context.ActorId);
    evolution.SetConditions(level: null, friendship: false, gender: null, item: null, tackle, location: null, timeOfDay: null, Context.ActorId);
    await _evolutionRepository.SaveAsync(evolution);

    PokemonDto created = await CreateOwnedPokemonAsync("knows-tackle");
    Assert.Contains(created.Moves, move => move.Move.Id == tackle.EntityId && move.Slot is not null);

    PokemonDto? pokemon = await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId });
    Assert.NotNull(pokemon);
    Assert.Equal(_target.EntityId, pokemon.Form.Id);
  }

  [Fact(DisplayName = "It should throw InvalidEvolutionSourceException when the Pokémon form does not match.")]
  public async Task Given_WrongSourceForm_When_Evolve_Then_InvalidEvolutionSourceException()
  {
    Evolution evolution = await CreateEvolutionAsync();
    PokemonDto created = await CreatePokemonAsync("already-charmander", _target);
    await _pokemonService.ReceiveAsync(created.Id, CreateReceivePayload());

    InvalidEvolutionSourceException exception = await Assert.ThrowsAsync<InvalidEvolutionSourceException>(
      async () => await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId }));
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal(evolution.EntityId, exception.Data["EvolutionId"]);
    Assert.Equal(_source.EntityId, exception.Data["ExpectedFormId"]);
    Assert.Equal(_target.EntityId, exception.Data["AttemptedFormId"]);
  }

  [Fact(DisplayName = "It should throw InventoryQuantityOutOfRangeException when the evolution item is missing.")]
  public async Task Given_MissingInventoryItem_When_Evolve_Then_InventoryQuantityOutOfRangeException()
  {
    Evolution evolution = new(EvolutionId.NewId(Context.WorldId), _source, _target, EvolutionTrigger.ItemUsed, _leafStone, Context.ActorId);
    await _evolutionRepository.SaveAsync(evolution);
    PokemonDto created = await CreateOwnedPokemonAsync("no-stone");

    InventoryQuantityOutOfRangeException exception = await Assert.ThrowsAsync<InventoryQuantityOutOfRangeException>(
      async () => await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId }));
    Assert.Equal(_trainer.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(_leafStone.EntityId, exception.Data["ItemId"]);

    PokemonDto? pokemon = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(pokemon);
    Assert.Equal(_source.EntityId, pokemon.Form.Id);
  }

  [Fact(DisplayName = "It should learn evolution moves into the moveset when slots remain.")]
  public async Task Given_EvolutionMovesAndRoom_When_Evolve_Then_MovesAdded()
  {
    Move ember = MoveBuilder.Ember(Faker, Context.World);
    Move waterGun = MoveBuilder.WaterGun(Faker, Context.World);
    await _moveRepository.SaveAsync([ember, waterGun]);

    Variety targetVariety = await _varietyRepository.LoadAsync(_target.VarietyId)
      ?? throw new InvalidOperationException("The target variety was not found.");
    targetVariety.AddMove(new VarietyMove(ember.Id, LearningMethod.Evolution), Context.ActorId);
    targetVariety.AddMove(new VarietyMove(waterGun.Id, LearningMethod.Evolution), Context.ActorId);
    await _varietyRepository.SaveAsync(targetVariety);

    Evolution evolution = await CreateEvolutionAsync();
    PokemonDto created = await CreateOwnedPokemonAsync("learn-on-evolve");
    Assert.Empty(created.Moves);

    PokemonDto? pokemon = await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId });
    Assert.NotNull(pokemon);
    Assert.Equal(_target.EntityId, pokemon.Form.Id);

    Assert.Equal(2, pokemon.Moves.Count);
    HashSet<int> slots = [];
    foreach (Move expected in new[] { ember, waterGun })
    {
      PokemonMoveDto move = Assert.Single(pokemon.Moves, dto => dto.Move.Id == expected.EntityId);
      Assert.NotNull(move.Slot);
      Assert.InRange(move.Slot.Value, 0, 1);
      Assert.True(slots.Add(move.Slot.Value));
      AssertEvolutionMove(move, pokemon.Level, move.Slot);
    }
    Assert.Equal(2, slots.Count);
  }

  [Fact(DisplayName = "It should add evolution moves only to the movepool when the moveset is full.")]
  public async Task Given_FullMoveset_When_Evolve_Then_EvolutionMoveOnlyInMovepool()
  {
    Move[] fillers = Enumerable.Range(0, Specimen.MoveLimit)
      .Select(index => new MoveBuilder(Faker)
        .WithWorld(Context.World)
        .WithKey($"evolve-filler-{index}")
        .WithName($"Evolve Filler {index}")
        .Build())
      .ToArray();
    Move evolutionMove = MoveBuilder.Ember(Faker, Context.World);
    await _moveRepository.SaveAsync([.. fillers, evolutionMove]);

    Variety sourceVariety = await _varietyRepository.LoadAsync(_source.VarietyId)
      ?? throw new InvalidOperationException("The source variety was not found.");
    for (int index = 0; index < fillers.Length; index++)
    {
      sourceVariety.AddMove(new VarietyMove(fillers[index].Id, LearningMethod.LevelUp, new Level(1)), Context.ActorId);
    }
    await _varietyRepository.SaveAsync(sourceVariety);

    Variety targetVariety = await _varietyRepository.LoadAsync(_target.VarietyId)
      ?? throw new InvalidOperationException("The target variety was not found.");
    targetVariety.AddMove(new VarietyMove(evolutionMove.Id, LearningMethod.Evolution), Context.ActorId);
    await _varietyRepository.SaveAsync(targetVariety);

    Evolution evolution = await CreateEvolutionAsync();
    PokemonDto created = await CreateOwnedPokemonAsync("full-moveset");
    Assert.Equal(Specimen.MoveLimit, created.Moves.Count(move => move.Slot.HasValue));

    PokemonDto? pokemon = await _pokemonService.EvolveAsync(created.Id, new EvolvePokemonPayload { EvolutionId = evolution.EntityId });
    Assert.NotNull(pokemon);

    PokemonMoveDto learned = Assert.Single(pokemon.Moves, move => move.Move.Id == evolutionMove.EntityId);
    AssertEvolutionMove(learned, pokemon.Level, expectedSlot: null);
    Assert.Equal(Specimen.MoveLimit, pokemon.Moves.Count(move => move.Slot.HasValue));
  }

  private void AssertEvolutionMove(PokemonMoveDto move, int learnedAtLevel, int? expectedSlot)
  {
    Assert.Equal(learnedAtLevel, move.LearnedAtLevel);
    Assert.Equal(LearningMethod.Evolution, move.LearningMethod);
    Assert.False(move.IsMastered);
    Assert.Equal(0, move.PowerPointUpgrades);
    Assert.Equal(expectedSlot, move.Slot);
    Assert.Equal(Actor, move.CreatedBy);
    Assert.Equal(DateTime.UtcNow, move.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(move.CreatedBy, move.UpdatedBy);
    Assert.Equal(move.CreatedOn, move.UpdatedOn, TimeSpan.FromMilliseconds(1));
  }

  private async Task<Evolution> CreateEvolutionAsync(
    EvolutionTrigger trigger = EvolutionTrigger.LeveledUp,
    Level? level = null,
    string? location = null,
    TimeOfDay? timeOfDay = null)
  {
    Evolution evolution = new(EvolutionId.NewId(Context.WorldId), _source, _target, trigger, actorId: Context.ActorId);
    if (level is not null || location is not null || timeOfDay.HasValue)
    {
      evolution.SetConditions(level, friendship: false, gender: null, item: null, move: null, Location.TryCreate(location), timeOfDay, Context.ActorId);
    }
    await _evolutionRepository.SaveAsync(evolution);
    return evolution;
  }

  private async Task<PokemonDto> CreateOwnedPokemonAsync(
    string key,
    Trainer? trainer = null,
    byte eggCycles = 0,
    Gender? gender = null,
    int experience = 0)
  {
    PokemonDto created = await CreatePokemonAsync(key, _source, eggCycles, gender, experience);
    PokemonDto? received = await _pokemonService.ReceiveAsync(created.Id, CreateReceivePayload(trainer));
    Assert.NotNull(received);
    return received;
  }

  private async Task<PokemonDto> CreatePokemonAsync(
    string key,
    Form? form = null,
    byte eggCycles = 0,
    Gender? gender = null,
    int experience = 0)
  {
    CreatePokemonPayload payload = new()
    {
      FormId = (form ?? _source).EntityId,
      Key = key,
      EggCycles = eggCycles,
      Gender = gender,
      Experience = experience
    };
    return await _pokemonService.CreateAsync(payload);
  }

  private ReceivePokemonPayload CreateReceivePayload(Trainer? trainer = null) => new()
  {
    TrainerId = (trainer ?? _trainer).EntityId,
    PokeBallId = _masterBall.EntityId,
    Location = "Pallet Town"
  };

  private void AssertEvolved(PokemonDto before, PokemonDto after)
  {
    Assert.Equal(before.Id, after.Id);
    Assert.Equal(before.Version + 1, after.Version);
    Assert.Equal(_target.EntityId, after.Form.Id);
    Assert.Equal(_target.VarietyId.EntityId, after.Form.Variety.Id);
    Assert.Equal(before.Key, after.Key);
    Assert.Equal(before.Gender, after.Gender);
    Assert.Equal(before.IsShiny, after.IsShiny);
    Assert.Equal(before.AbilitySlot, after.AbilitySlot);
    Assert.Equal(before.Level, after.Level);
    Assert.Equal(before.Experience, after.Experience);
    Assert.Equal(_trainer.EntityId, after.OriginalTrainer!.Id);

    int vitalityDelta = after.Statistics.Vitality.Total - before.Statistics.Vitality.Total;
    int staminaDelta = after.Statistics.Stamina.Total - before.Statistics.Stamina.Total;
    Assert.Equal(Math.Clamp(before.Vitality + vitalityDelta, 0, after.Statistics.Vitality.Total), after.Vitality);
    Assert.Equal(Math.Clamp(before.Stamina + staminaDelta, 0, after.Statistics.Stamina.Total), after.Stamina);
  }

  private void AssertPokemonAcquired(PokemonDto pokemon, Trainer trainer)
  {
    PokemonAcquired expected = new(
      new TrainerId(Context.WorldId, trainer.EntityId),
      new PokemonId(Context.WorldId, pokemon.Id),
      new VarietyId(Context.WorldId, pokemon.Form.Variety.Id));
    MessagingManager.Verify(x => x.PublishAsync(expected, It.IsAny<ActorId?>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  private static int ExperienceForLevel(int level)
    => level <= 1 ? 0 : ExperienceTable.GetThreshold(GrowthRate.MediumSlow, level - 1);

  private async Task<Form> CreateFormAsync(string key)
  {
    PokemonSpecies species = key switch
    {
      "bulbasaur" => SpeciesBuilder.Bulbasaur(Faker, Context.World),
      "charmander" => SpeciesBuilder.Charmander(Faker, Context.World),
      _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
    await _speciesRepository.SaveAsync(species);

    Variety variety = key switch
    {
      "bulbasaur" => VarietyBuilder.Bulbasaur(Faker, species, Context.World),
      "charmander" => VarietyBuilder.Charmander(Faker, species, Context.World),
      _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
    await _varietyRepository.SaveAsync(variety);

    Ability ability = key switch
    {
      "bulbasaur" => AbilityBuilder.Overgrow(Faker, Context.World),
      "charmander" => AbilityBuilder.Blaze(Faker, Context.World),
      _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
    await _abilityRepository.SaveAsync(ability);

    Form form = key switch
    {
      "bulbasaur" => FormBuilder.Bulbasaur(Faker, variety, ability, Context.World),
      "charmander" => FormBuilder.Charmander(Faker, variety, ability, Context.World),
      _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
    await _formRepository.SaveAsync(form);
    return form;
  }
}
