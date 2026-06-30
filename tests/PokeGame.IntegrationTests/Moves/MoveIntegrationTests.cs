using Krakenar.Contracts;
using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;
using PokeGame.Core.Permissions;
using PokeGame.Core.Worlds;

namespace PokeGame.Moves;

[Trait(Traits.Category, Categories.Integration)]
public class MoveIntegrationTests : IntegrationTests
{
  private readonly IMoveRepository _moveRepository;
  private readonly IMoveService _moveService;
  private readonly IWorldRepository _worldRepository;

  private Move _move = null!;

  public MoveIntegrationTests() : base()
  {
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _moveService = ServiceProvider.GetRequiredService<IMoveService>();
    _worldRepository = ServiceProvider.GetRequiredService<IWorldRepository>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _move = new MoveBuilder(Faker).WithWorld(Context.World).Build();
    _moveRepository.Add(_move);
    await Context.SaveChangesAsync();
  }

  [Theory(DisplayName = "It should create a new move.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_NotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Normal,
      Category = MoveCategory.Status,
      Key = "Growl",
      Name = " Growl ",
      Description = "  The user growls in an endearing way, making opposing Pokémon less wary. This lowers their Attack stats.  ",
      Accuracy = 100,
      PowerPoints = 40
    };
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    MoveModel move = result.Move;
    Assert.NotNull(move);

    if (id.HasValue)
    {
      Assert.Equal(id.Value, move.Id);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, move.Id);
    }
    Assert.Equal(1, move.Version);
    Assert.Equal(Actor, move.CreatedBy);
    Assert.Equal(DateTime.UtcNow, move.CreatedOn, TimeSpan.FromSeconds(1));
    Assert.Equal(move.CreatedBy, move.UpdatedBy);
    Assert.Equal(move.CreatedOn, move.UpdatedOn);

    Assert.Equal(payload.Type, move.Type);
    Assert.Equal(payload.Category, move.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), move.Key);
    Assert.Equal(payload.Name.Trim(), move.Name);
    Assert.Equal(payload.Description.Trim(), move.Description);
    Assert.Equal(payload.Accuracy, move.Accuracy);
    Assert.Equal(payload.Power, move.Power);
    Assert.Equal(payload.PowerPoints, move.PowerPoints);
  }

  [Fact(DisplayName = "It should read a move by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    MoveModel? move = await _moveService.ReadAsync(_move.Id);
    Assert.NotNull(move);
    Assert.Equal(_move.Id, move.Id);
  }

  [Fact(DisplayName = "It should read a move by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    MoveModel? move = await _moveService.ReadAsync(id: null, $" {_move.Key.ToUpperInvariant()} ");
    Assert.NotNull(move);
    Assert.Equal(_move.Id, move.Id);
  }

  [Fact(DisplayName = "It should replace an existing move.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Normal,
      Category = MoveCategory.Status,
      Key = "Growl",
      Name = " Growl ",
      Description = "  The user growls in an endearing way, making opposing Pokémon less wary. This lowers their Attack stats.  ",
      Accuracy = 100,
      PowerPoints = 40
    };

    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, _move.Id);
    Assert.False(result.Created);
    MoveModel move = result.Move;
    Assert.NotNull(move);

    Assert.Equal(_move.Id, move.Id);
    Assert.Equal(_move.Version, move.Version);
    Assert.Equal(_move.CreatedBy, move.CreatedBy.Id);
    Assert.Equal(_move.CreatedOn, move.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, move.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, move.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(_move.Type, move.Type);
    Assert.Equal(_move.Category, move.Category);
    Assert.Equal(SlugHelper.Format(payload.Key), move.Key);
    Assert.Equal(payload.Name.Trim(), move.Name);
    Assert.Equal(payload.Description.Trim(), move.Description);
    Assert.Equal(payload.Accuracy, move.Accuracy);
    Assert.Equal(payload.Power, move.Power);
    Assert.Equal(payload.PowerPoints, move.PowerPoints);
  }

  [Fact(DisplayName = "It should return empty search results when no move is matching.")]
  public async Task Given_NoneMatching_When_Search_Then_EmptyResults()
  {
    Context.World = new WorldBuilder().Build();

    SearchResults<MoveModel> results = await _moveService.SearchAsync(new SearchMovesPayload());
    Assert.Equal(0, results.Total);
    Assert.Empty(results.Items);
  }

  [Fact(DisplayName = "It should return null when the user does not own the world.")]
  public async Task Given_NotOwner_When_Read_Then_NullReturned()
  {
    Context.World = new WorldBuilder().Build();

    Assert.Null(await _moveService.ReadAsync(_move.Id, $" {_move.Key.ToUpperInvariant()} "));
  }

  [Fact(DisplayName = "It should return null when the move does not exist.")]
  public async Task Given_NotExist_When_Update_Then_NullReturned()
  {
    Assert.Null(await _moveService.UpdateAsync(Guid.Empty, new UpdateMovePayload()));
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Matches_When_Search_Then_CorrectResults()
  {
    World world = new WorldBuilder(Faker).WithOwner(Context.User).WithKey("the-new-world").Build();
    _worldRepository.Add(world);

    Move quickAttack = MoveBuilder.QuickAttack(Faker, world);
    Move sweetKiss = MoveBuilder.SweetKiss(Faker, world);
    Move tailWhip = MoveBuilder.TailWhip(Faker, world);
    Move thunderShock = MoveBuilder.ThunderShock(Faker, world);
    _moveRepository.Add(quickAttack, sweetKiss, tailWhip, thunderShock);

    Context.World = world;
    await Context.SaveChangesAsync();

    SearchMovesPayload payload = new()
    {
      Skip = 1,
      Limit = 1
    };
    payload.Ids.AddRange(quickAttack.Id, Guid.Empty, tailWhip.Id, thunderShock.Id);
    payload.Search.Operator = SearchOperator.Or;
    payload.Search.Terms.Add(new SearchTerm("%h%"));
    payload.Search.Terms.Add(new SearchTerm("_W%"));
    payload.Sort.Add(new MoveSortOption(MoveSort.Key));

    SearchResults<MoveModel> results = await _moveService.SearchAsync(payload);
    Assert.Equal(2, results.Total);
    Assert.Equal(thunderShock.Id, Assert.Single(results.Items).Id);
  }

  [Theory(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (CreateOrReplace).")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_KeyConflict_When_CreateOrReplace_Then_KeyAlreadyUsedException(bool exists)
  {
    Move tailWhip = MoveBuilder.TailWhip(Faker, Context.World);
    _moveRepository.Add(tailWhip);
    await Context.SaveChangesAsync();

    CreateOrReplaceMovePayload payload = new()
    {
      Type = _move.Type,
      Category = _move.Category,
      Key = _move.Key,
      Accuracy = _move.Accuracy,
      Power = _move.Power,
      PowerPoints = _move.PowerPoints
    };
    Guid? id = exists ? tailWhip.Id : null;

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _moveService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(Context.WorldId, exception.WorldId);
    Assert.Equal(Move.ResourceKind, exception.ResourceKind);
    if (id.HasValue)
    {
      Assert.Equal(id.Value, exception.ResourceId);
    }
    else
    {
      Assert.NotEqual(Guid.Empty, exception.ResourceId);
    }
    Assert.Equal(_move.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw KeyAlreadyUsedException when there is a key conflict (Update).")]
  public async Task Given_KeyConflict_When_Update_Then_KeyAlreadyUsedException()
  {
    Move thunderShock = MoveBuilder.ThunderShock(Faker, Context.World);
    _moveRepository.Add(thunderShock);
    await Context.SaveChangesAsync();

    UpdateMovePayload payload = new()
    {
      Key = _move.Key
    };

    var exception = await Assert.ThrowsAsync<KeyAlreadyUsedException>(async () => await _moveService.UpdateAsync(thunderShock.Id, payload));
    Assert.Equal(Context.WorldId, exception.WorldId);
    Assert.Equal(Move.ResourceKind, exception.ResourceKind);
    Assert.Equal(thunderShock.Id, exception.ResourceId);
    Assert.Equal(_move.Id, exception.ConflictId);
    Assert.Equal(payload.Key, exception.AttemptedKey);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when creating a new move.")]
  public async Task Given_NotExist_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Context.World = world;
    await Context.SaveChangesAsync();

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Normal,
      Category = MoveCategory.Status,
      Key = "Growl",
      Name = " Growl ",
      Description = "  The user growls in an endearing way, making opposing Pokémon less wary. This lowers their Attack stats.  ",
      Accuracy = 100,
      PowerPoints = 40
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _moveService.CreateOrReplaceAsync(payload));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.CreateMove, exception.Action);
    Assert.Equal(world.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when replacing an existing move.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Move move = new MoveBuilder(Faker).WithWorld(world).Build();
    _moveRepository.Add(move);

    Context.World = world;
    await Context.SaveChangesAsync();

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Normal,
      Category = MoveCategory.Status,
      Key = "Growl",
      Name = " Growl ",
      Description = "  The user growls in an endearing way, making opposing Pokémon less wary. This lowers their Attack stats.  ",
      Accuracy = 100,
      PowerPoints = 40
    };

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _moveService.CreateOrReplaceAsync(payload, move.Id));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(move.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when updating an existing move.")]
  public async Task Given_Exists_When_Update_Then_PermissionDeniedException()
  {
    World world = new WorldBuilder().WithKey("another-world").Build();
    _worldRepository.Add(world);

    Move move = new MoveBuilder(Faker).WithWorld(world).Build();
    _moveRepository.Add(move);

    Context.World = world;
    await Context.SaveChangesAsync();

    var exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _moveService.UpdateAsync(move.Id, new UpdateMovePayload()));
    Assert.Equal(Context.UserId, exception.UserId);
    Assert.Equal(Actions.Update, exception.Action);
    Assert.Equal(move.Identifier.ToString(), exception.Resource);
  }

  [Fact(DisplayName = "It should throw TooManyResultsException when many move were found.")]
  public async Task Given_ManyMoveFound_When_Read_Then_TooManyResultsException()
  {
    Move quickAttack = MoveBuilder.QuickAttack(Faker, Context.World);
    _moveRepository.Add(quickAttack);
    await Context.SaveChangesAsync();

    var exception = await Assert.ThrowsAsync<TooManyResultsException<MoveModel>>(async () => await _moveService.ReadAsync(_move.Id, $" {quickAttack.Key.ToUpperInvariant()} "));
    Assert.Equal(1, exception.ExpectedCount);
    Assert.Equal(2, exception.ActualCount);
  }

  [Fact(DisplayName = "It should update an existing move.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _move.Id;
    UpdateMovePayload payload = new()
    {
      Key = "Growl",
      Name = new Optional<string>(" Growl "),
      Description = new Optional<string>("  The user growls in an endearing way, making opposing Pokémon less wary. This lowers their Attack stats.  "),
      Accuracy = new Optional<int?>(100),
      PowerPoints = 40
    };

    MoveModel? move = await _moveService.UpdateAsync(id, payload);
    Assert.NotNull(move);

    Assert.Equal(id, move.Id);
    Assert.Equal(_move.Version, move.Version);
    Assert.Equal(_move.CreatedBy, move.CreatedBy.Id);
    Assert.Equal(_move.CreatedOn, move.CreatedOn, TimeSpan.FromMilliseconds(1));
    Assert.Equal(Actor, move.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, move.UpdatedOn, TimeSpan.FromSeconds(1));

    Assert.Equal(SlugHelper.Format(payload.Key), move.Key);
    Assert.Equal(payload.Name.Value?.Trim(), move.Name);
    Assert.Equal(payload.Description.Value?.Trim(), move.Description);
  }
}
