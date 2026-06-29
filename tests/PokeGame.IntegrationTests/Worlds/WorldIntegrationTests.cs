using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
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

    _world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("the-old-world").WithName("The Old World").Build();
    _worldRepository.Add(_world);
    await Context.SaveChangesAsync();
  }

  [Theory(DisplayName = "It should create a new world.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "The-New-World",
      Name = " The New World ",
      Description = "  This is the new world!  "
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    WorldModel world = result.World;
    Assert.NotNull(world);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, world.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, world.Id);
    }
    Assert.Equal(1, world.Version);
    Assert.Equal(Actor, world.CreatedBy);
    Assert.Equal(DateTime.UtcNow, world.CreatedOn, TimeSpan.FromSeconds(1));
    Assert.Equal(world.CreatedBy, world.UpdatedBy);
    Assert.Equal(world.CreatedOn, world.UpdatedOn);

    Assert.Equal(Actor, world.Owner);
    Assert.Equal(SlugHelper.Format(payload.Key), world.Key);
    Assert.Equal(payload.Name.Trim(), world.Name);
    Assert.Equal(payload.Description.Trim(), world.Description);
  }

  [Fact(DisplayName = "It should read a world by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    WorldModel? world = await _worldService.ReadAsync(_world.Id);
    Assert.NotNull(world);
    Assert.Equal(_world.Id, world.Id);
  }

  [Fact(DisplayName = "It should read a world by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    WorldModel? world = await _worldService.ReadAsync(id: null, $" {_world.Key.ToUpperInvariant()} ");
    Assert.NotNull(world);
    Assert.Equal(_world.Id, world.Id);
  }

  [Fact(DisplayName = "It should replace an existing world.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "The-New-World",
      Name = " The New World ",
      Description = "  This is the new world!  "
    };
    Guid id = _world.Id;

    CreateOrReplaceWorldResult result = await _worldService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    WorldModel world = result.World;
    Assert.NotNull(world);

    Assert.Equal(id, world.Id);
    Assert.Equal(_world.Version, world.Version);
    Assert.Equal(_world.CreatedBy, world.CreatedBy.Id);
    Assert.Equal(_world.CreatedOn, world.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(_world.OwnerId, world.Owner.Id);
    Assert.Equal(SlugHelper.Format(payload.Key), world.Key);
    Assert.Equal(payload.Name.Trim(), world.Name);
    Assert.Equal(payload.Description.Trim(), world.Description);
  }

  [Fact(DisplayName = "It should return empty search results when no world is matching.")]
  public async Task Given_NoneMatching_When_Search_Then_EmptyResults()
  {
    Context.User = new UserBuilder().Build();

    SearchResults<WorldModel> results = await _worldService.SearchAsync(new SearchWorldsPayload());
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the user does not own the world.")]
  public async Task Given_NotOwner_When_Read_Then_NullReturned()
  {
    Context.User = new UserBuilder().Build();

    Assert.Null(await _worldService.ReadAsync(Context.WorldId, $" {_world.Key.ToUpperInvariant()} "));
  }

  [Fact(DisplayName = "It should return null when the world does not exist.")]
  public async Task Given_NotExist_When_Update_Then_NullReturned()
  {
    Assert.Null(await _worldService.UpdateAsync(Guid.Empty, new UpdateWorldPayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_CorrectResults()
  {
    SearchWorldsPayload payload = new();
    payload.Ids.AddRange([Context.WorldId, _world.Id]);
    payload.Search.Terms.Add(new SearchTerm("%world"));
    payload.Sort.Add(new WorldSortOption(WorldSort.UpdatedOn, isDescending: true));

    SearchResults<WorldModel> results = await _worldService.SearchAsync(payload);
    Assert.Equal(2, results.Total);
    Assert.Equal(results.Total, results.Items.Count);
    Assert.Equal(_world.Id, results.Items.ElementAt(0).Id);
    Assert.Equal(Context.WorldId, results.Items.ElementAt(1).Id);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (CreateOrReplace).")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = _world.Key
    };
    Guid? id = exists ? Context.WorldId : null;

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _worldService.CreateOrReplaceAsync(payload, id));
    Assert.Null(exception.WorldId);
    Assert.Equal(World.ResourceKind, exception.ResourceKind);
    if (id.HasValue)
    {
      Assert.Equal(id.Value, exception.ResourceId);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, exception.ResourceId);
    }
    Assert.Equal(_world.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (Update).")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Guid id = Context.WorldId;
    UpdateWorldPayload payload = new()
    {
      Key = _world.Key
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _worldService.UpdateAsync(id, payload));
    Assert.Null(exception.WorldId);
    Assert.Equal(World.ResourceKind, exception.ResourceKind);
    Assert.Equal(id, exception.ResourceId);
    Assert.Equal(_world.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new world.")]
  public async Task Given_NotExist_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithOwner(Context.User).WithKey("the-new-world").WithName("The New World").Build();
    _worldRepository.Add(world);
    await Context.SaveChangesAsync();

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "the-new-world"
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _worldService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.CreateWorld, exception.Action);
    Assert.Null(exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing world.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceWorldPayload payload = new()
    {
      Key = "the-new-world"
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _worldService.CreateOrReplaceAsync(payload, _world.Id));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_world.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing world.")]
  public async Task Given_Exists_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _worldService.UpdateAsync(_world.Id, new UpdateWorldPayload()));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_world.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many world were found.")]
  public async Task Given_ManyWorldFound_When_Read_Then_TooManyResultsException()
  {
    var exception = await Assert.ThrowsAsync<TooManyResultsException<WorldModel>>(async () => await _worldService.ReadAsync(Context.WorldId, $" {_world.Key.ToUpperInvariant()} "));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing world.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _world.Id;
    UpdateWorldPayload payload = new()
    {
      Key = "The-New-World",
      Name = new Optional<string>(" The New World "),
      Description = new Optional<string>("  This is the new world!  ")
    };

    WorldModel? world = await _worldService.UpdateAsync(id, payload);
    Assert.NotNull(world);

    Assert.Equal(id, world.Id);
    Assert.Equal(_world.Version, world.Version);
    Assert.Equal(_world.CreatedBy, world.CreatedBy.Id);
    Assert.Equal(_world.CreatedOn, world.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, world.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, world.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(_world.OwnerId, world.Owner.Id);
    Assert.Equal(SlugHelper.Format(payload.Key), world.Key);
    Assert.Equal(payload.Name.Value?.Trim(), world.Name);
    Assert.Equal(payload.Description.Value?.Trim(), world.Description);
  }
}
