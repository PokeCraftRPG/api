using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class WorldIntegrationTests : IntegrationTests
{
  private readonly IWorldRepository _worldRepository;
  private readonly IWorldService _worldService;

  public WorldIntegrationTests() : base()
  {
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    _worldService = ServiceProvider.GetRequiredService<IWorldService>();
  }

  [Theory(DisplayName = "It should create a new world.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "the-NEW-world",
      Name = " The New World ",
      Description = "  This is the new world.  "
    };

    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.World);

    WorldModel world = result.World;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, world.Id);
    }
    else
    {
      Assert.NotEqual(default, world.Id);
    }
    Assert.Equal(2, world.Version);
    Assert.Equal(Actor, world.CreatedBy);
    Assert.Equal(DateTime.UtcNow, world.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), world.Key);
    Assert.Equal(payload.Name.Trim(), world.Name);
    Assert.Equal(payload.Description.Trim(), world.Description);
    Assert.Equal(Actor, world.Owner);
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_SearchAsync_Then_Results()
  {
    World world1 = new(World.OwnerId, new Slug("new-world"));
    World world2 = new(World.OwnerId, new Slug("old-world"));
    World world3 = new(World.OwnerId, new Slug("pokemon"));
    await _worldRepository.SaveAsync([world1, world2, world3]);

    SearchWorldsPayload payload = new()
    {
      Ids = [world1.Id.ToGuid(), world2.Id.ToGuid(), world3.Id.ToGuid(), Guid.Empty],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Terms.Add(new SearchTerm("%-world"));
    payload.Sort.Add(new WorldSortOption(WorldSort.Key, isDescending: true));

    SearchResults<WorldModel> results = await _worldService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    WorldModel world = Assert.Single(results.Items);
    Assert.Equal(world1.Id.ToGuid(), world.Id);
  }

  [Fact(DisplayName = "It should read a world by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = World.Id.ToGuid();
    WorldModel? world = await _worldService.ReadAsync(id);
    Assert.NotNull(world);
    Assert.Equal(id, world.Id);
  }

  [Fact(DisplayName = "It should read a world by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    WorldModel? world = await _worldService.ReadAsync(id: null, $" {World.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(world);
    Assert.Equal(World.Id.ToGuid(), world.Id);
  }

  [Fact(DisplayName = "It should replace an existing world.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "the-NEW-world",
      Name = " The New World ",
      Description = "  This is the new world.  "
    };
    Guid id = World.Id.ToGuid();

    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.World);

    WorldModel world = result.World;
    Assert.Equal(id, world.Id);
    Assert.Equal(3, world.Version);
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Key.ToLowerInvariant(), world.Key);
    Assert.Equal(payload.Name.Trim(), world.Name);
    Assert.Equal(payload.Description.Trim(), world.Description);
    Assert.Equal(Actor, world.Owner);
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = World.Key.Value.ToUpperInvariant()
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _worldService.CreateOrReplaceAsync(payload, id));
    Assert.Null(exception.WorldId);
    Assert.Equal("World", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(World.Id.ToGuid(), exception.ConflictId);
    Assert.Equal(World.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing world.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = World.Id.ToGuid();
    UpdateWorldPayload payload = new()
    {
      Name = new Optional<string>(" The New World "),
      Description = new Optional<string>("  This is the new world.  ")
    };

    WorldModel? world = await _worldService.UpdateAsync(id, payload);
    Assert.NotNull(world);

    Assert.Equal(id, world.Id);
    Assert.Equal(2, world.Version);
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(World.Key.Value, world.Key);
    Assert.Equal(payload.Name.Value?.Trim(), world.Name);
    Assert.Equal(payload.Description.Value?.Trim(), world.Description);
    Assert.Equal(Actor, world.Owner);
  }
}
