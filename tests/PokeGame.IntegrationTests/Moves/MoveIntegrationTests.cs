using FluentValidation;
using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Search;
using PokeGame.Core.Seo;

namespace PokeGame.Moves;

[Trait(Traits.Category, Categories.Integration)]
public class MoveIntegrationTests : IntegrationTests
{
  private readonly IMoveRepository _moveRepository;
  private readonly IMoveService _moveService;

  private Move _move = null!;
  private MoveDto _seeded = null!;

  public MoveIntegrationTests()
  {
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _moveService = ServiceProvider.GetRequiredService<IMoveService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _move = MoveBuilder.Tackle(Faker, Context.World);
    await _moveRepository.SaveAsync(_move);

    _seeded = (await _moveService.ReadAsync(_move.EntityId))!;
  }

  [Theory(DisplayName = "It should create a new move.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceMovePayload payload = CreateEmberPayload();
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    MoveDto move = result.Move;
    Assert.NotNull(move);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, move.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, move.Id);
    }
    Assert.Equal(3, move.Version);
    Assert.Equal(Actor, move.CreatedBy);
    Assert.Equal(DateTime.UtcNow, move.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(move.CreatedBy, move.UpdatedBy);
    Assert.True(move.CreatedOn < move.UpdatedOn);

    AssertEmber(payload, move);
  }

  [Fact(DisplayName = "It should read a move by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    MoveDto? move = await _moveService.ReadAsync(_move.EntityId);
    Assert.NotNull(move);
    Assert.Equal(_move.EntityId, move.Id);
  }

  [Fact(DisplayName = "It should read a move by key.")]
  public async Task Given_Key_When_Read_Then_Read()
  {
    MoveDto? move = await _moveService.ReadAsync(key: _seeded.Key);
    Assert.NotNull(move);
    Assert.Equal(_move.EntityId, move.Id);
  }

  [Fact(DisplayName = "It should replace an existing move.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceMovePayload payload = CreateUpdatedTacklePayload();
    payload.Key = _seeded.Key;
    Guid id = _move.EntityId;

    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    MoveDto move = result.Move;
    Assert.NotNull(move);

    Assert.Equal(id, move.Id);
    Assert.Equal(5, move.Version);
    Assert.Equal(_seeded.CreatedBy, move.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, move.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, move.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, move.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertUpdatedTackle(payload, move);
  }

  [Fact(DisplayName = "It should return empty search results.")]
  public async Task Given_NoMatch_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder(Faker).Build();

    SearchMovesPayload payload = new()
    {
      Limit = 10
    };

    SearchResults<MoveDto> results = await _moveService.SearchAsync(payload);
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when no move was found.")]
  public async Task Given_NotFound_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder(Faker).Build();

    Assert.Null(await _moveService.ReadAsync(_move.EntityId));
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many moves were read.")]
  public async Task Given_ManyFound_When_Read_Then_TooManyResultsException()
  {
    Move ember = MoveBuilder.Ember(Faker, Context.World);
    await _moveRepository.SaveAsync(ember);

    InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
      async () => await _moveService.ReadAsync(_move.EntityId, ember.Key.Value));
    TooManyResultsException<MoveDto> tooMany = Assert.IsType<TooManyResultsException<MoveDto>>(exception.InnerException);
    Assert.Equal(1, tooMany.ExpectedCount);
    Assert.Equal(2, tooMany.ActualCount);
  }

  [Fact(DisplayName = "It should return null when the move was not found.")]
  public async Task Given_NotFound_When_Update_Then_NullReturned()
  {
    Assert.Null(await _moveService.UpdateAsync(Guid.Empty, new UpdateMovePayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_Results()
  {
    Move ember = MoveBuilder.Ember(Faker, Context.World);
    Move waterGun = MoveBuilder.WaterGun(Faker, Context.World);
    Move thunderShock = MoveBuilder.ThunderShock(Faker, Context.World);
    await _moveRepository.SaveAsync([ember, waterGun, thunderShock]);

    SearchMovesPayload payload = new()
    {
      Offset = 1,
      Limit = 1
    };
    payload.Search.Mode = SearchMode.Any;
    payload.Search.Terms.Add("ember");
    payload.Search.Terms.Add("water");
    payload.Ids.AddRange([ember.EntityId, waterGun.EntityId]);
    payload.Sort.Add(new SortOption<MoveSort>(MoveSort.Name, SortDirection.Descending));

    SearchResults<MoveDto> results = await _moveService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    MoveDto move = Assert.Single(results.Items);
    Assert.Equal(ember.EntityId, move.Id);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when creating a move and the key conflicts.")]
  public async Task Given_KeyConflict_When_Create_Then_KeyAlreadyUsedException()
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Fire,
      Category = MoveCategory.Special,
      Key = _seeded.Key
    };
    Guid id = Guid.NewGuid();

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _moveService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Move.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_move.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Move.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when replacing a move and the key conflicts.")]
  public async Task Given_KeyConflict_When_Replace_Then_KeyAlreadyUsedException()
  {
    Move ember = MoveBuilder.Ember(Faker, Context.World);
    await _moveRepository.SaveAsync(ember);

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Fire,
      Category = MoveCategory.Special,
      Key = _seeded.Key
    };
    Guid id = ember.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _moveService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Move.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_move.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Move.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when updating a move and the key conflicts.")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Move ember = MoveBuilder.Ember(Faker, Context.World);
    await _moveRepository.SaveAsync(ember);

    UpdateMovePayload payload = new()
    {
      Key = _seeded.Key
    };
    Guid id = ember.EntityId;

    KeyAlreadyUsedException exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(
      async () => await _moveService.UpdateAsync(id, payload));
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
    Assert.Equal(Move.EntityKind, exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_move.EntityId, exception.ConflictId);
    Assert.Equal(SlugHelper.Format(payload.Key), exception.AttemptedKey);
    Assert.Equal(nameof(Move.Key), exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw ValidationException when the create/replace payload is invalid.")]
  public async Task Given_InvalidPayload_When_Create_Then_ValidationException()
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Fire,
      Category = MoveCategory.Special,
      Key = string.Empty
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _moveService.CreateOrReplaceAsync(payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when the update payload is invalid.")]
  public async Task Given_InvalidPayload_When_Update_Then_ValidationException()
  {
    UpdateMovePayload payload = new()
    {
      Key = "not valid"
    };

    await Assert.ThrowsAsync<ValidationException>(async () => await _moveService.UpdateAsync(_move.EntityId, payload));
  }

  [Fact(DisplayName = "It should throw ValidationException when creating a status move with power.")]
  public async Task Given_StatusMoveWithPower_When_Create_Then_InvalidMovePower()
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Normal,
      Category = MoveCategory.Status,
      Key = "growl",
      Power = 40
    };

    ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
      async () => await _moveService.CreateOrReplaceAsync(payload));
    Assert.Contains(exception.Errors, error => error.PropertyName == nameof(Move.Power) && error.ErrorCode == "InvalidMovePower");
  }

  [Fact(DisplayName = "It should throw ValidationException when updating a status move with power.")]
  public async Task Given_StatusMoveWithPower_When_Update_Then_InvalidMovePower()
  {
    Move growl = new MoveBuilder(Faker)
      .WithWorld(Context.World)
      .WithType(PokemonType.Normal)
      .WithCategory(MoveCategory.Status)
      .WithKey("growl")
      .WithName("Growl")
      .WithPower(null)
      .WithPowerPoints(40)
      .Build();
    await _moveRepository.SaveAsync(growl);

    UpdateMovePayload payload = new()
    {
      Power = new Optional<int?>(40)
    };

    ValidationException exception = await Assert.ThrowsAsync<ValidationException>(
      async () => await _moveService.UpdateAsync(growl.EntityId, payload));
    Assert.Contains(exception.Errors, error => error.PropertyName == nameof(Move.Power) && error.ErrorCode == "InvalidMovePower");
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a move.")]
  public async Task Given_NotAllowed_When_Create_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceMovePayload payload = CreateEmberPayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _moveService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("CreateMove", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing a move.")]
  public async Task Given_NotAllowed_When_Replace_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    CreateOrReplaceMovePayload payload = CreateUpdatedTacklePayload();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _moveService.CreateOrReplaceAsync(payload, _move.EntityId));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_move.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating a move.")]
  public async Task Given_NotAllowed_When_Update_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    UpdateMovePayload payload = new();

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(
      async () => await _moveService.UpdateAsync(_move.EntityId, payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Update", exception.Action);
    Assert.Equal(_move.GetEntity().ToString(), exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should update an existing move.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _move.EntityId;
    CreateOrReplaceMovePayload create = CreateUpdatedTacklePayload();
    UpdateMovePayload payload = new()
    {
      Name = new Optional<string>(create.Name),
      Summary = new Optional<string>(create.Summary),
      Content = new Optional<string>(create.Content),
      Power = new Optional<int?>(create.Power),
      Accuracy = new Optional<int?>(create.Accuracy),
      PowerPoints = new Optional<int?>(create.PowerPoints)
    };

    MoveDto? move = await _moveService.UpdateAsync(id, payload);
    Assert.NotNull(move);

    Assert.Equal(id, move.Id);
    Assert.Equal(5, move.Version);
    Assert.Equal(_seeded.CreatedBy, move.CreatedBy);
    Assert.Equal(_seeded.CreatedOn, move.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, move.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, move.UpdatedOn, TimeSpan.FromSeconds(10));

    AssertUpdatedTackle(create, move);
  }

  private static CreateOrReplaceMovePayload CreateEmberPayload() => new()
  {
    Type = PokemonType.Fire,
    Category = MoveCategory.Special,
    Key = "ember",
    Name = " Ember ",
    Summary = "  A weak Fire attack.  ",
    Content = "   The target is attacked with small flames. May inflict a burn.   ",
    Accuracy = 100,
    Power = 40,
    PowerPoints = 25
  };

  private static CreateOrReplaceMovePayload CreateUpdatedTacklePayload() => new()
  {
    Type = PokemonType.Normal,
    Category = MoveCategory.Physical,
    Key = "tackle",
    Name = " Tackle ",
    Summary = "  A stronger physical attack.  ",
    Content = "   The target is slammed with increased force.   ",
    Accuracy = 100,
    Power = 45,
    PowerPoints = 35
  };

  private static void AssertEmber(CreateOrReplaceMovePayload payload, MoveDto move)
  {
    Assert.Equal(payload.Type, move.Type);
    Assert.Equal(payload.Category, move.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), move.Key);
    Assert.Equal(payload.Name?.Trim(), move.Name);
    Assert.Equal(payload.Summary?.Trim(), move.Summary);
    Assert.Equal(payload.Content?.Trim(), move.Content);
    Assert.Equal(payload.Accuracy, move.Accuracy);
    Assert.Equal(payload.Power, move.Power);
    Assert.Equal(payload.PowerPoints, move.PowerPoints);
  }

  private static void AssertUpdatedTackle(CreateOrReplaceMovePayload payload, MoveDto move)
  {
    Assert.Equal(payload.Type, move.Type);
    Assert.Equal(payload.Category, move.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), move.Key);
    Assert.Equal(payload.Name?.Trim(), move.Name);
    Assert.Equal(payload.Summary?.Trim(), move.Summary);
    Assert.Equal(payload.Content?.Trim(), move.Content);
    Assert.Equal(payload.Accuracy, move.Accuracy);
    Assert.Equal(payload.Power, move.Power);
    Assert.Equal(payload.PowerPoints, move.PowerPoints);
  }
}
