using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Regions;
using PokeGame.Core.Regions.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class RegionIntegrationTests : IntegrationTests
{
  private readonly IRegionRepository _regionRepository;
  private readonly IRegionService _regionService;

  private Region _region = null!;

  public RegionIntegrationTests() : base()
  {
    _regionRepository = ServiceProvider.GetRequiredService<IRegionRepository>();
    _regionService = ServiceProvider.GetRequiredService<IRegionService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _region = new RegionBuilder(Faker).WithWorld(World).Build();
    await _regionRepository.SaveAsync(_region);
  }

  [Theory(DisplayName = "It should create a new region.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "kanto",
      Name = " Kanto ",
      Description = "  Kanto is the classic region where many Trainer journeys begin: Professor Oak, Pallet Town, 8 Gyms, Team Rocket, and the Indigo League.  ",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Kanto_(Region)",
      Notes = "   Classic, temperate region east of Johto: 10 settlements, 8 Gyms, Team Rocket, Oak in Pallet, and the Indigo League at Indigo Plateau.   "
    };

    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Region);

    RegionModel region = result.Region;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, region.Id);
    }
    else
    {
      Assert.NotEqual(default, region.Id);
    }
    Assert.Equal(2, region.Version);
    Assert.Equal(Actor, region.CreatedBy);
    Assert.Equal(DateTime.UtcNow, region.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), region.Key);
    Assert.Equal(payload.Name.Trim(), region.Name);
    Assert.Equal(payload.Description.Trim(), region.Description);
    Assert.Equal(payload.Url, region.Url);
    Assert.Equal(payload.Notes.Trim(), region.Notes);
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_SearchAsync_Then_Results()
  {
    Region hoenn = RegionBuilder.Hoenn(Faker, World);
    Region johto = RegionBuilder.Johto(Faker, World);
    Region kanto = RegionBuilder.Kanto(Faker, World);
    Region sinnoh = RegionBuilder.Sinnoh(Faker, World);
    await _regionRepository.SaveAsync([hoenn, johto, kanto, sinnoh]);

    SearchRegionsPayload payload = new()
    {
      Ids = [hoenn.EntityId, johto.EntityId, Guid.Empty, sinnoh.EntityId],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Operator = SearchOperator.Or;
    payload.Search.Terms.Add(new SearchTerm("%nn%"));
    payload.Search.Terms.Add(new SearchTerm("k%"));
    payload.Sort.Add(new RegionSortOption(RegionSort.Key, isDescending: true));

    SearchResults<RegionModel> results = await _regionService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    RegionModel region = Assert.Single(results.Items);
    Assert.Equal(hoenn.EntityId, region.Id);
  }

  [Fact(DisplayName = "It should read a region by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _region.EntityId;
    RegionModel? region = await _regionService.ReadAsync(id);
    Assert.NotNull(region);
    Assert.Equal(id, region.Id);
  }

  [Fact(DisplayName = "It should read a region by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    RegionModel? region = await _regionService.ReadAsync(id: null, $" {_region.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(region);
    Assert.Equal(_region.EntityId, region.Id);
  }

  [Fact(DisplayName = "It should replace an existing region.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceRegionPayload payload = new()
    {
      Key = "kanto",
      Name = " Kanto ",
      Description = "  Kanto is the classic region where many Trainer journeys begin: Professor Oak, Pallet Town, 8 Gyms, Team Rocket, and the Indigo League.  ",
      Url = "https://bulbapedia.bulbagarden.net/wiki/Kanto_(Region)",
      Notes = "   Classic, temperate region east of Johto: 10 settlements, 8 Gyms, Team Rocket, Oak in Pallet, and the Indigo League at Indigo Plateau.   "
    };
    Guid id = _region.EntityId;

    CreateOrReplaceRegionResult result = await _regionService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.Region);

    RegionModel region = result.Region;
    Assert.Equal(id, region.Id);
    Assert.Equal(3, region.Version);
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), region.Key);
    Assert.Equal(payload.Name.Trim(), region.Name);
    Assert.Equal(payload.Description.Trim(), region.Description);
    Assert.Equal(payload.Url, region.Url);
    Assert.Equal(payload.Notes.Trim(), region.Notes);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceRegionPayload payload = new()
    {
      Key = _region.Key.Value.ToUpperInvariant()
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _regionService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Region", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_region.EntityId, exception.ConflictId);
    Assert.Equal(_region.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing region.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _region.EntityId;
    UpdateRegionPayload payload = new()
    {
      Name = new Optional<string>(" Kanto "),
      Description = new Optional<string>("  Kanto is the classic region where many Trainer journeys begin: Professor Oak, Pallet Town, 8 Gyms, Team Rocket, and the Indigo League.  "),
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Kanto_(Region)"),
      Notes = new Optional<string>("   Classic, temperate region east of Johto: 10 settlements, 8 Gyms, Team Rocket, Oak in Pallet, and the Indigo League at Indigo Plateau.   ")
    };

    RegionModel? region = await _regionService.UpdateAsync(id, payload);
    Assert.NotNull(region);

    Assert.Equal(id, region.Id);
    Assert.Equal(2, region.Version);
    Assert.Equal(Actor, region.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, region.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_region.Key.Value, region.Key);
    Assert.Equal(payload.Name.Value?.Trim(), region.Name);
    Assert.Equal(payload.Description.Value?.Trim(), region.Description);
    Assert.Equal(payload.Url.Value, region.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), region.Notes);
  }
}
