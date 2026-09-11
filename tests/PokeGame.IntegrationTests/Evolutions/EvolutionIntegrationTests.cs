using FluentValidation;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Abilities;
using PokeGame.Core.Evolutions;
using PokeGame.Core.Evolutions.Models;
using PokeGame.Core.Forms;
using PokeGame.Core.Items;
using PokeGame.Core.Moves;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Species;
using PokeGame.Core.Varieties;

namespace PokeGame.Evolutions;

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

  private Evolution _evolution = null!;
  private Form _source = null!;
  private Form _target = null!;
  private Item _item = null!;
  private Move _move = null!;
  private EvolutionDto _seeded = null!;

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

    (_, _, _source) = await CreateFormAsync("bulbasaur");
    (_, _, _target) = await CreateFormAsync("charmander");
    _item = ItemBuilder.SuperPotion(Faker, Context.World);
    _move = MoveBuilder.Ember(Faker, Context.World);
    await _itemRepository.SaveAsync(_item);
    await _moveRepository.SaveAsync(_move);

    _evolution = new Evolution(Context.World!, _source, _target, EvolutionTrigger.LeveledUp, actorId: Context.ActorId);
    _evolution.SetConditions(new Level(16), friendship: true, Gender.Female, item: null, _move, Location.TryCreate("Route 1"), TimeOfDay.Day, Context.ActorId);
    await _evolutionRepository.SaveAsync(_evolution);

    _seeded = (await _evolutionService.ReadAsync(_evolution.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new evolution.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceEvolutionPayload payload = CreateItemEvolutionPayload(_source.EntityId, _target.EntityId, _item.EntityId, _move.EntityId);
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceEvolutionResult result = await _evolutionService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    EvolutionDto evolution = result.Evolution;

    if (id.HasValue)
    {
      Assert.Equal(id.Value, evolution.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, evolution.Id);
    }
    Assert.Equal(2, evolution.Version);
    Assert.Equal(Actor, evolution.CreatedBy);
    Assert.Equal(DateTime.UtcNow, evolution.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(evolution.CreatedBy, evolution.UpdatedBy);
    Assert.True(evolution.CreatedOn <= evolution.UpdatedOn);

    AssertEvolution(payload, evolution);
  }

  [Fact(DisplayName = "It should read an evolution by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    EvolutionDto? evolution = await _evolutionService.ReadAsync(_evolution.EntityId);
    Assert.NotNull(evolution);
    Assert.Equal(_evolution.EntityId, evolution.Id);
  }

  [Fact(DisplayName = "It should replace an existing evolution.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceEvolutionPayload payload = CreateUpdatedLevelPayload(_source.EntityId, _target.EntityId);

    CreateOrReplaceEvolutionResult result = await _evolutionService.CreateOrReplaceAsync(payload, _evolution.EntityId);
    Assert.False(result.Created);
    EvolutionDto evolution = result.Evolution;

    Assert.Equal(_evolution.EntityId, evolution.Id);
    Assert.Equal(3, evolution.Version);
    Assert.Equal(_seeded.CreatedBy, evolution.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, evolution.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, evolution.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, evolution.UpdatedOn, TimeSpan.FromSeconds(10));
    AssertEvolution(payload, evolution);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchEvolutionsPayload payload = new() { Limit = 10 };
    SearchResults<EvolutionDto> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no evolution was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Assert.Null(await _evolutionService.ReadAsync(Guid.NewGuid()));
  }

  [Fact(DisplayName = "It should return null when the evolution was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _evolutionService.UpdateAsync(Guid.NewGuid(), new UpdateEvolutionPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    (_, _, Form squirtle) = await CreateFormAsync("squirtle");
    Item item = ItemBuilder.Antidote(Faker, Context.World);
    await _itemRepository.SaveAsync(item);
    Evolution extra = new Evolution(Context.World!, _target, squirtle, EvolutionTrigger.ItemUsed, item, Context.ActorId);
    extra.SetConditions(new Level(36), friendship: false, Gender.Male, item, move: null, Location.TryCreate("Mt Moon"), TimeOfDay.Night, Context.ActorId);
    await _evolutionRepository.SaveAsync(extra);

    SearchEvolutionsPayload payload = new()
    {
      Source = _target.Key.Value,
      Trigger = EvolutionTrigger.ItemUsed,
      Limit = 10
    };

    SearchResults<EvolutionDto> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(1, results.Total);
    EvolutionDto evolution = Assert.Single(results.Items);
    Assert.Equal(extra.EntityId, evolution.Id);
  }

  [Theory(DisplayName = "It should filter search results by source.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_SourceFilter_When_Search_Then_Results(bool byId)
  {
    SearchEvolutionsPayload payload = new()
    {
      Source = byId ? _source.EntityId.ToString() : _source.Key.Value,
      Limit = 10
    };

    SearchResults<EvolutionDto> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(1, results.Total);
    Assert.Equal(_evolution.EntityId, Assert.Single(results.Items).Id);
  }

  [Theory(DisplayName = "It should filter search results by target.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_TargetFilter_When_Search_Then_Results(bool byId)
  {
    SearchEvolutionsPayload payload = new()
    {
      Target = byId ? _target.EntityId.ToString() : _target.Key.Value,
      Limit = 10
    };

    SearchResults<EvolutionDto> results = await _evolutionService.SearchAsync(payload);
    Assert.Equal(1, results.Total);
    Assert.Equal(_evolution.EntityId, Assert.Single(results.Items).Id);
  }

  [Fact(DisplayName = "It should throw ImmutablePropertyException when replacing an evolution with a different trigger.")]
  public async Task Given_DifferentTrigger_When_Replace_Then_ImmutablePropertyException()
  {
    CreateOrReplaceEvolutionPayload payload = CreateUpdatedLevelPayload(_source.EntityId, _target.EntityId);
    payload.Trigger = EvolutionTrigger.Traded;

    ImmutablePropertyException<EvolutionTrigger> exception = await Assert.ThrowsAsync<ImmutablePropertyException<EvolutionTrigger>>(
      async () => await _evolutionService.CreateOrReplaceAsync(payload, _evolution.EntityId));
    Assert.Equal(Evolution.EntityKind, exception.EntityKind);
    Assert.Equal(_evolution.EntityId, exception.EntityId);
    Assert.Equal(_seeded.Trigger, exception.ExpectedValue);
    Assert.Equal(payload.Trigger, exception.AttemptedValue);
    Assert.Equal(nameof(payload.Trigger), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the source form does not exist.")]
  public async Task Given_MissingSource_When_Create_Then_EntityNotFoundException()
  {
    CreateOrReplaceEvolutionPayload payload = CreateUpdatedLevelPayload(Guid.NewGuid(), _target.EntityId);

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _evolutionService.CreateOrReplaceAsync(payload));
    Assert.Equal(Form.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(nameof(payload.SourceId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw EntityNotFoundException when the item does not exist.")]
  public async Task Given_MissingItem_When_Create_Then_EntityNotFoundException()
  {
    CreateOrReplaceEvolutionPayload payload = CreateItemEvolutionPayload(_source.EntityId, _target.EntityId, Guid.NewGuid(), _move.EntityId);

    EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
      async () => await _evolutionService.CreateOrReplaceAsync(payload));
    Assert.Equal(Item.EntityKind, exception.Data["EntityKind"]);
    Assert.Equal(nameof(payload.ItemId), exception.Data["PropertyName"]);
  }

  [Fact(DisplayName = "It should throw EvolutionItemRequiredException when creating an item-triggered evolution without item.")]
  public async Task Given_ItemTriggerWithoutItem_When_Create_Then_EvolutionItemRequiredException()
  {
    CreateOrReplaceEvolutionPayload payload = CreateUpdatedLevelPayload(_source.EntityId, _target.EntityId);
    payload.Trigger = EvolutionTrigger.ItemUsed;
    payload.ItemId = null;

    EvolutionItemRequiredException exception = await Assert.ThrowsAsync<EvolutionItemRequiredException>(
      async () => await _evolutionService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceEvolutionPayload payload = new()
    {
      SourceId = _source.EntityId,
      TargetId = _source.EntityId,
      Trigger = EvolutionTrigger.LeveledUp
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _evolutionService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateEvolutionPayload payload = new()
    {
      Level = new Optional<int?>(0)
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _evolutionService.UpdateAsync(_evolution.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating an evolution.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _evolutionService.CreateOrReplaceAsync(CreateUpdatedLevelPayload(_source.EntityId, _target.EntityId)));
    Assert.Equal("CreateEvolution", exception.Data["Action"]);
    Assert.Null(exception.Data["Resource"]);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an evolution.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = KrakenarFactory.Instance.NewUser(Faker);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _evolutionService.CreateOrReplaceAsync(CreateUpdatedLevelPayload(_source.EntityId, _target.EntityId), _evolution.EntityId));
    Assert.Equal("Update", exception.Data["Action"]);
    Assert.Equal(_evolution.GetEntity().ToString(), exception.Data["Resource"]);
  }

  [Fact(DisplayName = "It should update an existing evolution.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    UpdateEvolutionPayload payload = new()
    {
      Level = new Optional<int?>(20),
      Friendship = false,
      Gender = new Optional<Gender?>(Gender.Male),
      ItemId = new Optional<Guid?>(_item.EntityId),
      Location = new Optional<string>("Pallet Town"),
      TimeOfDay = new Optional<TimeOfDay?>(TimeOfDay.Night)
    };

    EvolutionDto? evolution = await _evolutionService.UpdateAsync(_evolution.EntityId, payload);
    Assert.NotNull(evolution);
    Assert.Equal(_evolution.EntityId, evolution.Id);
    Assert.Equal(3, evolution.Version);
    Assert.Equal(20, evolution.Level);
    Assert.False(evolution.Friendship);
    Assert.Equal(Gender.Male, evolution.Gender);
    Assert.NotNull(evolution.Item);
    Assert.Equal(_item.EntityId, evolution.Item.Id);
    Assert.Equal("Pallet Town", evolution.Location);
    Assert.Equal(TimeOfDay.Night, evolution.TimeOfDay);
  }

  private CreateOrReplaceEvolutionPayload CreateItemEvolutionPayload(Guid sourceId, Guid targetId, Guid itemId, Guid moveId) => new()
  {
    SourceId = sourceId,
    TargetId = targetId,
    Trigger = EvolutionTrigger.ItemUsed,
    Level = 36,
    Friendship = false,
    Gender = Gender.Male,
    ItemId = itemId,
    MoveId = moveId,
    Location = "Route 2",
    TimeOfDay = TimeOfDay.Night
  };

  private static CreateOrReplaceEvolutionPayload CreateUpdatedLevelPayload(Guid sourceId, Guid targetId) => new()
  {
    SourceId = sourceId,
    TargetId = targetId,
    Trigger = EvolutionTrigger.LeveledUp,
    Level = 18,
    Friendship = false,
    Gender = Gender.Female,
    Location = "Route 3",
    TimeOfDay = TimeOfDay.Evening
  };

  private static void AssertEvolution(CreateOrReplaceEvolutionPayload payload, EvolutionDto evolution)
  {
    Assert.Equal(payload.SourceId, evolution.Source.Id);
    Assert.Equal(payload.TargetId, evolution.Target.Id);
    Assert.Equal(payload.Trigger, evolution.Trigger);
    Assert.Equal(payload.Level, evolution.Level);
    Assert.Equal(payload.Friendship, evolution.Friendship);
    Assert.Equal(payload.Gender, evolution.Gender);
    Assert.Equal(payload.MoveId, evolution.Move?.Id);
    Assert.Equal(payload.ItemId, evolution.Item?.Id);
    Assert.Equal(payload.Location?.Trim(), evolution.Location);
    Assert.Equal(payload.TimeOfDay, evolution.TimeOfDay);
  }

  private async Task<(PokemonSpecies species, Variety variety, Form form)> CreateFormAsync(string key)
  {
    PokemonSpecies species = key switch
    {
      "bulbasaur" => SpeciesBuilder.Bulbasaur(Faker, Context.World),
      "charmander" => SpeciesBuilder.Charmander(Faker, Context.World),
      "squirtle" => SpeciesBuilder.Squirtle(Faker, Context.World),
      _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
    await _speciesRepository.SaveAsync(species);

    Variety variety = key switch
    {
      "bulbasaur" => VarietyBuilder.Bulbasaur(Faker, species, Context.World),
      "charmander" => VarietyBuilder.Charmander(Faker, species, Context.World),
      "squirtle" => VarietyBuilder.Squirtle(Faker, species, Context.World),
      _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
    await _varietyRepository.SaveAsync(variety);

    Ability ability = key switch
    {
      "bulbasaur" => AbilityBuilder.Overgrow(Faker, Context.World),
      "charmander" => AbilityBuilder.Blaze(Faker, Context.World),
      "squirtle" => AbilityBuilder.Torrent(Faker, Context.World),
      _ => throw new ArgumentOutOfRangeException(nameof(key))
    };
    await ServiceProvider.GetRequiredService<IAbilityRepository>().SaveAsync(ability);

    Form form = key switch
    {
      "bulbasaur" => FormBuilder.Bulbasaur(Faker, variety, ability, Context.World),
      "charmander" => FormBuilder.Charmander(Faker, variety, ability, Context.World),
      "squirtle" => FormBuilder.Squirtle(Faker, variety, ability, Context.World),
      _ => throw new ArgumentOutOfRangeException(nameof(key))
    };

    await _formRepository.SaveAsync(form);
    return (species, variety, form);
  }
}
