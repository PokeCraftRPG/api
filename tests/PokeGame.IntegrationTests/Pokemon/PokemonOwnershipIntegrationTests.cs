using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Forms;
using PokeGame.Core.Inventory;
using PokeGame.Core.Items;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Pokemon.Models;
using PokeGame.Core.Regions;
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
  private readonly IInventoryRepository _inventoryRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IPokemonRepository _pokemonRepository;
  private readonly IPokemonService _pokemonService;
  private readonly IRosterRepository _rosterRepository;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ITrainerRepository _trainerRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Ability _lightningRod = null!;
  private Ability _static = null!;
  private PokemonSpecies _species = null!;
  private Variety _variety = null!;
  private Form _form = null!;
  private Specimen _specimen = null!;
  private Trainer _trainer = null!;
  private Item _pokeBall = null!;

  public PokemonOwnershipIntegrationTests() : base()
  {
    _abilityRepository = ServiceProvider.GetRequiredService<IAbilityRepository>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _inventoryRepository = ServiceProvider.GetRequiredService<IInventoryRepository>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _pokemonRepository = ServiceProvider.GetRequiredService<IPokemonRepository>();
    _pokemonService = ServiceProvider.GetRequiredService<IPokemonService>();
    _rosterRepository = ServiceProvider.GetRequiredService<IRosterRepository>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _lightningRod = AbilityBuilder.LightningRod(Faker, World);
    _static = AbilityBuilder.Static(Faker, World);
    await _abilityRepository.SaveAsync([_lightningRod, _static]);

    _species = SpeciesBuilder.Pikachu(Faker, World);
    await _speciesRepository.SaveAsync(_species);

    _variety = VarietyBuilder.Pikachu(Faker, World, _species);
    await _varietyRepository.SaveAsync(_variety);

    _form = FormBuilder.Pikachu(Faker, World, _variety, new FormAbilities(_static, secondary: null, _lightningRod));
    await _formRepository.SaveAsync(_form);

    _specimen = new SpecimenBuilder(Faker).WithWorld(World).Is(_species, _variety, _form).Build();
    await _pokemonRepository.SaveAsync(_specimen);

    _trainer = TrainerBuilder.AshKetchum(Faker, World);
    await _trainerRepository.SaveAsync(_trainer);

    _pokeBall = ItemBuilder.PokeBall(Faker, World);
    await _itemRepository.SaveAsync(_pokeBall);
  }

  [Fact(DisplayName = "It should catch a wild Pokémon.")]
  public async Task Given_Wild_When_Catch_Then_Caught()
  {
    InventoryAggregate inventory = await _inventoryRepository.LoadAsync(_trainer);
    inventory.Add(_pokeBall, quantity: 1, World.OwnerId);
    await _inventoryRepository.SaveAsync(inventory);

    CatchPokemonPayload payload = new()
    {
      Trainer = $"  {_trainer.Key.Value.ToUpperInvariant()}  ",
      PokeBall = $"  {_pokeBall.EntityId.ToString().ToUpperInvariant()}  ",
      Location = "  Viridian Forest  "
    };

    PokemonModel? pokemon = await _pokemonService.CatchAsync(_specimen.EntityId, payload);
    Assert.NotNull(pokemon);

    Assert.Equal(_specimen.EntityId, pokemon.Id);
    Assert.Equal(_specimen.Version + 2, pokemon.Version);
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(OwnershipKind.Caught, pokemon.Ownership.Kind);
    Assert.Equal(_trainer.EntityId, pokemon.Ownership.CurrentTrainer.Id);
    Assert.Equal(_pokeBall.EntityId, pokemon.Ownership.PokeBall.Id);
    Assert.Equal(_specimen.Level, pokemon.Ownership.Level);
    Assert.Equal(payload.Location.Trim(), pokemon.Ownership.Location);
    Assert.Equal(DateTime.UtcNow, pokemon.Ownership.MetOn, TimeSpan.FromSeconds(10));

    Assert.Equal(0, pokemon.Ownership.Position);
    Assert.Null(pokemon.Ownership.Box);
  }

  [Fact(DisplayName = "It should catch an egg Pokémon.")]
  public async Task Given_Egg_When_Catch_Then_Caught()
  {
    Specimen specimen = new SpecimenBuilder(Faker).WithWorld(World).Is(_species, _variety, _form).IsEgg().WithKey(new Slug("an-egg")).Build();
    await _pokemonRepository.SaveAsync(specimen);

    InventoryAggregate inventory = await _inventoryRepository.LoadAsync(_trainer);
    inventory.Add(_pokeBall, quantity: 1, World.OwnerId);
    await _inventoryRepository.SaveAsync(inventory);

    CatchPokemonPayload payload = new()
    {
      Trainer = $"  {_trainer.Key.Value.ToUpperInvariant()}  ",
      PokeBall = $"  {_pokeBall.EntityId.ToString().ToUpperInvariant()}  ",
      Location = "  Viridian Forest  "
    };

    PokemonModel? pokemon = await _pokemonService.CatchAsync(specimen.EntityId, payload);
    Assert.NotNull(pokemon);

    Assert.Equal(specimen.EntityId, pokemon.Id);
    Assert.Equal(specimen.Version + 2, pokemon.Version);
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Null(pokemon.OriginalTrainer);

    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(OwnershipKind.Caught, pokemon.Ownership.Kind);
    Assert.Equal(_trainer.EntityId, pokemon.Ownership.CurrentTrainer.Id);
    Assert.Equal(_pokeBall.EntityId, pokemon.Ownership.PokeBall.Id);
    Assert.Equal(specimen.Level, pokemon.Ownership.Level);
    Assert.Equal(payload.Location.Trim(), pokemon.Ownership.Location);
    Assert.Equal(DateTime.UtcNow, pokemon.Ownership.MetOn, TimeSpan.FromSeconds(10));

    Assert.Equal(0, pokemon.Ownership.Position);
    Assert.Null(pokemon.Ownership.Box);
  }

  [Fact(DisplayName = "It should deposit a Pokémon.")]
  public async Task Given_Ownership_When_Deposit_Then_Deposited()
  {
    _specimen.Receive(_trainer, _pokeBall, new Location("Viridian Forest"), World.OwnerId);

    Specimen specimen = new SpecimenBuilder(Faker).WithWorld(World).Is(_species, _variety, _form).WithKey(new Slug("another-pokemon")).Build();
    specimen.Catch(_trainer, _pokeBall, new Location("Mt. Coronet"), World.OwnerId);

    Roster roster = new(_trainer);
    roster.Add(_specimen, World.OwnerId);
    roster.Add(specimen, World.OwnerId);
    await _rosterRepository.SaveAsync(roster);

    await _pokemonRepository.SaveAsync([_specimen, specimen]);

    PokemonModel? pokemon = await _pokemonService.DepositAsync(_specimen.EntityId);
    Assert.NotNull(pokemon);

    Assert.Equal(_specimen.EntityId, pokemon.Id);
    Assert.Equal(_specimen.Version + 1, pokemon.Version);
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(_trainer.EntityId, pokemon.Ownership.CurrentTrainer.Id);
    Assert.Equal(0, pokemon.Ownership.Position);
    Assert.Equal(0, pokemon.Ownership.Box);

    pokemon = await _pokemonService.ReadAsync(specimen.EntityId);
    Assert.NotNull(pokemon);

    Assert.Equal(specimen.Version + 1, pokemon.Version);

    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(_trainer.EntityId, pokemon.Ownership.CurrentTrainer.Id);
    Assert.Equal(0, pokemon.Ownership.Position);
    Assert.Null(pokemon.Ownership.Box);
  }

  [Fact(DisplayName = "It should receive a Pokémon.")]
  public async Task Given_Pokemon_When_Receive_Then_Received()
  {
    Trainer trainer = new TrainerBuilder(Faker).WithWorld(World).Build();
    await _trainerRepository.SaveAsync(trainer);

    Item greatBall = ItemBuilder.GreatBall(Faker, World);
    await _itemRepository.SaveAsync(greatBall);

    InventoryAggregate inventory = await _inventoryRepository.LoadAsync(trainer);
    inventory.Add(greatBall, quantity: 1, World.OwnerId);
    await _inventoryRepository.SaveAsync(inventory);

    _specimen.Catch(trainer, greatBall, new Location("Viridian Forest"), World.OwnerId);
    await _pokemonRepository.SaveAsync(_specimen);

    ReceivePokemonPayload payload = new()
    {
      Trainer = $"  {_trainer.Key.Value.ToUpperInvariant()}  ",
      PokeBall = $"  {_pokeBall.EntityId.ToString().ToUpperInvariant()}  ",
      Location = "  Pallet Town  "
    };

    PokemonModel? pokemon = await _pokemonService.ReceiveAsync(_specimen.EntityId, payload);
    Assert.NotNull(pokemon);

    Assert.Equal(_specimen.EntityId, pokemon.Id);
    Assert.Equal(_specimen.Version + 2, pokemon.Version);
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(trainer.EntityId, pokemon.OriginalTrainer.Id);

    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(OwnershipKind.Received, pokemon.Ownership.Kind);
    Assert.Equal(_trainer.EntityId, pokemon.Ownership.CurrentTrainer.Id);
    Assert.Equal(_pokeBall.EntityId, pokemon.Ownership.PokeBall.Id);
    Assert.Equal(_specimen.Level, pokemon.Ownership.Level);
    Assert.Equal(payload.Location.Trim(), pokemon.Ownership.Location);
    Assert.Equal(DateTime.UtcNow, pokemon.Ownership.MetOn, TimeSpan.FromSeconds(10));

    Assert.Equal(0, pokemon.Ownership.Position);
    Assert.Null(pokemon.Ownership.Box);
  }

  [Fact(DisplayName = "It should release a Pokémon.")]
  public async Task Given_Ownership_When_Release_Then_Released()
  {
    _specimen.Receive(_trainer, _pokeBall, new Location("Viridian Forest"), World.OwnerId);

    Specimen specimen = new SpecimenBuilder(Faker).WithWorld(World).Is(_species, _variety, _form).WithKey(new Slug("another-pokemon")).Build();
    specimen.Catch(_trainer, _pokeBall, new Location("Mt. Coronet"), World.OwnerId);

    Roster roster = new(_trainer);
    roster.Add(_specimen, World.OwnerId);
    roster.Add(specimen, World.OwnerId);
    await _rosterRepository.SaveAsync(roster);

    await _pokemonRepository.SaveAsync([_specimen, specimen]);

    PokemonModel? pokemon = await _pokemonService.ReleaseAsync(_specimen.EntityId);
    Assert.NotNull(pokemon);

    Assert.Equal(_specimen.EntityId, pokemon.Id);
    Assert.Equal(_specimen.Version + 1, pokemon.Version);
    Assert.Equal(Actor, pokemon.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, pokemon.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    Assert.Null(pokemon.Ownership);

    pokemon = await _pokemonService.ReadAsync(specimen.EntityId);
    Assert.NotNull(pokemon);

    Assert.Equal(specimen.Version + 1, pokemon.Version);

    Assert.NotNull(pokemon.OriginalTrainer);
    Assert.Equal(_trainer.EntityId, pokemon.OriginalTrainer.Id);

    Assert.NotNull(pokemon.Ownership);
    Assert.Equal(_trainer.EntityId, pokemon.Ownership.CurrentTrainer.Id);
    Assert.Equal(0, pokemon.Ownership.Position);
    Assert.Null(pokemon.Ownership.Box);
  }
}
