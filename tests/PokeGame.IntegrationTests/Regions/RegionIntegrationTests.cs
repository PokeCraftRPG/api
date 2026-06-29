using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Worlds;

namespace PokeGame.Regions;

[Trait(Traits.Category, Categories.Integration)]
public class RegionIntegrationTests : IntegrationTests
{
  private readonly IRegionRepository _regionRepository;
  private readonly IRegionService _regionService;
  private readonly IWorldRepository _worldRepository;

  private Region _region = null!;

  public RegionIntegrationTests() : base()
  {
    _regionRepository = ServiceProvider.GetRequiredService<IRegionRepository>();
    _regionService = ServiceProvider.GetRequiredService<IRegionService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _region = new RegionBuilder(Faker).WithWorld(Context.World).Build();
    _regionRepository.Add(_region);
    await Context.SaveChangesAsync();
  }

  [Theory(DisplayName = "It should create a new region.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "Kanto",
      Name = " Kanto ",
      Description = "  Kanto is Pokémon’s first region, based on Japan’s Kantō area, home to Pallet Town, Professor Oak, Team Rocket, and the Indigo League.  "
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    RegionModel region = result.Region;
    Assert.NotNull(region);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, region.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, region.Id);
    }
    Assert.Equal(1, region.Version);
    Assert.Equal(Actor, region.CreatedBy);
    Assert.Equal(DateTime.UtcNow, region.CreatedOn, TimeSpan.FromSeconds(1));
    Assert.Equal(region.CreatedBy, region.UpdatedBy);
    Assert.Equal(region.CreatedOn, region.UpdatedOn);

    Assert.Equal(SlugHelper.Format(payload.Key), region.Key);
    Assert.Equal(payload.Name.Trim(), region.Name);
    Assert.Equal(payload.Description.Trim(), region.Description);
  }

  [Fact(DisplayName = "It should read a region by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    RegionModel? region = await _regionService.ReadAsync(_region.Id);
    Assert.NotNull(region);
    Assert.Equal(_region.Id, region.Id);
  }

  [Fact(DisplayName = "It should read a region by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    RegionModel? region = await _regionService.ReadAsync(id: null, $" {_region.Key.ToUpperInvariant()} ");
    Assert.NotNull(region);
    Assert.Equal(_region.Id, region.Id);
  }

  [Fact(DisplayName = "It should replace an existing region.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "Kanto",
      Name = " Kanto ",
      Description = "  Kanto is Pokémon’s first region, based on Japan’s Kantō area, home to Pallet Town, Professor Oak, Team Rocket, and the Indigo League.  "
    };

    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, _region.Id);
    Assert.False(result.Created);
    RegionModel region = result.Region;
    Assert.NotNull(region);

    Assert.Equal(_region.Id, region.Id);
    Assert.Equal(_region.Version, region.Version);
    Assert.Equal(_region.CreatedBy, region.CreatedBy.Id);
    Assert.Equal(_region.CreatedOn, region.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(SlugHelper.Format(payload.Key), region.Key);
    Assert.Equal(payload.Name.Trim(), region.Name);
    Assert.Equal(payload.Description.Trim(), region.Description);
  }

  [Fact(DisplayName = "It should return empty search results when no region is matching.")]
  public async Task Given_NoneMatching_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder().Build();

    SearchResults<RegionModel> results = await _regionService.SearchAsync(new SearchRegionsPayload());
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the user does not own the world.")]
  public async Task Given_NotOwner_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder().Build();

    Assert.Null(await _regionService.ReadAsync(_region.Id, $" {_region.Key.ToUpperInvariant()} "));
  }

  [Fact(DisplayName = "It should return null when the region does not exist.")]
  public async Task Given_NotExist_When_Update_Then_NullReturned()
  {
    Assert.Null(await _regionService.UpdateAsync(Guid.Empty, new UpdateRegionPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_CorrectResults()
  {
    World world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("the-new-world").Build();
    _worldRepository.Add(world);

    Region kanto = RegionBuilder.Kanto(Faker, world);
    Region johto = RegionBuilder.Johto(Faker, world);
    Region hoenn = RegionBuilder.Hoenn(Faker, world);
    Region sinnoh = RegionBuilder.Sinnoh(Faker, world);
    _regionRepository.Add(kanto, johto, hoenn, sinnoh);

    Context.World = world;
    await Context.SaveChangesAsync();

    SearchRegionsPayload payload = new()
    {
      Skip = 1,
      Limit = 1
    };
    payload.Ids.AddRange(kanto.Id, johto.Id, hoenn.Id, Guid.Empty);
    payload.Search.Operator = SearchOperator.Or;
    payload.Search.Terms.Add(new SearchTerm("%to"));
    payload.Search.Terms.Add(new SearchTerm("SiNnOh"));
    payload.Sort.Add(new RegionSortOption(RegionSort.CreatedOn, isDescending: true));

    SearchResults<RegionModel> results = await _regionService.SearchAsync(payload);
    Assert.Equal(2, results.Total);
    Assert.Equal(kanto.Id, Assert.Single(results.Items).Id);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (CreateOrReplace).")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    Region kanto = RegionBuilder.Kanto(Faker, Context.World);
    _regionRepository.Add(kanto);
    await Context.SaveChangesAsync();

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = _region.Key
    };
    Guid? id = exists ? kanto.Id : null;

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _regionService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId, exception.WorldId);
    Assert.Equal(Region.ResourceKind, exception.ResourceKind);
    if (id.HasValue)
    {
      Assert.Equal(id.Value, exception.ResourceId);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, exception.ResourceId);
    }
    Assert.Equal(_region.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (Update).")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Region kanto = RegionBuilder.Kanto(Faker, Context.World);
    _regionRepository.Add(kanto);
    await Context.SaveChangesAsync();

    UpdateRegionPayload payload = new()
    {
      Key = _region.Key
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _regionService.UpdateAsync(kanto.Id, payload));
    Assert.Equal(Context.WorldId, exception.WorldId);
    Assert.Equal(Region.ResourceKind, exception.ResourceKind);
    Assert.Equal(kanto.Id, exception.ResourceId);
    Assert.Equal(_region.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new region.")]
  public async Task Given_NotExist_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Context.World = world;
    await Context.SaveChangesAsync();

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "Kanto",
      Name = " Kanto ",
      Description = "  Kanto is Pokémon’s first region, based on Japan’s Kantō area, home to Pallet Town, Professor Oak, Team Rocket, and the Indigo League.  "
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.CreateRegion, exception.Action);
    Assert.Equal(world.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing region.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Region region = new RegionBuilder(Faker).WithWorld(world).Build();
    _regionRepository.Add(region);

    Context.World = world;
    await Context.SaveChangesAsync();

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "Kanto",
      Name = " Kanto ",
      Description = "  Kanto is Pokémon’s first region, based on Japan’s Kantō area, home to Pallet Town, Professor Oak, Team Rocket, and the Indigo League.  "
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.CreateOrReplaceAsync(payload, region.Id));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(region.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing region.")]
  public async Task Given_Exists_When_Update_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Region region = new RegionBuilder(Faker).WithWorld(world).Build();
    _regionRepository.Add(region);

    Context.World = world;
    await Context.SaveChangesAsync();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.UpdateAsync(region.Id, new UpdateRegionPayload()));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(region.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many region were found.")]
  public async Task Given_ManyRegionFound_When_Read_Then_TooManyResultsException()
  {
    Region kanto = RegionBuilder.Kanto(Faker, Context.World);
    _regionRepository.Add(kanto);
    await Context.SaveChangesAsync();

    var exception = await Assert.ThrowsAsync<TooManyResultsException<RegionModel>>(async () => await _regionService.ReadAsync(_region.Id, $" {kanto.Key.ToUpperInvariant()} "));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing region.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _region.Id;
    UpdateRegionPayload payload = new()
    {
      Key = "Kanto",
      Name = new Optional<string>(" Kanto "),
      Description = new Optional<string>("  Kanto is Pokémon’s first region, based on Japan’s Kantō area, home to Pallet Town, Professor Oak, Team Rocket, and the Indigo League.  ")
    };

    RegionModel? region = await _regionService.UpdateAsync(id, payload);
    Assert.NotNull(region);

    Assert.Equal(id, region.Id);
    Assert.Equal(_region.Version, region.Version);
    Assert.Equal(_region.CreatedBy, region.CreatedBy.Id);
    Assert.Equal(_region.CreatedOn, region.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(SlugHelper.Format(payload.Key), region.Key);
    Assert.Equal(payload.Name.Value?.Trim(), region.Name);
    Assert.Equal(payload.Description.Value?.Trim(), region.Description);
  }
}
