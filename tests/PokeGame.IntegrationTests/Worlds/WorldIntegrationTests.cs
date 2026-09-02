using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Worlds;

[Trait(Traits.Category, Categories.Integration)]
public class WorldIntegrationTests : IntegrationTests
{
  private readonly IWorldRepository _worldRepository;
  private readonly IWorldService _worldService;

  private World _world = null!;
  private WorldDto _seeded = null!;

  public WorldIntegrationTests()
  {
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    _worldService = ServiceProvider.GetRequiredService<IWorldService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _world = new WorldBuilder(Faker)
      .WithOwner(Context.User)
      .WithKey("the-old-world")
      .WithName("The Old World")
      .Build();
    await _worldRepository.SaveAsync(_world);

    _seeded = (await _worldService.ReadAsync(_world.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new world.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceWorldPayload payload = CreateNewWorldPayload();
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    WorldDto world = result.World;
    Assert.NotNull(world);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, world.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, world.Id);
    }
    Assert.Equal(2, world.Version);
    Assert.Equal(Actor, world.CreatedBy);
    Assert.Equal(DateTime.UtcNow, world.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(world.CreatedBy, world.UpdatedBy);
    Assert.True(world.CreatedOn < world.UpdatedOn);

    AssertNewWorld(payload, world);
  }

  [Fact(DisplayName = "It should read a world by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    WorldDto? world = await _worldService.ReadAsync(_world.EntityId);
    Assert.NotNull(world);
    Assert.Equal(_world.EntityId, world.Id);
  }

  [Fact(DisplayName = "It should read a world by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    WorldDto? world = await _worldService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(world);
    Assert.Equal(_world.EntityId, world.Id);
  }

  [Fact(DisplayName = "It should replace an existing world.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceWorldPayload payload = CreateNewWorldPayload();
    payload.Key = _seeded.Key;
    Guid id = _world.EntityId;

    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    WorldDto world = result.World;
    Assert.NotNull(world);

    Assert.Equal(id, world.Id);
    Assert.Equal(3, world.Version);
    Assert.Equal(_seeded.CreatedBy, world.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, world.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertNewWorld(payload, world);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.User = new UserBuilder(Faker).Build();

    SearchWorldsPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<WorldDto> results = await _worldService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no world was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.User = new UserBuilder(Faker).Build();

    Assert.Null(await _worldService.ReadAsync(_world.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many worlds were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await _worldService.ReadAsync(Context.World!.EntityId, _world.Key.Value));
    TooManyResultsException<WorldDto> tooMany = Assert.IsType<TooManyResultsException<WorldDto>>(exception.InnerException);
    Assert.Equal(1, tooMany.ExpectedCount);
    Assert.Equal(2, tooMany.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the world was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _worldService.UpdateAsync(Guid.Empty, new UpdateWorldPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    World newWorld = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("the-new-world").Build();
    World anotherWorld = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("another-world").Build();
    await _worldRepository.SaveAsync([newWorld, anotherWorld]);

    SearchWorldsPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("new-world");
    payload.Search.Terms.Add("old-world");
    payload.Ids.AddRange([Context.World!.EntityId, _world.EntityId, newWorld.EntityId]);
    payload.Sort.Add(new SortOption<WorldSort>(WorldSort.Key, SortDirection.Descending));

    SearchResults<WorldDto> results = await _worldService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    WorldDto world = Assert.Single(results.Items);
    Assert.Equal(newWorld.EntityId, world.Id);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating a world and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = Guid.NewGuid();

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _worldService.CreateOrReplaceAsync(payload, id));
    Assert.Null(exception.WorldId);
    Assert.Equal(World.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_world.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(World.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing a world and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    World newWorld = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("the-new-world").Build();
    await _worldRepository.SaveAsync(newWorld);

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = newWorld.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _worldService.CreateOrReplaceAsync(payload, id));
    Assert.Null(exception.WorldId);
    Assert.Equal(World.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_world.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(World.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating a world and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    World newWorld = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("the-new-world").Build();
    await _worldRepository.SaveAsync(newWorld);

    UpdateWorldPayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = newWorld.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _worldService.UpdateAsync(id, payload));
    Assert.Null(exception.WorldId);
    Assert.Equal(World.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_world.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(World.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = string.Empty
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _worldService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateWorldPayload payload = new()
    {
      Key = "not valid"
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _worldService.UpdateAsync(_world.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a world.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    World world2 = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("world-2").Build();
    World world3 = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("world-3").Build();
    await _worldRepository.SaveAsync([world2, world3]);

    CreateOrReplaceWorldPayload payload = CreateNewWorldPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _worldService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("CreateWorld", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing a world.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceWorldPayload payload = CreateNewWorldPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _worldService.CreateOrReplaceAsync(payload, _world.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_world.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating a world.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    UpdateWorldPayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _worldService.UpdateAsync(_world.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_world.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should update an existing world.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _world.EntityId;
    CreateOrReplaceWorldPayload create = CreateNewWorldPayload();
    UpdateWorldPayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content)
    };

    WorldDto? world = await _worldService.UpdateAsync(id, payload);
    Assert.NotNull(world);

    Assert.Equal(id, world.Id);
    Assert.Equal(3, world.Version);
    Assert.Equal(_seeded.CreatedBy, world.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, world.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(create.Name?.Trim(), world.Name);
    Assert.Equal(create.Summary?.Trim(), world.Summary);
    Assert.Equal(create.Content?.Trim(), world.Content);
  }

  private static CreateOrReplaceWorldPayload CreateNewWorldPayload() => new()
  {
    Key = "the-new-world",
    Name = " The New World ",
    Summary = "  A brand new world.  ",
    Content = "  This is the new world.  "
  };

  private static void AssertNewWorld(CreateOrReplaceWorldPayload payload, WorldDto world)
  {
    Assert.Equal(SlugHelper.Format(payload.Key), world.Key);
    Assert.Equal(payload.Name?.Trim(), world.Name);
    Assert.Equal(payload.Summary?.Trim(), world.Summary);
    Assert.Equal(payload.Content?.Trim(), world.Content);
  }
}
