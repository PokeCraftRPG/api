using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Permissions;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokemon;

[Trait(Traits.Category, Categories.Integration)]
public class PokemonOwnershipIntegrationTests : IntegrationTests
{
  private readonly IAbilityRepository _abilityRepository;
  private readonly IFormRepository _formRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IPokemonService _pokemonService;
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
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
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

    PokemonDto? read = await _pokemonService.ReadAsync(created.Id);
    Assert.NotNull(read);
    AssertReceived(read, _trainer, _masterBall, created.Level, "Pallet Town");
    Assert.NotNull(read.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, read.OriginalTrainer.Id);
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
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Trainer.EntityKind, exception.EntityKind);
    Assert.Equal(missingTrainerId, exception.EntityId);
    Assert.Equal(nameof(payload.TrainerId), exception.PropertyName);
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
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Item.EntityKind, exception.EntityKind);
    Assert.Equal(missingPokeBallId, exception.EntityId);
    Assert.Equal(nameof(payload.PokeBallId), exception.PropertyName);
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
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(potion.EntityId, exception.ItemId);
    Assert.Equal(ItemCategory.PokeBall, exception.ExpectedCategory);
    Assert.Equal(ItemCategory.Medicine, exception.AttemptedCategory);
    Assert.Equal(nameof(PokemonOwnership.PokeBallId), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PokemonAlreadyOwnedException when the trainer already owns the Pokémon.")]
  public async Task Given_SameTrainer_When_Receive_Then_PokemonAlreadyOwnedException()
  {
    PokemonDto created = await CreatePokemonAsync("already-owned");
    ReceivePokemonPayload payload = CreatePayload("Pallet Town");
    await _pokemonService.ReceiveAsync(created.Id, payload);

    PokemonAlreadyOwnedException exception = await Assert.ThrowsAsync<PokemonAlreadyOwnedException>(
      async () => await _pokemonService.ReceiveAsync(created.Id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(created.Id, exception.PokemonId);
    Assert.Equal(_trainer.EntityId, exception.TrainerId);
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
    Assert.Equal(Specimen.EntityKind, exception.EntityKind);
    Assert.Equal(created.Id, exception.EntityId);
    Assert.Equal(_masterBall.EntityId, exception.ExpectedValue);
    Assert.Equal(pokeBall.EntityId, exception.AttemptedValue);
    Assert.Equal(nameof(PokemonOwnership.PokeBallId), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_Receive_Then_ValidationException()
  {
    PokemonDto created = await CreatePokemonAsync("invalid-payload");
    ReceivePokemonPayload payload = new()
    {
      TrainerId = _trainer.EntityId,
      PokeBallId = _masterBall.EntityId,
      Location = string.Empty
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _pokemonService.ReceiveAsync(created.Id, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when receiving a Pokémon.")]
  public async Task Given_NotAllowed_When_Receive_Then_PermissionDeniedException()
  {
    PokemonDto created = await CreatePokemonAsync("denied-receive");
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _pokemonService.ReceiveAsync(created.Id, CreatePayload("Pallet Town")));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(new Entity(Specimen.EntityKind, created.Id, Context.WorldId).ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
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

  private ReceivePokemonPayload CreatePayload(string location, Trainer? trainer = null) => new()
  {
    TrainerId = (trainer ?? _trainer).EntityId,
    PokeBallId = _masterBall.EntityId,
    Location = location
  };

  private static void AssertReceived(PokemonDto pokemon, Trainer trainer, Item pokeBall, int metLevel, string location)
  {
    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(OwnershipEvent.Received, pokemon.Ownership.Event);
    Assert.Equal(trainer.EntityId, pokemon.Ownership.Trainer.Id);
    Assert.Equal(pokeBall.EntityId, pokemon.Ownership.PokeBall.Id);
    Assert.Equal(ItemCategory.PokeBall, pokemon.Ownership.PokeBall.Category);
    Assert.Equal(metLevel, pokemon.Ownership.MetLevel);
    Assert.Equal(location, pokemon.Ownership.MetAt);
    Assert.Equal(DateTime.UtcNow, pokemon.Ownership.MetOn, TimeSpan.FromSeconds(10));
  }
}
