using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Logitar;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Actors;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Moves;

[Trait(Traits.Category, Categories.Integration)]
public class MoveIntegrationTests : IntegrationTests
{
  private readonly IMoveRepository _moveRepository;
  private readonly IMoveService _moveService;

  private Move _move = null!;
  private Move _thunderbolt = null!;

  public MoveIntegrationTests()
  {
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _moveService = ServiceProvider.GetRequiredService<IMoveService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _move = MoveBuilder.Tackle(Faker, Context.World);
    _thunderbolt = MoveBuilder.Thunderbolt(Faker, Context.World);
    await _moveRepository.SaveAsync([_move, _thunderbolt]);
  }

  [Theory(DisplayName = "It should create a new move.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Fire,
      Category = MoveCategory.Special,
      Key = "Flamethrower",
      Name = " Flamethrower ",
      Description = "  The target is scorched with an intense blast of fire. This may also leave the target with a burn.  ",
      Accuracy = 100,
      Power = 90,
      PowerPoints = 15
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id);
    MoveModel move = result.Move;
    Assert.NotNull(move);
    Assert.True(result.Created);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, move.Id);
    }
    Assert.Equal(3, move.Version);
    Assert.Equal(Actor, move.CreatedBy);
    Assert.Equal(DateTime.UtcNow, move.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(Actor, move.UpdatedBy);
    Assert.True(move.CreatedOn < move.UpdatedOn);

    Assert.Equal(payload.Type, move.Type);
    Assert.Equal(payload.Category, move.Category);
    Assert.Equal(payload.Key.ToLowerInvariant(), move.Key);
    Assert.Equal(payload.Name.Trim(), move.Name);
    Assert.Equal(payload.Description.Trim(), move.Description);
    Assert.Equal(payload.Accuracy, move.Accuracy);
    Assert.Equal(payload.Power, move.Power);
    Assert.Equal(payload.PowerPoints, move.PowerPoints);
  }

  [Fact(DisplayName = "It should read a move by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    MoveModel? move = await _moveService.ReadAsync(_move.EntityId);
    Assert.NotNull(move);
    Assert.Equal(_move.EntityId, move.Id);
  }

  [Fact(DisplayName = "It should read a move by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    MoveModel? move = await _moveService.ReadAsync(id: null, _move.Key.Value);
    Assert.NotNull(move);
    Assert.Equal(_move.EntityId, move.Id);
  }

  [Fact(DisplayName = "It should replace an existing move.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Normal,
      Category = MoveCategory.Physical,
      Key = "Body-Slam",
      Name = " Body Slam ",
      Description = "  A reckless, full-body charge attack for slamming into the target. This may also leave the target with paralysis.  ",
      Accuracy = 100,
      Power = 85,
      PowerPoints = 15
    };
    Guid id = _move.EntityId;

    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id);
    MoveModel move = result.Move;
    Assert.NotNull(move);
    Assert.False(result.Created);

    Assert.Equal(id, move.Id);
    Assert.Equal(_move.Version + 4, move.Version);
    Assert.Equal(_move.CreatedBy, move.CreatedBy.ToActorId());
    Assert.Equal(_move.CreatedOn.AsUniversalTime(), move.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, move.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, move.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(payload.Type, move.Type);
    Assert.Equal(payload.Category, move.Category);
    Assert.Equal(payload.Key.ToLowerInvariant(), move.Key);
    Assert.Equal(payload.Name.Trim(), move.Name);
    Assert.Equal(payload.Description.Trim(), move.Description);
    Assert.Equal(payload.Accuracy, move.Accuracy);
    Assert.Equal(payload.Power, move.Power);
    Assert.Equal(payload.PowerPoints, move.PowerPoints);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NotFound_When_Search_Then_Empty()
  {
    SearchMovesPayload payload = new();
    payload.Ids.Add(Guid.Empty);

    SearchResults<MoveModel> results = await _moveService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the move was not read.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder().Build();
    Assert.Null(await _moveService.ReadAsync(_move.EntityId, _move.Key.Value));
  }

  [Fact(DisplayName = "It should return null when the move was not updated.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _moveService.UpdateAsync(Guid.Empty, new UpdateMovePayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Found_When_Search_Then_Results()
  {
    SearchMovesPayload payload = new();

    SearchResults<MoveModel> results = await _moveService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    Assert.Equal(results.Total, results.Items.Count);
    Assert.Contains(results.Items, move => move.Id == _move.EntityId);
    Assert.Contains(results.Items, move => move.Id == _thunderbolt.EntityId);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when creating or replacing a move.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    Guid id = Guid.NewGuid();
    if (exists)
    {
      id = _thunderbolt.EntityId;
    }

    CreateOrReplaceMovePayload payload = new()
    {
      Type = exists ? PokemonType.Electric : PokemonType.Normal,
      Category = exists ? MoveCategory.Special : MoveCategory.Physical,
      Key = _move.Key.Value,
      Accuracy = exists ? (byte?)100 : 100,
      Power = exists ? 90 : (byte?)40,
      PowerPoints = exists ? (byte)15 : (byte)35
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _moveService.CreateOrReplaceAsync(payload, id));
    Assert.NotNull(Context.World);
    Assert.Equal(Context.World.EntityId, exception.WorldId);
    Assert.Equal(Move.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_move.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating an existing move.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Guid id = _thunderbolt.EntityId;
    UpdateMovePayload payload = new()
    {
      Key = _move.Key.Value
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _moveService.UpdateAsync(id, payload));
    Assert.NotNull(Context.World);
    Assert.Equal(Context.World.EntityId, exception.WorldId);
    Assert.Equal(Move.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_move.EntityId, exception.ConflictingId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new move.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Normal,
      Category = MoveCategory.Physical,
      Key = "denied-move",
      PowerPoints = 35
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _moveService.CreateOrReplaceAsync(payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.CreateMove, exception.Action);
    Assert.Null(exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing move.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Normal,
      Category = MoveCategory.Physical,
      Key = "denied-move",
      PowerPoints = 35
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _moveService.CreateOrReplaceAsync(payload, _move.EntityId));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_move.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing move.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder().Build();

    UpdateMovePayload payload = new();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _moveService.UpdateAsync(_move.EntityId, payload));
    Assert.Equal(Actor.ToActorId().Value, exception.Principal);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(_move.GetEntity().ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many moves were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    var exception = await Assert.ThrowsAsync<TooManyResultsException<MoveModel>>(async () => await _moveService.ReadAsync(_move.EntityId, _thunderbolt.Key.Value));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing move.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _move.EntityId;
    UpdateMovePayload payload = new()
    {
      Name = new Optional<string>(" Headbutt "),
      Description = new Optional<string>("  The user sticks out its head and attacks by charging straight into the target.  ")
    };

    MoveModel? move = await _moveService.UpdateAsync(id, payload);
    Assert.NotNull(move);

    Assert.Equal(id, move.Id);
    Assert.Equal(_move.Version + 2, move.Version);
    Assert.Equal(_move.CreatedBy, move.CreatedBy.ToActorId());
    Assert.Equal(_move.CreatedOn.AsUniversalTime(), move.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, move.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, move.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_move.Type, move.Type);
    Assert.Equal(_move.Category, move.Category);
    Assert.Equal(_move.Key.Value, move.Key);
    Assert.Equal(payload.Name.Value?.Trim(), move.Name);
    Assert.Equal(payload.Description.Value?.Trim(), move.Description);
    Assert.Equal(_move.Accuracy?.Value, move.Accuracy);
    Assert.Equal(_move.Power?.Value, move.Power);
    Assert.Equal(_move.PowerPoints.Value, move.PowerPoints);
  }
}
