using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Logitar;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Regions;

[Trait(Traits.Category, Categories.Integration)]
public class RegionIntegrationTests : IntegrationTests
{
  private readonly IRegionRepository _regionRepository;
  private readonly IRegionService _regionService;

  private Region _region = null!;
  private Region _johto = null!;

  public RegionIntegrationTests()
  {
    _regionRepository = ServiceProvider.GetRequiredService<IRegionRepository>();
    _regionService = ServiceProvider.GetRequiredService<IRegionService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _region = new RegionBuilder(Faker).WithWorld(Context.World).Build();
    _johto = RegionBuilder.Johto(Faker, Context.World);
    await _regionRepository.SaveAsync([_region, _johto]);
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
      Description = "  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla aliquet, dolor sed sollicitudin biam.  "
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id);
    RegionModel region = result.Region;
    Assert.NotNull(region);
    Assert.True(result.Created);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, region.Id);
    }
    Assert.Equal(3, region.Version);
    Assert.Equal(Actor, region.CreatedBy);
    Assert.Equal(DateTime.UtcNow, region.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.True(region.CreatedOn < region.UpdatedOn);

    Assert.Equal(payload.Key.ToLowerInvariant(), region.Key);
    Assert.Equal(payload.Name.Trim(), region.Name);
    Assert.Equal(payload.Description.Trim(), region.Description);
  }

  [Fact(DisplayName = "It should read a region by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    RegionModel? region = await _regionService.ReadAsync(_region.EntityId);
    Assert.NotNull(region);
    Assert.Equal(_region.EntityId, region.Id);
  }

  [Fact(DisplayName = "It should read a region by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    RegionModel? region = await _regionService.ReadAsync(id: null, _region.Key.Value);
    Assert.NotNull(region);
    Assert.Equal(_region.EntityId, region.Id);
  }

  [Fact(DisplayName = "It should replace an existing region.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "Kanto",
      Name = " Kanto ",
      Description = "  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla aliquet, dolor sed sollicitudin biam.  "
    };
    Guid id = _region.EntityId;

    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id);
    RegionModel region = result.Region;
    Assert.NotNull(region);
    Assert.False(result.Created);

    Assert.Equal(id, region.Id);
    Assert.Equal(_region.Version + 3, region.Version);
    Assert.Equal(_region.CreatedBy, region.CreatedBy.ToActorId());
    Assert.Equal(_region.CreatedOn.AsUniversalTime(), region.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), region.Key);
    Assert.Equal(payload.Name.Trim(), region.Name);
    Assert.Equal(payload.Description.Trim(), region.Description);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NotFound_When_Search_Then_Empty()
  {
    SearchRegionsPayload payload = new();
    payload.Ids.Add(Guid.Empty);

    SearchResults<RegionModel> results = await _regionService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the region was not read.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder().Build();
    Assert.Null(await _regionService.ReadAsync(_region.EntityId, _region.Key.Value));
  }

  [Fact(DisplayName = "It should return null when the region was not updated.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _regionService.UpdateAsync(Guid.Empty, new UpdateRegionPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Found_When_Search_Then_Results()
  {
    SearchRegionsPayload payload = new();

    SearchResults<RegionModel> results = await _regionService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    Assert.Equal(results.Total, results.Items.Count);
    Assert.Contains(results.Items, region => region.Id == _region.EntityId);
    Assert.Contains(results.Items, region => region.Id == _johto.EntityId);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when creating or replacing a region.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    Guid id = Guid.NewGuid();
    if (exists)
    {
      id = _johto.EntityId;
    }

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = _region.Key.Value
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _regionService.CreateOrReplaceAsync(payload, id));
    Assert.NotNull(Context.World);
    Assert.Equal(Context.World.EntityId, exception.WorldId);
    Assert.Equal(Region.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_region.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating an existing region.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Guid id = _johto.EntityId;
    UpdateRegionPayload payload = new()
    {
      Key = _region.Key.Value
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _regionService.UpdateAsync(id, payload));
    Assert.NotNull(Context.World);
    Assert.Equal(Context.World.EntityId, exception.WorldId);
    Assert.Equal(Region.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_region.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new region.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "denied-region"
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.CreateOrReplaceAsync(payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.CreateRegion, exception.Action);
    Assert.Null(exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing region.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "denied-region"
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.CreateOrReplaceAsync(payload, _region.EntityId));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_region.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing region.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    UpdateRegionPayload payload = new();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.UpdateAsync(_region.EntityId, payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_region.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many regions were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    var exception = await Assert.ThrowsAsync<TooManyResultsException<RegionModel>>(async () => await _regionService.ReadAsync(_region.EntityId, _johto.Key.Value));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing region.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _region.EntityId;
    UpdateRegionPayload payload = new()
    {
      Name = new Optional<string>(" Kanto "),
      Description = new Optional<string>("  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla aliquet, dolor sed sollicitudin biam.  ")
    };

    RegionModel? region = await _regionService.UpdateAsync(id, payload);
    Assert.NotNull(region);

    Assert.Equal(id, region.Id);
    Assert.Equal(_region.Version + 2, region.Version);
    Assert.Equal(_region.CreatedBy, region.CreatedBy.ToActorId());
    Assert.Equal(_region.CreatedOn.AsUniversalTime(), region.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_region.Key.Value, region.Key);
    Assert.Equal(payload.Name.Value?.Trim(), region.Name);
    Assert.Equal(payload.Description.Value?.Trim(), region.Description);
  }
}
