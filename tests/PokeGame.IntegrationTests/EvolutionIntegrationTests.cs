using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Pokemon;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class EvolutionIntegrationTests : IntegrationTests
{
  private readonly IEvolutionRepository _evolutionRepository;
  private readonly IEvolutionService _evolutionService;
  private readonly IFormRepository _formRepository;
  private readonly IItemRepository _itemRepository;
  private readonly IMoveRepository _moveRepository;
  private readonly ISpeciesRepository _speciesRepository;
  private readonly IVarietyRepository _varietyRepository;

  private SpeciesAggregate _pikachuSpecies = null!;
  private SpeciesAggregate _raichuSpecies = null!;
  private Variety _pikachuVariety = null!;
  private Variety _raichuVariety = null!;
  private Form _pikachuForm = null!;
  private Form _raichuForm = null!;
  private Item _thunderStone = null!;
  private Item _oranBerry = null!;
  private Move _thunderPunch = null!;
  private Evolution _evolution = null!;

  public EvolutionIntegrationTests()
  {
    _evolutionRepository = ServiceProvider.GetRequiredService<IEvolutionRepository>();
    _evolutionService = ServiceProvider.GetRequiredService<IEvolutionService>();
    _formRepository = ServiceProvider.GetRequiredService<IFormRepository>();
    _itemRepository = ServiceProvider.GetRequiredService<IItemRepository>();
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _varietyRepository = ServiceProvider.GetRequiredService<IVarietyRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _pikachuSpecies = SpeciesBuilder.Pikachu(Faker, World);
    _raichuSpecies = SpeciesBuilder.Raichu(Faker, World);
    await _speciesRepository.SaveAsync([_pikachuSpecies, _raichuSpecies]);

    _pikachuVariety = VarietyBuilder.Pikachu(Faker, World, _pikachuSpecies);
    _raichuVariety = VarietyBuilder.Raichu(Faker, World, _raichuSpecies);
    await _varietyRepository.SaveAsync([_pikachuVariety, _raichuVariety]);

    _pikachuForm = FormBuilder.Pikachu(Faker, World, _pikachuVariety);
    _raichuForm = FormBuilder.Raichu(Faker, World, _raichuVariety);
    await _formRepository.SaveAsync([_pikachuForm, _raichuForm]);

    _thunderStone = ItemBuilder.ThunderStone(Faker, World);
    _oranBerry = ItemBuilder.OranBerry(Faker, World);
    await _itemRepository.SaveAsync([_thunderStone, _oranBerry]);

    _thunderPunch = MoveBuilder.ThunderPunch(Faker, World);
    await _moveRepository.SaveAsync(_thunderPunch);

    _evolution = new Evolution(World, _pikachuForm, _raichuForm, EvolutionTrigger.Item, _thunderStone);
    await _evolutionRepository.SaveAsync(_evolution);
  }

  [Theory(DisplayName = "It should create a new evolution.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceEvolutionPayload payload = new()
    {
      Source = $"  {_pikachuForm.EntityId.ToString().ToUpperInvariant()}  ",
      Target = $"  {_raichuForm.Key.Value.ToUpperInvariant()}  ",
      Trigger = EvolutionTrigger.Item,
      Item = $"  {(withId ? _thunderStone.EntityId.ToString() : _thunderStone.Key.Value).ToUpperInvariant()}  ",
      Level = 18,
      Friendship = true,
      Gender = Faker.PickRandom<PokemonGender>(),
      HeldItem = $"  {(withId ? _oranBerry.EntityId.ToString() : _oranBerry.Key.Value).ToUpperInvariant()}  ",
      KnownMove = $"  {(withId ? _thunderPunch.EntityId.ToString() : _thunderPunch.Key.Value).ToUpperInvariant()}  ",
      Location = "  Mt. Coronet  ",
      TimeOfDay = Faker.PickRandom<TimeOfDay>()
    };

    CreateOrReplaceEvolutionResult result = await _evolutionService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Evolution);

    EvolutionModel evolution = result.Evolution;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, evolution.Id);
    }
    else
    {
      Assert.NotEqual(default, evolution.Id);
    }
    Assert.Equal(2, evolution.Version);
    Assert.Equal(Actor, evolution.CreatedBy);
    Assert.Equal(DateTime.UtcNow, evolution.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, evolution.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, evolution.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_pikachuForm.EntityId, evolution.Source.Id);
    Assert.Equal(_raichuForm.EntityId, evolution.Target.Id);
    Assert.Equal(payload.Trigger, evolution.Trigger);
    Assert.Equal(_thunderStone.EntityId, evolution.Item?.Id);
    Assert.Equal(payload.Level, evolution.Level);
    Assert.Equal(payload.Friendship, evolution.Friendship);
    Assert.Equal(payload.Gender, evolution.Gender);
    Assert.Equal(_oranBerry.EntityId, evolution.HeldItem?.Id);
    Assert.Equal(_thunderPunch.EntityId, evolution.KnownMove?.Id);
    Assert.Equal(payload.Location.Trim(), evolution.Location);
    Assert.Equal(payload.TimeOfDay, evolution.TimeOfDay);
  }

  [Fact(DisplayName = "It should read an evolution by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _evolution.EntityId;
    EvolutionModel? evolution = await _evolutionService.ReadAsync(id);
    Assert.NotNull(evolution);
    Assert.Equal(id, evolution.Id);
  }

  [Fact(DisplayName = "It should replace an existing evolution.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceEvolutionPayload payload = new()
    {
      Source = $"  {_pikachuForm.EntityId.ToString().ToUpperInvariant()}  ",
      Target = $"  {_raichuForm.Key.Value.ToUpperInvariant()}  ",
      Trigger = EvolutionTrigger.Item,
      Item = $"  {_thunderStone.EntityId.ToString().ToUpperInvariant()}  ",
      Level = 18,
      Friendship = true,
      Gender = Faker.PickRandom<PokemonGender>(),
      HeldItem = $"  {_oranBerry.EntityId.ToString().ToUpperInvariant()}  ",
      KnownMove = $"  {_thunderPunch.EntityId.ToString().ToUpperInvariant()}  ",
      Location = "  Mt. Coronet  ",
      TimeOfDay = Faker.PickRandom<TimeOfDay>()
    };

    CreateOrReplaceEvolutionResult result = await _evolutionService.CreateOrReplaceAsync(payload, _evolution.EntityId);
    Assert.False(result.Created);
    Assert.NotNull(result.Evolution);

    EvolutionModel evolution = result.Evolution;
    Assert.Equal(_evolution.EntityId, evolution.Id);
    Assert.Equal(2, evolution.Version);
    Assert.Equal(Actor, evolution.CreatedBy);
    Assert.Equal(DateTime.UtcNow, evolution.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, evolution.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, evolution.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_pikachuForm.EntityId, evolution.Source.Id);
    Assert.Equal(_raichuForm.EntityId, evolution.Target.Id);
    Assert.Equal(payload.Trigger, evolution.Trigger);
    Assert.Equal(_thunderStone.EntityId, evolution.Item?.Id);
    Assert.Equal(payload.Level, evolution.Level);
    Assert.Equal(payload.Friendship, evolution.Friendship);
    Assert.Equal(payload.Gender, evolution.Gender);
    Assert.Equal(_oranBerry.EntityId, evolution.HeldItem?.Id);
    Assert.Equal(_thunderPunch.EntityId, evolution.KnownMove?.Id);
    Assert.Equal(payload.Location.Trim(), evolution.Location);
    Assert.Equal(payload.TimeOfDay, evolution.TimeOfDay);
  }

  [Fact(DisplayName = "It should return the correct search results (ItemId).")]
  public async Task Given_ItemId_When_SearchAsync_Then_Results()
  {
    Evolution level = new EvolutionBuilder(Faker).WithWorld(World).WithSource(_pikachuForm).WithTarget(_raichuForm).OnLevelUp().WithLevel(new Level(25)).Build();
    Evolution trade = new EvolutionBuilder(Faker).WithWorld(World).WithSource(_pikachuForm).WithTarget(_raichuForm).OnTrade().IsFriendship().Build();
    await _evolutionRepository.SaveAsync([level, trade]);

    SearchEvolutionsPayload payload = new()
    {
      ItemId = _thunderStone.EntityId
    };

    SearchResults<EvolutionModel> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    EvolutionModel evolution = Assert.Single(results.Items);
    Assert.Equal(_evolution.EntityId, evolution.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (Search).")]
  public async Task Given_Search_When_SearchAsync_Then_Results()
  {
    Form raichuAlola = FormBuilder.RaichuAlola(Faker, World, _raichuVariety);
    await _formRepository.SaveAsync(raichuAlola);

    Evolution evolution = new EvolutionBuilder(Faker).WithWorld(World).WithSource(_pikachuForm).WithTarget(raichuAlola).OnItem(_thunderStone).WithLocation(new Location("Alola")).Build();
    await _evolutionRepository.SaveAsync(evolution);

    SearchEvolutionsPayload payload = new();
    payload.Search.Terms.Add(new SearchTerm("%o%"));

    SearchResults<EvolutionModel> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    EvolutionModel model = Assert.Single(results.Items);
    Assert.Equal(evolution.EntityId, model.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (SourceId).")]
  public async Task Given_SourceId_When_SearchAsync_Then_Results()
  {
    SpeciesAggregate pichuSpecies = SpeciesBuilder.Pichu(Faker, World);
    await _speciesRepository.SaveAsync(pichuSpecies);

    Variety pichuVariety = VarietyBuilder.Pichu(Faker, World, pichuSpecies);
    await _varietyRepository.SaveAsync(pichuVariety);

    Form pichu = FormBuilder.Pichu(Faker, World, pichuVariety);
    Form raichuAlola = FormBuilder.RaichuAlola(Faker, World, _raichuVariety);
    await _formRepository.SaveAsync([pichu, raichuAlola]);

    Evolution pichuToPikachu = new EvolutionBuilder(Faker).WithWorld(World).WithSource(pichu).WithTarget(_pikachuForm).OnLevelUp().IsFriendship().Build();
    Evolution pikachuToRaichuAlola = new EvolutionBuilder(Faker).WithWorld(World).WithSource(_pikachuForm).WithTarget(raichuAlola).OnItem(_thunderStone).Build();
    await _evolutionRepository.SaveAsync([pichuToPikachu, pikachuToRaichuAlola]);

    SearchEvolutionsPayload payload = new()
    {
      SourceId = Faker.PickRandom(pichu.EntityId, _pikachuForm.EntityId, _raichuForm.EntityId)
    };

    SearchResults<EvolutionModel> results = await _evolutionService.SearchAsync(payload);

    if (payload.SourceId == pichu.EntityId)
    {
      Assert.Equal(1, results.Total);
      Assert.Equal(pichuToPikachu.EntityId, Assert.Single(results.Items).Id);
    }
    else if (payload.SourceId == _pikachuForm.EntityId)
    {
      Assert.Equal(2, results.Total);
      Assert.Equal(results.Total, results.Items.Count);
      Assert.Contains(results.Items, x => x.Id == _evolution.EntityId);
      Assert.Contains(results.Items, x => x.Id == pikachuToRaichuAlola.EntityId);
    }
    else
    {
      Assert.Equal(0, results.Total);
      Assert.Empty(results.Items);
    }
  }

  [Fact(DisplayName = "It should return the correct search results (TargetId).")]
  public async Task Given_TargetId_When_SearchAsync_Then_Results()
  {
    SpeciesAggregate pichuSpecies = SpeciesBuilder.Pichu(Faker, World);
    await _speciesRepository.SaveAsync(pichuSpecies);

    Variety pichuVariety = VarietyBuilder.Pichu(Faker, World, pichuSpecies);
    await _varietyRepository.SaveAsync(pichuVariety);

    Form pichu = FormBuilder.Pichu(Faker, World, pichuVariety);
    await _formRepository.SaveAsync(pichu);

    Evolution pichuToPikachu = new EvolutionBuilder(Faker).WithWorld(World).WithSource(pichu).WithTarget(_pikachuForm).OnLevelUp().IsFriendship().Build();
    await _evolutionRepository.SaveAsync(pichuToPikachu);

    SearchEvolutionsPayload payload = new()
    {
      TargetId = Faker.PickRandom(_pikachuForm.EntityId, _raichuForm.EntityId)
    };

    SearchResults<EvolutionModel> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    EvolutionModel evolution = Assert.Single(results.Items);
    Assert.Equal((payload.TargetId == _pikachuForm.EntityId ? pichuToPikachu : _evolution).EntityId, evolution.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (Trigger).")]
  public async Task Given_Trigger_When_SearchAsync_Then_Results()
  {
    Evolution level = new EvolutionBuilder(Faker).WithWorld(World).WithSource(_pikachuForm).WithTarget(_raichuForm).OnLevelUp().WithLevel(new Level(25)).Build();
    Evolution trade = new EvolutionBuilder(Faker).WithWorld(World).WithSource(_pikachuForm).WithTarget(_raichuForm).OnTrade().IsFriendship().Build();
    await _evolutionRepository.SaveAsync([level, trade]);

    SearchEvolutionsPayload payload = new()
    {
      Trigger = Faker.PickRandom<EvolutionTrigger>()
    };

    SearchResults<EvolutionModel> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    EvolutionModel evolution = Assert.Single(results.Items);
    switch (evolution.Trigger)
    {
      case EvolutionTrigger.Level:
        Assert.Equal(level.EntityId, evolution.Id);
        break;
      case EvolutionTrigger.Trade:
        Assert.Equal(trade.EntityId, evolution.Id);
        break;
      default:
        Assert.Equal(_evolution.EntityId, evolution.Id);
        break;
    }
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_SearchAsync_Then_Results()
  {
    Evolution level = new EvolutionBuilder(Faker).WithWorld(World).WithSource(_pikachuForm).WithTarget(_raichuForm).OnLevelUp().WithLevel(new Level(25)).Build();
    Evolution trade = new EvolutionBuilder(Faker).WithWorld(World).WithSource(_pikachuForm).WithTarget(_raichuForm).OnTrade().IsFriendship().Build();
    await _evolutionRepository.SaveAsync([level, trade]);

    SearchEvolutionsPayload payload = new()
    {
      Ids = [Guid.Empty, level.EntityId, trade.EntityId],
      Skip = 1,
      Limit = 1
    };
    payload.Sort.Add(new EvolutionSortOption(EvolutionSort.Level, isDescending: true));

    SearchResults<EvolutionModel> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    EvolutionModel evolution = Assert.Single(results.Items);
    Assert.Equal(trade.EntityId, evolution.Id);
  }

  [Fact(DisplayName = "It should update an existing evolution.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    UpdateEvolutionPayload payload = new()
    {
      Level = new Optional<int?>(18),
      Gender = new Optional<PokemonGender?>(Faker.PickRandom<PokemonGender>()),
      HeldItem = new Optional<string>($"  {_oranBerry.Key.Value.ToUpperInvariant()}  "),
      KnownMove = new Optional<string>($"  {_thunderPunch.Key.Value.ToUpperInvariant()}  "),
      Location = new Optional<string>("  Mt. Coronet  "),
      TimeOfDay = new Optional<TimeOfDay?>(Faker.PickRandom<TimeOfDay>())
    };

    EvolutionModel? evolution = await _evolutionService.UpdateAsync(_evolution.EntityId, payload);
    Assert.NotNull(evolution);

    Assert.Equal(_evolution.EntityId, evolution.Id);
    Assert.Equal(2, evolution.Version);
    Assert.Equal(Actor, evolution.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, evolution.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_pikachuForm.EntityId, evolution.Source.Id);
    Assert.Equal(_raichuForm.EntityId, evolution.Target.Id);
    Assert.Equal(_evolution.Trigger, evolution.Trigger);
    Assert.Equal(_thunderStone.EntityId, evolution.Item?.Id);
    Assert.Equal(payload.Level.Value, evolution.Level);
    Assert.Equal(_evolution.Friendship, evolution.Friendship);
    Assert.Equal(payload.Gender.Value, evolution.Gender);
    Assert.Equal(_oranBerry.EntityId, evolution.HeldItem?.Id);
    Assert.Equal(_thunderPunch.EntityId, evolution.KnownMove?.Id);
    Assert.Equal(payload.Location.Value?.Trim(), evolution.Location);
    Assert.Equal(payload.TimeOfDay.Value, evolution.TimeOfDay);
  }
}
