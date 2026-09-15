using Logitar.EventSourcing;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core.Pokedexes;
using PokeGame.Core.Species;
using PokeGame.Core.Trainers;
using PokeGame.Core.Varieties;

namespace PokeGame.Pokedexes;

[Trait(Traits.Category, Categories.Integration)]
public class PokedexIntegrationTests : IntegrationTests
{
  private readonly IPokedexRepository _pokedexRepository;
  private readonly IPokedexService _pokedexService;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ITrainerRepository _trainerRepository;
  private readonly IVarietyRepository _varietyRepository;

  private Trainer _trainer = null!;
  private Variety _bulbasaur = null!;
  private Variety _charmander = null!;

  public PokedexIntegrationTests()
  {
    _pokedexRepository = ServiceProvider.GetRequiredService<IPokedexRepository>();
    _pokedexService = ServiceProvider.GetRequiredService<IPokedexService>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _trainerRepository = ServiceProvider.GetRequiredService<ITrainerRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _trainer = TrainerBuilder.Red(Faker, Context.World);
    await _trainerRepository.SaveAsync(_trainer);

    PokemonSpecies bulbasaurSpecies = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    await _speciesRepository.SaveAsync(bulbasaurSpecies);
    _bulbasaur = VarietyBuilder.Bulbasaur(Faker, bulbasaurSpecies, Context.World);
    await _varietyRepository.SaveAsync(_bulbasaur);

    PokemonSpecies charmanderSpecies = SpeciesBuilder.Charmander(Faker, Context.World);
    await _speciesRepository.SaveAsync(charmanderSpecies);
    _charmander = VarietyBuilder.Charmander(Faker, charmanderSpecies, Context.World);
    await _varietyRepository.SaveAsync(_charmander);
  }

  [Fact(DisplayName = "It should create a pokedex entry when a variety is acquired.")]
  public async Task Given_MissingPokedex_When_RegisterAcquired_Then_EntryPersisted()
  {
    await _pokedexService.RegisterEntryAcquiredAsync(_trainer.Id, _bulbasaur.Id, new ActorId(Actor.Id));

    Pokedex? pokedex = await _pokedexRepository.LoadAsync(new PokedexId(_trainer.Id));
    Assert.NotNull(pokedex);
    Assert.Equal(_trainer.Id, pokedex.TrainerId);
    Assert.Equal(PokedexEntryStatus.Acquired, pokedex.Entries[_bulbasaur.Id]);
    Assert.Equal(1, pokedex.Version);
  }

  [Fact(DisplayName = "It should be idempotent when the variety is already acquired.")]
  public async Task Given_AcquiredEntry_When_RegisterAcquiredAgain_Then_Unchanged()
  {
    await _pokedexService.RegisterEntryAcquiredAsync(_trainer.Id, _bulbasaur.Id);
    Pokedex? before = await _pokedexRepository.LoadAsync(new PokedexId(_trainer.Id));
    Assert.NotNull(before);

    await _pokedexService.RegisterEntryAcquiredAsync(_trainer.Id, _bulbasaur.Id);

    Pokedex? after = await _pokedexRepository.LoadAsync(new PokedexId(_trainer.Id));
    Assert.NotNull(after);
    Assert.Equal(before.Version, after.Version);
    Assert.Equal(PokedexEntryStatus.Acquired, after.Entries[_bulbasaur.Id]);
  }

  [Fact(DisplayName = "It should register multiple acquired varieties on the same pokedex.")]
  public async Task Given_ExistingPokedex_When_RegisterOtherVariety_Then_BothAcquired()
  {
    await _pokedexService.RegisterEntryAcquiredAsync(_trainer.Id, _bulbasaur.Id);
    await _pokedexService.RegisterEntryAcquiredAsync(_trainer.Id, _charmander.Id);

    Pokedex? pokedex = await _pokedexRepository.LoadAsync(new PokedexId(_trainer.Id));
    Assert.NotNull(pokedex);
    Assert.Equal(2, pokedex.Entries.Count);
    Assert.Equal(PokedexEntryStatus.Acquired, pokedex.Entries[_bulbasaur.Id]);
    Assert.Equal(PokedexEntryStatus.Acquired, pokedex.Entries[_charmander.Id]);
  }

  [Fact(DisplayName = "It should upgrade a seen entry to acquired.")]
  public async Task Given_SeenEntry_When_RegisterAcquired_Then_Upgraded()
  {
    Pokedex pokedex = new(_trainer);
    pokedex.RegisterSeen(_bulbasaur.Id);
    await _pokedexRepository.SaveAsync(pokedex);

    await _pokedexService.RegisterEntryAcquiredAsync(_trainer.Id, _bulbasaur.Id);

    Pokedex? loaded = await _pokedexRepository.LoadAsync(new PokedexId(_trainer.Id));
    Assert.NotNull(loaded);
    Assert.Equal(PokedexEntryStatus.Acquired, loaded.Entries[_bulbasaur.Id]);
    Assert.Equal(2, loaded.Version);
  }

  [Fact(DisplayName = "It should keep pokedexes isolated per trainer.")]
  public async Task Given_TwoTrainers_When_RegisterAcquired_Then_EntriesIsolated()
  {
    Trainer blue = TrainerBuilder.Blue(Faker, Context.World);
    await _trainerRepository.SaveAsync(blue);

    await _pokedexService.RegisterEntryAcquiredAsync(_trainer.Id, _bulbasaur.Id);
    await _pokedexService.RegisterEntryAcquiredAsync(blue.Id, _charmander.Id);

    Pokedex? redPokedex = await _pokedexRepository.LoadAsync(new PokedexId(_trainer.Id));
    Pokedex? bluePokedex = await _pokedexRepository.LoadAsync(new PokedexId(blue.Id));
    Assert.NotNull(redPokedex);
    Assert.NotNull(bluePokedex);
    Assert.True(redPokedex.Entries.ContainsKey(_bulbasaur.Id));
    Assert.False(redPokedex.Entries.ContainsKey(_charmander.Id));
    Assert.True(bluePokedex.Entries.ContainsKey(_charmander.Id));
    Assert.False(bluePokedex.Entries.ContainsKey(_bulbasaur.Id));
  }
}
