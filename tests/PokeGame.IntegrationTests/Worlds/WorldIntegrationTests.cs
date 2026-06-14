using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Logitar;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Worlds;

[Trait(Traits.Category, Categories.Integration)]
public class WorldIntegrationTests : IntegrationTests
{
  private readonly IWorldRepository _worldRepository;
  private readonly IWorldService _worldService;

  private World _world = null!;

  public WorldIntegrationTests() : base()
  {
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
    _worldService = ServiceProvider.GetRequiredService<IWorldService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey(new Slug("old-world")).Build();
    await _worldRepository.SaveAsync(_world);
  }

  [Theory(DisplayName = "It should create a new world.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "New-World",
      Name = " The New World ",
      Description = "  This is the new world!  "
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id);
    WorldModel world = result.World;
    Assert.NotNull(world);
    Assert.True(result.Created);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, world.Id);
    }
    Assert.Equal(3, world.Version);
    Assert.Equal(Actor, world.CreatedBy);
    Assert.Equal(DateTime.UtcNow, world.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.True(world.CreatedOn < world.UpdatedOn);

    Assert.Equal(Actor, world.Owner);
    Assert.Equal(payload.Key.ToLowerInvariant(), world.Key);
    Assert.Equal(payload.Name.Trim(), world.Name);
    Assert.Equal(payload.Description.Trim(), world.Description);
  }

  [Fact(DisplayName = "It should read a world by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    WorldModel? world = await _worldService.ReadAsync(_world.EntityId);
    Assert.NotNull(world);
    Assert.Equal(_world.EntityId, world.Id);
  }

  [Fact(DisplayName = "It should read a world by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    WorldModel? world = await _worldService.ReadAsync(id: null, _world.Key.Value);
    Assert.NotNull(world);
    Assert.Equal(_world.EntityId, world.Id);
  }

  [Fact(DisplayName = "It should replace an existing world.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "New-World",
      Name = " The New World ",
      Description = "  This is the new world!  "
    };
    Guid id = _world.EntityId;

    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id);
    WorldModel world = result.World;
    Assert.NotNull(world);
    Assert.False(result.Created);

    Assert.Equal(id, world.Id);
    Assert.Equal(_world.Version + 3, world.Version);
    Assert.Equal(_world.CreatedBy, world.CreatedBy.ToActorId());
    Assert.Equal(_world.CreatedOn.AsUniversalTime(), world.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(Actor, world.Owner);
    Assert.Equal(payload.Key.ToLowerInvariant(), world.Key);
    Assert.Equal(payload.Name.Trim(), world.Name);
    Assert.Equal(payload.Description.Trim(), world.Description);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NotFound_When_Search_Then_Empty()
  {
    SearchWorldsPayload payload = new();
    payload.Ids.Add(Guid.Empty);

    SearchResults<WorldModel> results = await _worldService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the world was not read.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    World world = new WorldBuilder(Faker).WithKey(new Slug("another-world")).Build();
    await _worldRepository.SaveAsync(world);

    Assert.Null(await _worldService.ReadAsync(world.EntityId, world.Key.Value));
  }

  [Fact(DisplayName = "It should return null when the world was not updated.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _worldService.UpdateAsync(Guid.Empty, new UpdateWorldPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Found_When_Search_Then_Results()
  {
    SearchWorldsPayload payload = new();

    SearchResults<WorldModel> results = await _worldService.SearchAsync(payload);
    Assert.Equal(Context.World is null ? 1 : 2, results.Total);

    Assert.Equal(results.Total, results.Items.Count);
    Assert.Contains(results.Items, world => world.Id == _world.EntityId);
    if (Context.World is not null)
    {
      Assert.Contains(results.Items, world => world.Id == Context.World.EntityId);
    }
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when creating or replacing a world.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    Guid id = Guid.NewGuid();
    if (exists)
    {
      Assert.NotNull(Context.World);
      id = Context.World.EntityId;
    }

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = _world.Key.Value
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _worldService.CreateOrReplaceAsync(payload, id));
    Assert.Null(exception.WorldId);
    Assert.Equal(World.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_world.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating an existing world.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Assert.NotNull(Context.World);

    Guid id = Context.World.EntityId;
    UpdateWorldPayload payload = new()
    {
      Key = _world.Key.Value
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _worldService.UpdateAsync(id, payload));
    Assert.Null(exception.WorldId);
    Assert.Equal(World.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_world.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new world.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Assert.NotNull(Context.World);

    World world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey(new Slug("new-world")).Build();
    await _worldRepository.SaveAsync(world);

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "denied-world"
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _worldService.CreateOrReplaceAsync(payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.CreateWorld, exception.Action);
    Assert.Null(exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing world.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "denied-world"
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _worldService.CreateOrReplaceAsync(payload, _world.EntityId));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_world.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing world.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    UpdateWorldPayload payload = new();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _worldService.UpdateAsync(_world.EntityId, payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_world.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many worlds were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    Assert.NotNull(Context.World);

    var exception = await Assert.ThrowsAsync<TooManyResultsException<WorldModel>>(async () => await _worldService.ReadAsync(_world.EntityId, Context.World.Key.Value));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing world.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _world.EntityId;
    UpdateWorldPayload payload = new()
    {
      Name = new Optional<string>(" The New World "),
      Description = new Optional<string>("  This is the new world!  ")
    };

    WorldModel? world = await _worldService.UpdateAsync(id, payload);
    Assert.NotNull(world);

    Assert.Equal(id, world.Id);
    Assert.Equal(_world.Version + 2, world.Version);
    Assert.Equal(_world.CreatedBy, world.CreatedBy.ToActorId());
    Assert.Equal(_world.CreatedOn.AsUniversalTime(), world.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(Actor, world.Owner);
    Assert.Equal(_world.Key.Value, world.Key);
    Assert.Equal(payload.Name.Value?.Trim(), world.Name);
    Assert.Equal(payload.Description.Value?.Trim(), world.Description);
  }
}
