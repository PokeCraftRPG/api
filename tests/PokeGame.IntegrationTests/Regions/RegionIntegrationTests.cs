using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Permissions;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Models;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;

namespace PokeGame.Regions;

[Trait(Traits.Category, Categories.Integration)]
public class RegionIntegrationTests : IntegrationTests
{
  private readonly IRegionRepository _regionRepository;
  private readonly IRegionService _regionService;

  private Region _region = null!;
  private RegionDto _seeded = null!;

  public RegionIntegrationTests()
  {
    _regionRepository = ServiceProvider.GetRequiredService<IRegionRepository>();
    _regionService = ServiceProvider.GetRequiredService<IRegionService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _region = RegionBuilder.Kanto(Faker, Context.World);
    await _regionRepository.SaveAsync(_region);

    _seeded = (await _regionService.ReadAsync(_region.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new region.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceRegionPayload payload = CreateJohtoPayload();
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    RegionDto region = result.Region;
    Assert.NotNull(region);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, region.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, region.Id);
    }
    Assert.Equal(2, region.Version);
    Assert.Equal(Actor, region.CreatedBy);
    Assert.Equal(DateTime.UtcNow, region.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(region.CreatedBy, region.UpdatedBy);
    Assert.True(region.CreatedOn < region.UpdatedOn);

    AssertJohto(payload, region);
  }

  [Fact(DisplayName = "It should read a region by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    RegionDto? region = await _regionService.ReadAsync(_region.EntityId);
    Assert.NotNull(region);
    Assert.Equal(_region.EntityId, region.Id);
  }

  [Fact(DisplayName = "It should read a region by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    RegionDto? region = await _regionService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(region);
    Assert.Equal(_region.EntityId, region.Id);
  }

  [Fact(DisplayName = "It should replace an existing region.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceRegionPayload payload = CreateJohtoPayload();
    payload.Key = _seeded.Key;
    Guid id = _region.EntityId;

    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    RegionDto region = result.Region;
    Assert.NotNull(region);

    Assert.Equal(id, region.Id);
    Assert.Equal(3, region.Version);
    Assert.Equal(_seeded.CreatedBy, region.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, region.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertJohto(payload, region);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchRegionsPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<RegionDto> results = await _regionService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no region was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _regionService.ReadAsync(_region.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many regions were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    Region johto = RegionBuilder.Johto(Faker, Context.World);
    await _regionRepository.SaveAsync(johto);

    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await _regionService.ReadAsync(_region.EntityId, johto.Key.Value));
    TooManyResultsException<RegionDto> tooMany = Assert.IsType<TooManyResultsException<RegionDto>>(exception.InnerException);
    Assert.Equal(1, tooMany.ExpectedCount);
    Assert.Equal(2, tooMany.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the region was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _regionService.UpdateAsync(Guid.Empty, new UpdateRegionPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    Region johto = RegionBuilder.Johto(Faker, Context.World);
    Region hoenn = RegionBuilder.Hoenn(Faker, Context.World);
    Region sinnoh = RegionBuilder.Sinnoh(Faker, Context.World);
    await _regionRepository.SaveAsync([johto, hoenn, sinnoh]);

    SearchRegionsPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("johto");
    payload.Search.Terms.Add("hoenn");
    payload.Ids.AddRange([johto.EntityId, hoenn.EntityId]);
    payload.Sort.Add(new SortOption<RegionSort>(RegionSort.Name, SortDirection.Descending));

    SearchResults<RegionDto> results = await _regionService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    RegionDto region = Assert.Single(results.Items);
    Assert.Equal(hoenn.EntityId, region.Id);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating a region and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceRegionPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _regionService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Region.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_region.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Region.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing a region and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    Region johto = RegionBuilder.Johto(Faker, Context.World);
    await _regionRepository.SaveAsync(johto);

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = johto.EntityId;

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _regionService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Region.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_region.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Region.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating a region and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Region johto = RegionBuilder.Johto(Faker, Context.World);
    await _regionRepository.SaveAsync(johto);

    UpdateRegionPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = johto.EntityId;

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _regionService.UpdateAsync(id, payload));
    Assert.Equal(Region.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_region.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Region.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceRegionPayload payload = new()
    {
      Key = string.Empty
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _regionService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateRegionPayload payload = new()
    {
      Key = "not valid"
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _regionService.UpdateAsync(_region.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a region.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceRegionPayload payload = CreateJohtoPayload();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("CreateRegion", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing a region.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceRegionPayload payload = CreateJohtoPayload();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.CreateOrReplaceAsync(payload, _region.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_region.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating a region.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    UpdateRegionPayload payload = new();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _regionService.UpdateAsync(_region.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_region.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should update an existing region.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _region.EntityId;
    CreateOrReplaceRegionPayload create = CreateJohtoPayload();
    UpdateRegionPayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content)
    };

    RegionDto? region = await _regionService.UpdateAsync(id, payload);
    Assert.NotNull(region);

    Assert.Equal(id, region.Id);
    Assert.Equal(3, region.Version);
    Assert.Equal(_seeded.CreatedBy, region.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, region.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(create.Name?.Trim(), region.Name);
    Assert.Equal(create.Summary?.Trim(), region.Summary);
    Assert.Equal(create.Content?.Trim(), region.Content);
  }

  private static CreateOrReplaceRegionPayload CreateJohtoPayload() => new()
  {
    Key = "johto",
    Name = " Johto ",
    Summary = "  The second region.  ",
    Content = "   Home of Goldenrod City and the Bell Tower.   "
  };

  private static void AssertJohto(CreateOrReplaceRegionPayload payload, RegionDto region)
  {
    Assert.Equal(SlugHelper.Format(payload.Key), region.Key);
    Assert.Equal(payload.Name?.Trim(), region.Name);
    Assert.Equal(payload.Summary?.Trim(), region.Summary);
    Assert.Equal(payload.Content?.Trim(), region.Content);
  }
}
