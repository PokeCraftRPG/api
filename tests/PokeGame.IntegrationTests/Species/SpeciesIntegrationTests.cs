using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Logitar;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Permissions;
using PokeGame.Core.Species;
using PokeGame.Core.Species.Models;

namespace PokeGame.Species;

[Trait(Traits.Category, Categories.Integration)]
public class SpeciesIntegrationTests : IntegrationTests
{
  private readonly ISpeciesRepository _speciesRepository;
  private readonly ISpeciesService _speciesService;

  private PokemonSpecies _bulbasaur = null!;
  private PokemonSpecies _pikachu = null!;

  public SpeciesIntegrationTests()
  {
    _speciesRepository = ServiceProvider.GetRequiredService<ISpeciesRepository>();
    _speciesService = ServiceProvider.GetRequiredService<ISpeciesService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _bulbasaur = SpeciesBuilder.Bulbasaur(Faker, Context.World);
    _pikachu = SpeciesBuilder.Pikachu(Faker, Context.World);
    await _speciesRepository.SaveAsync([_bulbasaur, _pikachu]);
  }

  [Theory(DisplayName = "It should create a new species.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 4,
      Category = PokemonCategory.Standard,
      Key = "Charmander",
      Name = " Charmander ",
      Description = "  The flame on its tail indicates Charmander's life force. If it is healthy, the flame burns intensely.  ",
      BaseFriendship = 70,
      CatchRate = 45,
      GrowthRate = GrowthRate.MediumSlow,
      EggCycles = 20,
      EggGroups = new EggGroupsModel(EggGroup.Monster, EggGroup.Dragon)
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id);
    SpeciesModel species = result.Species;
    Assert.NotNull(species);
    Assert.True(result.Created);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, species.Id);
    }
    Assert.Equal(3, species.Version);
    Assert.Equal(Actor, species.CreatedBy);
    Assert.Equal(DateTime.UtcNow, species.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.True(species.CreatedOn < species.UpdatedOn);

    Assert.Equal(payload.Number, species.Number);
    Assert.Equal(payload.Category, species.Category);
    Assert.Equal(payload.Key.ToLowerInvariant(), species.Key);
    Assert.Equal(payload.Name.Trim(), species.Name);
    Assert.Equal(payload.Description.Trim(), species.Description);
    Assert.Equal(payload.BaseFriendship, species.BaseFriendship);
    Assert.Equal(payload.CatchRate, species.CatchRate);
    Assert.Equal(payload.GrowthRate, species.GrowthRate);
    Assert.Equal(payload.EggCycles, species.EggCycles);
    Assert.Equal(payload.EggGroups.Primary, species.EggGroups.Primary);
    Assert.Equal(payload.EggGroups.Secondary, species.EggGroups.Secondary);
  }

  [Fact(DisplayName = "It should read a species by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: _bulbasaur.EntityId);
    Assert.NotNull(species);
    Assert.Equal(_bulbasaur.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should read a species by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    SpeciesModel? species = await _speciesService.ReadAsync(id: null, key: _bulbasaur.Key.Value);
    Assert.NotNull(species);
    Assert.Equal(_bulbasaur.EntityId, species.Id);
  }

  [Fact(DisplayName = "It should replace an existing species.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = _bulbasaur.Number.Value,
      Category = _bulbasaur.Category,
      Key = "Ivysaur",
      Name = " Ivysaur ",
      Description = "  When the bulb on its back grows large, it appears to lose the ability to stand on its hind legs.  ",
      BaseFriendship = 50,
      CatchRate = 90,
      GrowthRate = GrowthRate.Fast,
      EggCycles = 15,
      EggGroups = new EggGroupsModel(EggGroup.Monster, EggGroup.Grass)
    };
    Guid id = _bulbasaur.EntityId;

    CreateOrReplaceSpeciesResult result = await _speciesService.CreateOrReplaceAsync(payload, id);
    SpeciesModel species = result.Species;
    Assert.NotNull(species);
    Assert.False(result.Created);

    Assert.Equal(id, species.Id);
    Assert.Equal(_bulbasaur.Version + 4, species.Version);
    Assert.Equal(_bulbasaur.CreatedBy, species.CreatedBy.ToActorId());
    Assert.Equal(_bulbasaur.CreatedOn.AsUniversalTime(), species.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Number, species.Number);
    Assert.Equal(payload.Category, species.Category);
    Assert.Equal(payload.Key.ToLowerInvariant(), species.Key);
    Assert.Equal(payload.Name.Trim(), species.Name);
    Assert.Equal(payload.Description.Trim(), species.Description);
    Assert.Equal(payload.BaseFriendship, species.BaseFriendship);
    Assert.Equal(payload.CatchRate, species.CatchRate);
    Assert.Equal(payload.GrowthRate, species.GrowthRate);
    Assert.Equal(payload.EggCycles, species.EggCycles);
    Assert.Equal(payload.EggGroups.Primary, species.EggGroups.Primary);
    Assert.Equal(payload.EggGroups.Secondary, species.EggGroups.Secondary);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NotFound_When_Search_Then_Empty()
  {
    SearchSpeciesPayload payload = new();
    payload.Ids.Add(Guid.Empty);

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the species was not read.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder().Build();
    Assert.Null(await _speciesService.ReadAsync(id: _bulbasaur.EntityId, key: _pikachu.Key.Value));
  }

  [Fact(DisplayName = "It should return null when the species was not updated.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _speciesService.UpdateAsync(Guid.Empty, new UpdateSpeciesPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Found_When_Search_Then_Results()
  {
    SearchSpeciesPayload payload = new();

    SearchResults<SpeciesModel> results = await _speciesService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    Assert.Equal(results.Total, results.Items.Count);
    Assert.Contains(results.Items, species => species.Id == _bulbasaur.EntityId);
    Assert.Contains(results.Items, species => species.Id == _pikachu.EntityId);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when creating or replacing a species.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    Guid id = Guid.NewGuid();
    if (exists)
    {
      id = _pikachu.EntityId;
    }

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = exists ? _pikachu.Number.Value : 150,
      Category = exists ? _pikachu.Category : PokemonCategory.Legendary,
      Key = _bulbasaur.Key.Value,
      CatchRate = exists ? _pikachu.CatchRate.Value : (byte)3,
      GrowthRate = exists ? _pikachu.GrowthRate : GrowthRate.Slow,
      EggCycles = exists ? _pikachu.EggCycles.Value : (byte)120,
      EggGroups = exists ? new EggGroupsModel(_pikachu.EggGroups) : new EggGroupsModel(EggGroup.NoEggsDiscovered)
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _speciesService.CreateOrReplaceAsync(payload, id));
    Assert.NotNull(Context.World);
    Assert.Equal(Context.World.EntityId, exception.WorldId);
    Assert.Equal(PokemonSpecies.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_bulbasaur.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating an existing species.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Guid id = _pikachu.EntityId;
    UpdateSpeciesPayload payload = new()
    {
      Key = _bulbasaur.Key.Value
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _speciesService.UpdateAsync(id, payload));
    Assert.NotNull(Context.World);
    Assert.Equal(Context.World.EntityId, exception.WorldId);
    Assert.Equal(PokemonSpecies.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_bulbasaur.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new species.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = 999,
      Category = PokemonCategory.Standard,
      Key = "denied-species",
      CatchRate = 45,
      GrowthRate = GrowthRate.MediumFast,
      EggCycles = 20,
      EggGroups = new EggGroupsModel(EggGroup.Field)
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _speciesService.CreateOrReplaceAsync(payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.CreateSpecies, exception.Action);
    Assert.Null(exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing species.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceSpeciesPayload payload = new()
    {
      Number = _bulbasaur.Number.Value,
      Category = _bulbasaur.Category,
      Key = "denied-species",
      CatchRate = _bulbasaur.CatchRate.Value,
      GrowthRate = _bulbasaur.GrowthRate,
      EggCycles = _bulbasaur.EggCycles.Value,
      EggGroups = new EggGroupsModel(_bulbasaur.EggGroups)
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _speciesService.CreateOrReplaceAsync(payload, _bulbasaur.EntityId));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_bulbasaur.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing species.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    UpdateSpeciesPayload payload = new();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _speciesService.UpdateAsync(_bulbasaur.EntityId, payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_bulbasaur.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many species were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    var exception = await Assert.ThrowsAsync<TooManyResultsException<SpeciesModel>>(async () => await _speciesService.ReadAsync(id: _bulbasaur.EntityId, key: _pikachu.Key.Value));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing species.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _bulbasaur.EntityId;
    UpdateSpeciesPayload payload = new()
    {
      Name = new Optional<string>(" Venusaur "),
      Description = new Optional<string>("  After a rainy day, the flower on its back smells stronger. The scent attracts other Pokémon.  ")
    };

    SpeciesModel? species = await _speciesService.UpdateAsync(id, payload);
    Assert.NotNull(species);

    Assert.Equal(id, species.Id);
    Assert.Equal(_bulbasaur.Version + 2, species.Version);
    Assert.Equal(_bulbasaur.CreatedBy, species.CreatedBy.ToActorId());
    Assert.Equal(_bulbasaur.CreatedOn.AsUniversalTime(), species.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, species.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, species.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_bulbasaur.Number.Value, species.Number);
    Assert.Equal(_bulbasaur.Category, species.Category);
    Assert.Equal(_bulbasaur.Key.Value, species.Key);
    Assert.Equal(payload.Name.Value?.Trim(), species.Name);
    Assert.Equal(payload.Description.Value?.Trim(), species.Description);
    Assert.Equal(_bulbasaur.BaseFriendship.Value, species.BaseFriendship);
    Assert.Equal(_bulbasaur.CatchRate.Value, species.CatchRate);
    Assert.Equal(_bulbasaur.GrowthRate, species.GrowthRate);
    Assert.Equal(_bulbasaur.EggCycles.Value, species.EggCycles);
    Assert.Equal(_bulbasaur.EggGroups.Primary, species.EggGroups.Primary);
    Assert.Equal(_bulbasaur.EggGroups.Secondary, species.EggGroups.Secondary);
  }
}
