using Microsoft.Extensions.DependencyInjection;
using Moq;
using Logitar.EventSourcing;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Inventory;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Events;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Rosters;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

[Trait(Traits.Category, Categories.Integration)]
public class PokemonOwnershipIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IFormRepository _formRepository;
  private readonly IInventoryService _inventoryService;
  private readonly IItemRepository _itemRepository;
  private readonly IPokemonService _pokemonService;
  private readonly IRosterRepository _rosterRepository;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ITrainerRepository _trainerRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Form _form = null!;
  private Item _masterBall = null!;
  private Trainer _trainer = null!;

  public PokemonOwnershipIntegrationTests()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _inventoryService = ServiceProvider.GetRequiredService<IInventoryService>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
    _rosterRepository = ServiceProvider.GetRequiredService<IRosterRepository>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    Ability ability = AbilityBuilder.Overgrow(Faker, Context.World);
    await _abilityRepository.SaveAsync(ability);

    PokemonSpecies species = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    await _speciesRepository.SaveAsync(species);

    Variety variety = VarietyBuilder.Bulbasaur(Faker, species, Context.World);
    await _varietyRepository.SaveAsync(variety);

    _form = FormBuilder.Bulbasaur(Faker, variety, ability, Context.World);
    await _formRepository.SaveAsync(_form);

    _trainer = TrainerBuilder.Red(Faker, Context.World);
    await _trainerRepository.SaveAsync(_trainer);

    _masterBall = ItemBuilder.MasterBall(Faker, Context.World);
    await _itemRepository.SaveAsync(_masterBall);
  }

  [Fact(DisplayName = "It should receive a Pokémon.")]
  public async Task Given_WildPokemon_When_Receive_Then_Received()
  {
    PokemonDto created = await CreatePokemonAsync("received-bulbasaur");
    ReceivePokemonPayload payload = CreatePayload(" Pallet Town ");

    PokemonDto? pokemon = await _pokemonService.ReceiveAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.Equal(created.Id, pokemon.Id);
    Assert.Equal(created.Version + 1, pokemon.Version);
    Assert.Equal(created.CreatedBy, pokemon.CreatedBy);
    Assert.Equal(created.CreatedOn, pokemon.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertReceived(pokemon, _trainer, _masterBall, created.Level, "Pallet Town");
    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    AssertPokemonAcquired(pokemon, _trainer);

    PokemonDto? read = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(read);
    AssertReceived(read, _trainer, _masterBall, created.Level, "Pallet Town");
    Assert.NotNull(read.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, read.OriginalTrainer.Id);

    await AssertRosterContainsAsync(_trainer, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should receive an egg without setting the original trainer.")]
  public async Task Given_Egg_When_Receive_Then_OriginalTrainerNotSet()
  {
    PokemonDto created = await CreatePokemonAsync("received-egg", eggCycles: 5);
    ReceivePokemonPayload payload = CreatePayload("Pallet Town");

    PokemonDto? pokemon = await _pokemonService.ReceiveAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.Null(pokemon.OriginalTrainer);
    AssertReceived(pokemon, _trainer, _masterBall, created.Level, "Pallet Town");

    await AssertRosterContainsAsync(_trainer, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should transfer a Pokémon to another trainer.")]
  public async Task Given_DifferentTrainer_When_Receive_Then_Transferred()
  {
    PokemonDto created = await CreatePokemonAsync("transferred-bulbasaur");
    await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));

    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    ReceivePokemonPayload payload = CreatePayload("Cerulean City", blue);

    PokemonDto? pokemon = await _pokemonService.ReceiveAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    AssertReceived(pokemon, blue, _masterBall, created.Level, "Cerulean City");
    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    await AssertRosterDoesNotContainAsync(_trainer, pokemon);
    await AssertRosterContainsAsync(blue, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should send a received Pokémon to the box when the party is full.")]
  public async Task Given_PartyIsFull_When_Receive_Then_AddedToBox()
  {
    for (int index = 0; index < Roster.PartyLimit; index++)
    {
      PokemonDto created = await CreatePokemonAsync($"party-{index}");
      PokemonDto? received = await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));
      Assert.NotNull(received);
      await AssertRosterContainsAsync(_trainer, received, isInParty: true);
    }

    PokemonDto boxedCreated = await CreatePokemonAsync("boxed");
    PokemonDto? boxed = await _pokemonService.ReceiveAsync(boxedCreated.Id, CreatePayload("Pallet Town"));
    Assert.NotNull(boxed);

    await AssertRosterContainsAsync(_trainer, boxed, isInParty: false);

    Roster roster = await LoadRosterAsync(_trainer);
    Assert.Equal(Roster.PartyLimit + 1, roster.Entries.Count);
    Assert.Equal(Roster.PartyLimit, roster.PartyIds.Count);
  }

  [Fact(DisplayName = "It should return null when the Pokémon was not found.")]
  public async Task Given_NotFound_When_Receive_Then_NullReturned()
  {
    Assert.Null(await _pokemonService.ReceiveAsync(Guid.NewGuid(), CreatePayload("Pallet Town")));
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the trainer does not exist.")]
  public async Task Given_MissingTrainer_When_Receive_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreatePokemonAsync("missing-trainer");
    Guid missingTrainerId = Guid.NewGuid();
    ReceivePokemonPayload payload = new()
    {
      TrainerId = missingTrainerId,
      PokeBallId = _masterBall.EntityId,
      Location = "Pallet Town"
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.ReceiveAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Trainer.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingTrainerId, exception.Data["EntityId"]);
    Assert.Equal(nameof(payload.TrainerId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the Poké Ball does not exist.")]
  public async Task Given_MissingPokeBall_When_Receive_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreatePokemonAsync("missing-poke-ball");
    Guid missingPokeBallId = Guid.NewGuid();
    ReceivePokemonPayload payload = new()
    {
      TrainerId = _trainer.EntityId,
      PokeBallId = missingPokeBallId,
      Location = "Pallet Town"
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.ReceiveAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Item.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingPokeBallId, exception.Data["EntityId"]);
    Assert.Equal(nameof(payload.PokeBallId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidItemCategoryException when the item is not a Poké Ball.")]
  public async Task Given_NotPokeBall_When_Receive_Then_InvalidItemCategoryException()
  {
    PokemonDto created = await CreatePokemonAsync("invalid-category");
    Item potion = ItemBuilder.Potion(Faker, Context.World);
    await _itemRepository.SaveAsync(potion);

    ReceivePokemonPayload payload = new()
    {
      TrainerId = _trainer.EntityId,
      PokeBallId = potion.EntityId,
      Location = "Pallet Town"
    };

    InvalidItemCategoryException exception = await Assert.ThrowsAsync<InvalidItemCategoryException>(
      async () => await _pokemonService.ReceiveAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(potion.EntityId, exception.Data["ItemId"]);
    Assert.Equal(ItemCategory.PokeBall, exception.Data["ExpectedCategory"]);
    Assert.Equal(ItemCategory.Medicine, exception.Data["AttemptedCategory"]);
    Assert.Equal(nameof(PokemonOwnership.PokeBallId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw PokemonAlreadyOwnedException when the trainer already owns the Pokémon.")]
  public async Task Given_SameTrainer_When_Receive_Then_PokemonAlreadyOwnedException()
  {
    PokemonDto created = await CreatePokemonAsync("already-owned");
    ReceivePokemonPayload payload = CreatePayload("Pallet Town");
    await _pokemonService.ReceiveAsync(created.Id, payload);

    PokemonAlreadyOwnedException exception = await Assert.ThrowsAsync<PokemonAlreadyOwnedException>(
      async () => await _pokemonService.ReceiveAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["TrainerId"]);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when transferring with a different Poké Ball.")]
  public async Task Given_DifferentPokeBall_When_Receive_Then_ImmutablePropertyException()
  {
    PokemonDto created = await CreatePokemonAsync("immutable-poke-ball");
    await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));

    Item pokeBall = new ItemBuilder(Faker)
      .WithWorld(Context.World)
      .WithCategory(ItemCategory.PokeBall)
      .WithKey("poke-ball")
      .WithName("Poké Ball")
      .Build();
    await _itemRepository.SaveAsync(pokeBall);

    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    ReceivePokemonPayload payload = new()
    {
      TrainerId = blue.EntityId,
      PokeBallId = pokeBall.EntityId,
      Location = "Cerulean City"
    };

    ImmutablePropertyException<Guid> exception = await Assert.ThrowsAsync<ImmutablePropertyException<Guid>>(
      async () => await _pokemonService.ReceiveAsync(created.Id, payload));
    Assert.Equal(Specimen.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(created.Id, exception.Data["EntityId"]);
    Assert.Equal(_masterBall.EntityId, exception.Data["ExpectedValue"]);
    Assert.Equal(pokeBall.EntityId, exception.Data["AttemptedValue"]);
    Assert.Equal(nameof(PokemonOwnership.PokeBallId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_Receive_Then_InvalidCommandException()
  {
    PokemonDto created = await CreatePokemonAsync("invalid-payload");
    ReceivePokemonPayload payload = new()
    {
      TrainerId = _trainer.EntityId,
      PokeBallId = _masterBall.EntityId,
      Location = string.Empty
    };

    await Assert.ThrowsAsync<InvalidCommandException>(async () => await _pokemonService.ReceiveAsync(created.Id, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when receiving a Pokémon.")]
  public async Task Given_NotAllowed_When_Receive_Then_PermissionDeniedException()
  {
    PokemonDto created = await CreatePokemonAsync("denied-receive");
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town")));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(new Entity(Specimen.EntityKind, created.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);
  }

  [Fact(DisplayName = "It should catch a wild Pokémon and remove a Poké Ball from the inventory.")]
  public async Task Given_WildPokemonAndPokeBall_When_Catch_Then_Caught()
  {
    PokemonDto created = await CreatePokemonAsync("caught-bulbasaur");
    await AddPokeBallsAsync(3);

    CatchPokemonPayload payload = CreateCatchPayload(" Viridian Forest ");

    PokemonDto? pokemon = await _pokemonService.CatchAsync(created.Id, payload);
    Assert.NotNull(pokemon);
    Assert.Equal(created.Id, pokemon.Id);
    Assert.Equal(created.Version + 1, pokemon.Version);
    Assert.Equal(created.CreatedBy, pokemon.CreatedBy);
    Assert.Equal(created.CreatedOn, pokemon.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertOwned(pokemon, OwnershipEvent.Caught, _trainer, _masterBall, created.Level, "Viridian Forest");
    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    PokemonDto? read = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(read);
    AssertOwned(read, OwnershipEvent.Caught, _trainer, _masterBall, created.Level, "Viridian Forest");

    InventoryItemDto? inventoryItem = await _inventoryService.ReadAsync(_trainer.EntityId, _masterBall.EntityId);
    Assert.NotNull(inventoryItem);
    Assert.Equal(2, inventoryItem.Quantity);

    AssertPokemonAcquired(pokemon, _trainer);

    await AssertRosterContainsAsync(_trainer, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should remove the last Poké Ball from the inventory when catching a Pokémon.")]
  public async Task Given_LastPokeBall_When_Catch_Then_RemovedFromInventory()
  {
    PokemonDto created = await CreatePokemonAsync("last-ball");
    await AddPokeBallsAsync(1);

    PokemonDto? pokemon = await _pokemonService.CatchAsync(created.Id, CreateCatchPayload("Viridian Forest"));
    Assert.NotNull(pokemon);
    AssertOwned(pokemon, OwnershipEvent.Caught, _trainer, _masterBall, created.Level, "Viridian Forest");

    Assert.Null(await _inventoryService.ReadAsync(_trainer.EntityId, _masterBall.EntityId));

    await AssertRosterContainsAsync(_trainer, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should throw PokemonEggCannotBeCaughtException when catching an egg.")]
  public async Task Given_Egg_When_Catch_Then_PokemonEggCannotBeCaughtException()
  {
    PokemonDto created = await CreatePokemonAsync("caught-egg", eggCycles: 5);
    await AddPokeBallsAsync();

    PokemonEggCannotBeCaughtException exception = await Assert.ThrowsAsync<PokemonEggCannotBeCaughtException>(
      async () => await _pokemonService.CatchAsync(created.Id, CreateCatchPayload("Viridian Forest")));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal(created.EggCycles, exception.Data["EggCycles"]);

    PokemonDto? pokemon = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(pokemon);
    Assert.Null(pokemon.Ownership);

    InventoryItemDto? inventoryItem = await _inventoryService.ReadAsync(_trainer.EntityId, _masterBall.EntityId);
    Assert.NotNull(inventoryItem);
    Assert.Equal(1, inventoryItem.Quantity);

    await AssertRosterDoesNotContainAsync(_trainer, pokemon);
  }

  [Fact(DisplayName = "It should throw InventoryQuantityOutOfRangeException when the trainer has no Poké Ball.")]
  public async Task Given_NoPokeBall_When_Catch_Then_InventoryQuantityOutOfRangeException()
  {
    PokemonDto created = await CreatePokemonAsync("no-ball");

    InventoryQuantityOutOfRangeException exception = await Assert.ThrowsAsync<InventoryQuantityOutOfRangeException>(
      async () => await _pokemonService.CatchAsync(created.Id, CreateCatchPayload("Viridian Forest")));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["TrainerId"]);
    Assert.Equal(_masterBall.EntityId, exception.Data["ItemId"]);
    Assert.Equal(TrainerInventory.MinimumQuantity, exception.Data["MinimumQuantity"]);
    Assert.Equal(TrainerInventory.MaximumQuantity, exception.Data["MaximumQuantity"]);
    Assert.Equal(-1, exception.Data["AttemptedQuantity"]);
    Assert.Equal("Quantity", exception.Data["PropertyName"]);

    PokemonDto? pokemon = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(pokemon);
    Assert.Null(pokemon.Ownership);

    await AssertRosterDoesNotContainAsync(_trainer, pokemon);
  }

  [Fact(DisplayName = "It should throw PokemonAlreadyOwnedException when catching a Pokémon that is not wild.")]
  public async Task Given_OwnedPokemon_When_Catch_Then_PokemonAlreadyOwnedException()
  {
    PokemonDto created = await CreatePokemonAsync("not-wild");
    await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));
    await AddPokeBallsAsync();

    PokemonAlreadyOwnedException exception = await Assert.ThrowsAsync<PokemonAlreadyOwnedException>(
      async () => await _pokemonService.CatchAsync(created.Id, CreateCatchPayload("Viridian Forest")));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal(_trainer.EntityId, exception.Data["TrainerId"]);

    InventoryItemDto? inventoryItem = await _inventoryService.ReadAsync(_trainer.EntityId, _masterBall.EntityId);
    Assert.NotNull(inventoryItem);
    Assert.Equal(1, inventoryItem.Quantity);

    PokemonDto? pokemon = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(pokemon);
    await AssertRosterContainsAsync(_trainer, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should return null when catching a Pokémon that was not found.")]
  public async Task Given_NotFound_When_Catch_Then_NullReturned()
  {
    Assert.Null(await _pokemonService.CatchAsync(Guid.NewGuid(), CreateCatchPayload("Viridian Forest")));
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when catching with a missing trainer.")]
  public async Task Given_MissingTrainer_When_Catch_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreatePokemonAsync("catch-missing-trainer");
    Guid missingTrainerId = Guid.NewGuid();
    CatchPokemonPayload payload = new()
    {
      TrainerId = missingTrainerId,
      PokeBallId = _masterBall.EntityId,
      Location = "Viridian Forest"
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.CatchAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Trainer.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingTrainerId, exception.Data["EntityId"]);
    Assert.Equal(nameof(payload.TrainerId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when catching with a missing Poké Ball.")]
  public async Task Given_MissingPokeBall_When_Catch_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreatePokemonAsync("catch-missing-poke-ball");
    Guid missingPokeBallId = Guid.NewGuid();
    CatchPokemonPayload payload = new()
    {
      TrainerId = _trainer.EntityId,
      PokeBallId = missingPokeBallId,
      Location = "Viridian Forest"
    };

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.CatchAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Item.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingPokeBallId, exception.Data["EntityId"]);
    Assert.Equal(nameof(payload.PokeBallId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidItemCategoryException when catching with an item that is not a Poké Ball.")]
  public async Task Given_NotPokeBall_When_Catch_Then_InvalidItemCategoryException()
  {
    PokemonDto created = await CreatePokemonAsync("catch-invalid-category");
    Item potion = ItemBuilder.Potion(Faker, Context.World);
    await _itemRepository.SaveAsync(potion);

    CatchPokemonPayload payload = new()
    {
      TrainerId = _trainer.EntityId,
      PokeBallId = potion.EntityId,
      Location = "Viridian Forest"
    };

    InvalidItemCategoryException exception = await Assert.ThrowsAsync<InvalidItemCategoryException>(
      async () => await _pokemonService.CatchAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(potion.EntityId, exception.Data["ItemId"]);
    Assert.Equal(ItemCategory.PokeBall, exception.Data["ExpectedCategory"]);
    Assert.Equal(ItemCategory.Medicine, exception.Data["AttemptedCategory"]);
    Assert.Equal(nameof(PokemonOwnership.PokeBallId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the catch payload is invalid.")]
  public async Task Given_InvalidPayload_When_Catch_Then_InvalidCommandException()
  {
    PokemonDto created = await CreatePokemonAsync("catch-invalid-payload");
    CatchPokemonPayload payload = new()
    {
      TrainerId = _trainer.EntityId,
      PokeBallId = _masterBall.EntityId,
      Location = string.Empty
    };

    await Assert.ThrowsAsync<InvalidCommandException>(async () => await _pokemonService.CatchAsync(created.Id, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when catching a Pokémon.")]
  public async Task Given_NotAllowed_When_Catch_Then_PermissionDeniedException()
  {
    PokemonDto created = await CreatePokemonAsync("denied-catch");
    await AddPokeBallsAsync();
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.CatchAsync(created.Id, CreateCatchPayload("Viridian Forest")));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(new Entity(Specimen.EntityKind, created.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);

    InventoryItemDto? inventoryItem = await _inventoryService.ReadAsync(_trainer.EntityId, _masterBall.EntityId);
    Assert.NotNull(inventoryItem);
    Assert.Equal(1, inventoryItem.Quantity);
  }

  [Fact(DisplayName = "It should release a received Pokémon.")]
  public async Task Given_ReceivedPokemon_When_Release_Then_Released()
  {
    PokemonDto created = await CreatePokemonAsync("released-received");
    PokemonDto? received = await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));
    Assert.NotNull(received);

    PokemonDto? pokemon = await _pokemonService.ReleaseAsync(created.Id);
    Assert.NotNull(pokemon);
    Assert.Equal(created.Id, pokemon.Id);
    Assert.Equal(received.Version + 1, pokemon.Version);
    Assert.Equal(created.CreatedBy, pokemon.CreatedBy);
    Assert.Equal(created.CreatedOn, pokemon.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));
    Assert.Null(pokemon.Ownership);
    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    PokemonDto? read = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(read);
    Assert.Null(read.Ownership);
    Assert.NotNull(read.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, read.OriginalTrainer.Id);

    await AssertRosterDoesNotContainAsync(_trainer, pokemon);
  }

  [Fact(DisplayName = "It should release a caught Pokémon.")]
  public async Task Given_CaughtPokemon_When_Release_Then_Released()
  {
    PokemonDto created = await CreatePokemonAsync("released-caught");
    await AddPokeBallsAsync();
    PokemonDto? caught = await _pokemonService.CatchAsync(created.Id, CreateCatchPayload("Viridian Forest"));
    Assert.NotNull(caught);

    PokemonDto? pokemon = await _pokemonService.ReleaseAsync(created.Id);
    Assert.NotNull(pokemon);
    Assert.Null(pokemon.Ownership);
    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    InventoryItemDto? inventoryItem = await _inventoryService.ReadAsync(_trainer.EntityId, _masterBall.EntityId);
    Assert.Null(inventoryItem);

    await AssertRosterDoesNotContainAsync(_trainer, pokemon);
  }

  [Fact(DisplayName = "It should catch a Pokémon after it was released.")]
  public async Task Given_ReleasedPokemon_When_Catch_Then_Caught()
  {
    PokemonDto created = await CreatePokemonAsync("recaught");
    await AddPokeBallsAsync(2);
    await _pokemonService.CatchAsync(created.Id, CreateCatchPayload("Viridian Forest"));
    await _pokemonService.ReleaseAsync(created.Id);

    PokemonDto? pokemon = await _pokemonService.CatchAsync(created.Id, CreateCatchPayload("Route 1"));
    Assert.NotNull(pokemon);
    AssertOwned(pokemon, OwnershipEvent.Caught, _trainer, _masterBall, created.Level, "Route 1");
    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    InventoryItemDto? inventoryItem = await _inventoryService.ReadAsync(_trainer.EntityId, _masterBall.EntityId);
    Assert.Null(inventoryItem);

    await AssertRosterContainsAsync(_trainer, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should return null when releasing a Pokémon that was not found.")]
  public async Task Given_NotFound_When_Release_Then_NullReturned()
  {
    Assert.Null(await _pokemonService.ReleaseAsync(Guid.NewGuid()));
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when releasing a wild Pokémon.")]
  public async Task Given_WildPokemon_When_Release_Then_PokemonHasNoOwnerException()
  {
    PokemonDto created = await CreatePokemonAsync("wild-release");

    PokemonHasNoOwnerException exception = await Assert.ThrowsAsync<PokemonHasNoOwnerException>(
      async () => await _pokemonService.ReleaseAsync(created.Id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
  }

  [Fact(DisplayName = "It should throw PokemonEggCannotBeReleasedException when releasing an egg.")]
  public async Task Given_Egg_When_Release_Then_PokemonEggCannotBeReleasedException()
  {
    PokemonDto created = await CreatePokemonAsync("released-egg", eggCycles: 5);
    await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));

    PokemonEggCannotBeReleasedException exception = await Assert.ThrowsAsync<PokemonEggCannotBeReleasedException>(
      async () => await _pokemonService.ReleaseAsync(created.Id));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(created.Id, exception.Data["PokemonId"]);
    Assert.Equal(created.EggCycles, exception.Data["EggCycles"]);

    PokemonDto? pokemon = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(pokemon);
    AssertReceived(pokemon, _trainer, _masterBall, created.Level, "Pallet Town");

    await AssertRosterContainsAsync(_trainer, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when releasing a Pokémon.")]
  public async Task Given_NotAllowed_When_Release_Then_PermissionDeniedException()
  {
    PokemonDto created = await CreatePokemonAsync("denied-release");
    await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.ReleaseAsync(created.Id));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(new Entity(Specimen.EntityKind, created.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);

    PokemonDto? pokemon = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(pokemon);
    AssertReceived(pokemon, _trainer, _masterBall, created.Level, "Pallet Town");

    await AssertRosterContainsAsync(_trainer, pokemon, isInParty: true);
  }

  [Fact(DisplayName = "It should trade two Pokémon owned by different trainers.")]
  public async Task Given_DifferentOwners_When_Trade_Then_Traded()
  {
    Item pokeBall = new ItemBuilder(Faker)
      .WithWorld(Context.World)
      .WithCategory(ItemCategory.PokeBall)
      .WithKey("poke-ball")
      .WithName("Poké Ball")
      .Build();
    await _itemRepository.SaveAsync(pokeBall);

    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    PokemonDto sourceCreated = await CreatePokemonAsync("trade-source");
    PokemonDto targetCreated = await CreatePokemonAsync("trade-target");
    await _pokemonService.ReceiveAsync(sourceCreated.Id, CreatePayload("Pallet Town"));
    await _pokemonService.ReceiveAsync(targetCreated.Id, CreatePayload("Cerulean City", blue, pokeBall));

    await _pokemonService.TradeAsync(new TradePokemonPayload
    {
      PokemonIds = [sourceCreated.Id, targetCreated.Id],
      Location = " Pokémon Center "
    });

    PokemonDto? source = await _pokemonService.ReadAsync(sourceCreated.Id);
    Assert.NotNull(source);
    Assert.Equal(sourceCreated.Version + 2, source.Version);
    AssertOwned(source, OwnershipEvent.Traded, blue, _masterBall, sourceCreated.Level, "Pokémon Center");
    Assert.NotNull(source.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, source.OriginalTrainer.Id);

    PokemonDto? target = await _pokemonService.ReadAsync(targetCreated.Id);
    Assert.NotNull(target);
    Assert.Equal(targetCreated.Version + 2, target.Version);
    AssertOwned(target, OwnershipEvent.Traded, _trainer, pokeBall, targetCreated.Level, "Pokémon Center");
    Assert.NotNull(target.OriginalTrainer);
    Assert.Equal(blue.EntityId, target.OriginalTrainer.Id);

    AssertPokemonAcquired((source, blue), (target, _trainer));

    await AssertRosterContainsAsync(blue, source, isInParty: true);
    await AssertRosterDoesNotContainAsync(_trainer, source);
    await AssertRosterContainsAsync(_trainer, target, isInParty: true);
    await AssertRosterDoesNotContainAsync(blue, target);
  }

  [Fact(DisplayName = "It should preserve party slots when trading a boxed Pokémon.")]
  public async Task Given_BoxedPokemon_When_Trade_Then_PartySlotsPreserved()
  {
    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    for (int index = 0; index < Roster.PartyLimit; index++)
    {
      PokemonDto created = await CreatePokemonAsync($"red-party-{index}");
      await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));
    }

    PokemonDto boxedCreated = await CreatePokemonAsync("red-boxed");
    PokemonDto? boxed = await _pokemonService.ReceiveAsync(boxedCreated.Id, CreatePayload("Pallet Town"));
    Assert.NotNull(boxed);
    await AssertRosterContainsAsync(_trainer, boxed, isInParty: false);

    PokemonDto blueCreated = await CreatePokemonAsync("blue-party");
    PokemonDto? blueParty = await _pokemonService.ReceiveAsync(blueCreated.Id, CreatePayload("Cerulean City", blue));
    Assert.NotNull(blueParty);
    await AssertRosterContainsAsync(blue, blueParty, isInParty: true);

    await _pokemonService.TradeAsync(new TradePokemonPayload
    {
      PokemonIds = [boxed.Id, blueParty.Id],
      Location = "Pokémon Center"
    });

    PokemonDto? tradedBoxed = await _pokemonService.ReadAsync(boxed.Id);
    Assert.NotNull(tradedBoxed);
    PokemonDto? tradedParty = await _pokemonService.ReadAsync(blueParty.Id);
    Assert.NotNull(tradedParty);

    await AssertRosterContainsAsync(blue, tradedBoxed, isInParty: true);
    await AssertRosterDoesNotContainAsync(_trainer, tradedBoxed);
    await AssertRosterContainsAsync(_trainer, tradedParty, isInParty: false);
    await AssertRosterDoesNotContainAsync(blue, tradedParty);

    Roster redRoster = await LoadRosterAsync(_trainer);
    Assert.Equal(Roster.PartyLimit, redRoster.PartyIds.Count);
    Assert.Equal(Roster.PartyLimit + 1, redRoster.Entries.Count);

    Roster blueRoster = await LoadRosterAsync(blue);
    Assert.Single(blueRoster.Entries);
    Assert.Single(blueRoster.PartyIds);
  }

  [Fact(DisplayName = "It should trade Pokémon eggs without setting the original trainer.")]
  public async Task Given_Eggs_When_Trade_Then_TradedWithoutOriginalTrainer()
  {
    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    PokemonDto sourceCreated = await CreatePokemonAsync("trade-source-egg", eggCycles: 5);
    PokemonDto targetCreated = await CreatePokemonAsync("trade-target-egg", eggCycles: 5);
    await _pokemonService.ReceiveAsync(sourceCreated.Id, CreatePayload("Pallet Town"));
    await _pokemonService.ReceiveAsync(targetCreated.Id, CreatePayload("Cerulean City", blue));

    await _pokemonService.TradeAsync(new TradePokemonPayload
    {
      PokemonIds = [sourceCreated.Id, targetCreated.Id],
      Location = "Day Care"
    });

    PokemonDto? source = await _pokemonService.ReadAsync(sourceCreated.Id);
    Assert.NotNull(source);
    Assert.Null(source.OriginalTrainer);
    AssertOwned(source, OwnershipEvent.Traded, blue, _masterBall, sourceCreated.Level, "Day Care");

    PokemonDto? target = await _pokemonService.ReadAsync(targetCreated.Id);
    Assert.NotNull(target);
    Assert.Null(target.OriginalTrainer);
    AssertOwned(target, OwnershipEvent.Traded, _trainer, _masterBall, targetCreated.Level, "Day Care");

    AssertPokemonAcquired((source, blue), (target, _trainer));

    await AssertRosterContainsAsync(blue, source, isInParty: true);
    await AssertRosterContainsAsync(_trainer, target, isInParty: true);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when a Pokémon to trade was not found.")]
  public async Task Given_MissingPokemon_When_Trade_Then_EntityNotFoundException()
  {
    PokemonDto created = await CreatePokemonAsync("trade-missing");
    await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town"));
    Guid missingId = Guid.NewGuid();

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _pokemonService.TradeAsync(new TradePokemonPayload
      {
        PokemonIds = [created.Id, missingId],
        Location = "Pokémon Center"
      }));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(Specimen.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(missingId, exception.Data["EntityId"]);
    Assert.Equal(nameof(TradePokemonPayload.PokemonIds), exception.Data["PropertyName"]);

    AssertPokemonAcquiredNotPublished();
  }

  [Fact(DisplayName = "It should throw PokemonHasNoOwnerException when a Pokémon to trade is wild.")]
  public async Task Given_WildPokemon_When_Trade_Then_PokemonHasNoOwnerException()
  {
    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    PokemonDto sourceCreated = await CreatePokemonAsync("trade-wild-source");
    PokemonDto targetCreated = await CreatePokemonAsync("trade-wild-target");
    await _pokemonService.ReceiveAsync(targetCreated.Id, CreatePayload("Cerulean City", blue));

    PokemonHasNoOwnerException exception = await Assert.ThrowsAsync<PokemonHasNoOwnerException>(
      async () => await _pokemonService.TradeAsync(new TradePokemonPayload
      {
        PokemonIds = [sourceCreated.Id, targetCreated.Id],
        Location = "Pokémon Center"
      }));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(sourceCreated.Id, exception.Data["PokemonId"]);

    AssertPokemonAcquiredNotPublished();
  }

  [Fact(DisplayName = "It should throw PokemonTradeRequiresDifferentOwnersException when both Pokémon have the same owner.")]
  public async Task Given_SameOwner_When_Trade_Then_PokemonTradeRequiresDifferentOwnersException()
  {
    PokemonDto sourceCreated = await CreatePokemonAsync("trade-same-source");
    PokemonDto targetCreated = await CreatePokemonAsync("trade-same-target");
    await _pokemonService.ReceiveAsync(sourceCreated.Id, CreatePayload("Pallet Town"));
    await _pokemonService.ReceiveAsync(targetCreated.Id, CreatePayload("Viridian City"));

    PokemonTradeRequiresDifferentOwnersException exception = await Assert.ThrowsAsync<PokemonTradeRequiresDifferentOwnersException>(
      async () => await _pokemonService.TradeAsync(new TradePokemonPayload
      {
        PokemonIds = [sourceCreated.Id, targetCreated.Id],
        Location = "Pokémon Center"
      }));
    Assert.Equal(Context.WorldId.EntityId, exception.Data["WorldId"]);
    Assert.Equal(sourceCreated.Id, exception.Data["SourcePokemonId"]);
    Assert.Equal(targetCreated.Id, exception.Data["TargetPokemonId"]);

    AssertPokemonAcquiredNotPublished();
  }

  [Fact(DisplayName = "It should throw InvalidCommandException when the trade payload is invalid.")]
  public async Task Given_InvalidPayload_When_Trade_Then_InvalidCommandException()
  {
    await Assert.ThrowsAsync<InvalidCommandException>(async () => await _pokemonService.TradeAsync(new TradePokemonPayload
    {
      PokemonIds = [Guid.NewGuid()],
      Location = "Pokémon Center"
    }));

    AssertPokemonAcquiredNotPublished();
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when trading Pokémon.")]
  public async Task Given_NotAllowed_When_Trade_Then_PermissionDeniedException()
  {
    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    PokemonDto sourceCreated = await CreatePokemonAsync("denied-trade-source");
    PokemonDto targetCreated = await CreatePokemonAsync("denied-trade-target");
    await _pokemonService.ReceiveAsync(sourceCreated.Id, CreatePayload("Pallet Town"));
    await _pokemonService.ReceiveAsync(targetCreated.Id, CreatePayload("Cerulean City", blue));
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.TradeAsync(new TradePokemonPayload
      {
        PokemonIds = [sourceCreated.Id, targetCreated.Id],
        Location = "Pokémon Center"
      }));
    Assert.Equal(Context.ActorId?.Value, exception.Data["Principal"]);
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(new Entity(Specimen.EntityKind, sourceCreated.Id, Context.WorldId).ToString(), exception.Data["Resource"]);
    Assert.Equal(Context.WorldId, exception.Data["WorldId"]);

    PokemonDto? source = await _pokemonService.ReadAsync(sourceCreated.Id);
    Assert.NotNull(source);
    AssertReceived(source, _trainer, _masterBall, sourceCreated.Level, "Pallet Town");

    PokemonDto? target = await _pokemonService.ReadAsync(targetCreated.Id);
    Assert.NotNull(target);
    AssertReceived(target, blue, _masterBall, targetCreated.Level, "Cerulean City");

    AssertPokemonAcquiredNotPublished();
  }

  private async Task<PokemonDto> CreatePokemonAsync(string key, byte eggCycles = 0)
  {
    CreatePokemonPayload payload = new()
    {
      FormId = _form.EntityId,
      Key = key,
      EggCycles = eggCycles
    };
    return await _pokemonService.CreateAsync(payload);
  }

  private ReceivePokemonPayload CreatePayload(string location, Trainer? trainer = null, Item? pokeBall = null) => new()
  {
    TrainerId = (trainer ?? _trainer).EntityId,
    PokeBallId = (pokeBall ?? _masterBall).EntityId,
    Location = location
  };

  private CatchPokemonPayload CreateCatchPayload(string location, Trainer? trainer = null) => new()
  {
    TrainerId = (trainer ?? _trainer).EntityId,
    PokeBallId = _masterBall.EntityId,
    Location = location
  };

  private async Task AddPokeBallsAsync(int quantity = 1)
  {
    await _inventoryService.SetAsync(_trainer.EntityId, _masterBall.EntityId, new SetInventoryItemPayload
    {
      Quantity = quantity
    });
  }

  private static void AssertReceived(PokemonDto pokemon, Trainer trainer, Item pokeBall, int metLevel, string location)
  {
    AssertOwned(pokemon, OwnershipEvent.Received, trainer, pokeBall, metLevel, location);
  }

  private static void AssertOwned(PokemonDto pokemon, OwnershipEvent @event, Trainer trainer, Item pokeBall, int metLevel, string location)
  {
    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(@event, pokemon.Ownership.Event);
    Assert.Equal(trainer.EntityId, pokemon.Ownership.Trainer.Id);
    Assert.Equal(pokeBall.EntityId, pokemon.Ownership.PokeBall.Id);
    Assert.Equal(ItemCategory.PokeBall, pokemon.Ownership.PokeBall.Category);
    Assert.Equal(metLevel, pokemon.Ownership.MetLevel);
    Assert.Equal(location, pokemon.Ownership.MetAt);
    Assert.Equal(DateTime.UtcNow, pokemon.Ownership.MetOn, TimeSpan.FromSeconds(10));
  }

  private void AssertPokemonAcquired(PokemonDto pokemon, Trainer trainer)
  {
    PokemonAcquired expected = ToPokemonAcquired(pokemon, trainer);
    MessagingManager.Verify(x => x.PublishAsync(expected, It.IsAny<CancellationToken>()), Times.Once);
  }
  private void AssertPokemonAcquired(params (PokemonDto Pokemon, Trainer Trainer)[] expected)
  {
    PokemonAcquired[] acquired = [.. expected.Select(item => ToPokemonAcquired(item.Pokemon, item.Trainer))];
    MessagingManager.Verify(x => x.PublishAsync(
      It.Is<IEnumerable<IEvent>>(events => MatchPokemonAcquired(events, acquired)),
      It.IsAny<CancellationToken>()), Times.Once);
  }
  private void AssertPokemonAcquiredNotPublished()
  {
    MessagingManager.Verify(x => x.PublishAsync(It.IsAny<IEnumerable<IEvent>>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  private static bool MatchPokemonAcquired(IEnumerable<IEvent> events, IReadOnlyCollection<PokemonAcquired> expected)
  {
    PokemonAcquired[] actual = [.. events.OfType<PokemonAcquired>()];
    return actual.Length == expected.Count && expected.All(actual.Contains);
  }

  private PokemonAcquired ToPokemonAcquired(PokemonDto pokemon, Trainer trainer) => new(
    new TrainerId(Context.WorldId, trainer.EntityId),
    new PokemonId(Context.WorldId, pokemon.Id),
    new VarietyId(Context.WorldId, pokemon.Form.Variety.Id));

  private async Task AssertRosterContainsAsync(Trainer trainer, PokemonDto pokemon, bool isInParty)
  {
    Roster roster = await LoadRosterAsync(trainer);
    PokemonId pokemonId = new(Context.WorldId, pokemon.Id);
    Assert.True(roster.Entries.TryGetValue(pokemonId, out RosterEntry? entry));
    Assert.Equal(isInParty, entry.IsInParty);
    Assert.Equal(isInParty, roster.PartyIds.Contains(pokemonId));
  }
  private async Task AssertRosterDoesNotContainAsync(Trainer trainer, PokemonDto pokemon)
  {
    Roster? roster = await _rosterRepository.LoadAsync(new RosterId(trainer.Id));
    if (roster is null)
    {
      return;
    }

    PokemonId pokemonId = new(Context.WorldId, pokemon.Id);
    Assert.False(roster.Entries.ContainsKey(pokemonId));
    Assert.DoesNotContain(pokemonId, roster.PartyIds);
  }
  private async Task<Roster> LoadRosterAsync(Trainer trainer)
  {
    Roster? roster = await _rosterRepository.LoadAsync(new RosterId(trainer.Id));
    Assert.NotNull(roster);
    return roster;
  }
}
