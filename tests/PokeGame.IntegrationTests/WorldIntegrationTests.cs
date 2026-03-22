using Microsoft.Extensions.DependencyInjection;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame;

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

    _world = new World(new UserId(Actor.GetActorId()), new Slug("the-old-world"));
    await _worldRepository.SaveAsync(_world);
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
  }

  [Fact(DisplayName = "It should read a world by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _world.Id.ToGuid();
    WorldModel? world = await _worldService.ReadAsync(id);
    Assert.NotNull(world);
    Assert.Equal(id, world.Id);
  }

  [Fact(DisplayName = "It should read a world by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    WorldModel? world = await _worldService.ReadAsync(id: null, $" {_world.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(world);
    Assert.Equal(_world.Id.ToGuid(), world.Id);
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
    Guid id = _world.Id.ToGuid();

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
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceWorldPayload payload = new()
    {
      Key = _world.Key.Value.ToUpperInvariant()
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _worldService.CreateOrReplaceAsync(payload, id));
    Assert.Null(exception.WorldId);
    Assert.Equal("World", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_world.Id.ToGuid(), exception.ConflictId);
    Assert.Equal(_world.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing world.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _world.Id.ToGuid();
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

    Assert.Equal(_world.Key.Value, world.Key);
    Assert.Equal(payload.Name.Value?.Trim(), world.Name);
    Assert.Equal(payload.Description.Value?.Trim(), world.Description);
  }
}
