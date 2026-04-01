using Krakenar.Contracts.Search;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core;
using PokeGame.Core.Moves;
using PokeGame.Core.Moves.Models;

namespace PokeGame;

[Trait(Traits.Category, Categories.Integration)]
public class MoveIntegrationTests : IntegrationTests
{
  private readonly IMoveRepository _moveRepository;
  private readonly IMoveService _moveService;

  private Move _move = null!;

  public MoveIntegrationTests() : base()
  {
    _moveRepository = ServiceProvider.GetRequiredService<IMoveRepository>();
    _moveService = ServiceProvider.GetRequiredService<IMoveService>();
  }

  public override async Task InitializeAsync()
  {
    await base.InitializeAsync();

    _move = new MoveBuilder(Faker).WithWorld(World).WithType(PokemonType.Electric).WithCategory(MoveCategory.Special).Build();
    await _moveRepository.SaveAsync(_move);
  }

  [Theory(DisplayName = "It should create a new move.")]
  [InlineData(false)]
  [InlineData(true)]
  public async Task Given_DoesNotExist_When_CreateOrReplace_Then_Created(bool withId)
  {
    Guid? id = withId ? Guid.NewGuid() : null;

    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Electric,
      Category = MoveCategory.Special,
      Key = "thunder-shock",
      Name = " Thunder Shock ",
      Description = "  The user attacks the target with a jolt of electricity. This may also leave the target with paralysis.  ",
      Accuracy = 100,
      Power = 40,
      PowerPoints = 30,
      Url = "https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)",
      Notes = "   Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed.   "
    };

    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id);
    Assert.True(result.Created);
    Assert.NotNull(result.Move);

    MoveModel move = result.Move;
    if (id.HasValue)
    {
      Assert.Equal(id.Value, move.Id);
    }
    else
    {
      Assert.NotEqual(default, move.Id);
    }
    Assert.Equal(2, move.Version);
    Assert.Equal(Actor, move.CreatedBy);
    Assert.Equal(DateTime.UtcNow, move.CreatedOn, TimeSpan.FromSeconds(10));
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
    Assert.Equal(payload.Url, move.Url);
    Assert.Equal(payload.Notes.Trim(), move.Notes);
  }

  [Fact(DisplayName = "It should read a move by ID.")]
  public async Task Given_Id_When_Read_Then_Found()
  {
    Guid id = _move.EntityId;
    MoveModel? move = await _moveService.ReadAsync(id);
    Assert.NotNull(move);
    Assert.Equal(id, move.Id);
  }

  [Fact(DisplayName = "It should read a move by key.")]
  public async Task Given_Key_When_Read_Then_Found()
  {
    MoveModel? move = await _moveService.ReadAsync(id: null, $" {_move.Key.Value.ToUpperInvariant()} ");
    Assert.NotNull(move);
    Assert.Equal(_move.EntityId, move.Id);
  }

  [Fact(DisplayName = "It should replace an existing move.")]
  public async Task Given_Exists_When_CreateOrReplace_Then_Replaced()
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Type = PokemonType.Electric,
      Category = MoveCategory.Special,
      Key = "thunder-shock",
      Name = " Thunder Shock ",
      Description = "  The user attacks the target with a jolt of electricity. This may also leave the target with paralysis.  ",
      Accuracy = 100,
      Power = 40,
      PowerPoints = 30,
      Url = "https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)",
      Notes = "   Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed.   "
    };
    Guid id = _move.EntityId;

    CreateOrReplaceMoveResult result = await _moveService.CreateOrReplaceAsync(payload, id);
    Assert.False(result.Created);
    Assert.NotNull(result.Move);

    MoveModel move = result.Move;
    Assert.Equal(id, move.Id);
    Assert.Equal(3, move.Version);
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
    Assert.Equal(payload.Url, move.Url);
    Assert.Equal(payload.Notes.Trim(), move.Notes);
  }

  [Fact(DisplayName = "It should return the correct search results.")]
  public async Task Given_Payload_When_SearchAsync_Then_Results()
  {
    Move agility = MoveBuilder.Agility(Faker, World);
    Move quickAttack = MoveBuilder.QuickAttack(Faker, World);
    Move sweetKiss = MoveBuilder.SweetKiss(Faker, World);
    Move thunderPunch = MoveBuilder.ThunderPunch(Faker, World);
    await _moveRepository.SaveAsync([agility, quickAttack, sweetKiss, thunderPunch]);

    SearchMovesPayload payload = new()
    {
      Ids = [agility.EntityId, quickAttack.EntityId, Guid.Empty, thunderPunch.EntityId],
      Skip = 1,
      Limit = 1
    };
    payload.Search.Operator = SearchOperator.Or;
    payload.Search.Terms.Add(new SearchTerm("%u%"));
    payload.Search.Terms.Add(new SearchTerm("%w%"));
    payload.Sort.Add(new MoveSortOption(MoveSort.Key, isDescending: true));

    SearchResults<MoveModel> results = await _moveService.SearchAsync(payload);
    Assert.Equal(2, results.Total);

    MoveModel move = Assert.Single(results.Items);
    Assert.Equal(quickAttack.EntityId, move.Id);
  }

  [Fact(DisplayName = "It should return the correct search results (Category).")]
  public async Task Given_Category_When_SearchAsync_Then_Results()
  {
    Move sweetKiss = MoveBuilder.SweetKiss(Faker, World);
    Move thunderPunch = MoveBuilder.ThunderPunch(Faker, World);
    Move thunderShock = MoveBuilder.ThunderShock(Faker, World);
    await _moveRepository.SaveAsync([sweetKiss, thunderPunch, thunderShock]);

    SearchMovesPayload payload = new()
    {
      Category = Faker.PickRandom<MoveCategory>()
    };

    SearchResults<MoveModel> results = await _moveService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    MoveModel move = Assert.Single(results.Items);
    switch (payload.Category)
    {
      case MoveCategory.Physical:
        Assert.Equal(thunderPunch.EntityId, move.Id);
        break;
      case MoveCategory.Special:
        Assert.Equal(thunderShock.EntityId, move.Id);
        break;
      case MoveCategory.Status:
        Assert.Equal(sweetKiss.EntityId, move.Id);
        break;
      default:
        Assert.Fail($"The move category '{payload.Category}' is not valid.");
        break;
    }
  }

  [Fact(DisplayName = "It should return the correct search results (Type).")]
  public async Task Given_Type_When_SearchAsync_Then_Results()
  {
    Move agility = MoveBuilder.Agility(Faker, World);
    Move quickAttack = MoveBuilder.QuickAttack(Faker, World);
    Move sweetKiss = MoveBuilder.SweetKiss(Faker, World);
    Move thunderShock = MoveBuilder.ThunderShock(Faker, World);
    await _moveRepository.SaveAsync([agility, quickAttack, sweetKiss, thunderShock]);

    SearchMovesPayload payload = new()
    {
      Type = Faker.PickRandom(PokemonType.Electric, PokemonType.Fairy, PokemonType.Normal, PokemonType.Psychic)
    };

    SearchResults<MoveModel> results = await _moveService.SearchAsync(payload);
    Assert.Equal(1, results.Total);

    MoveModel move = Assert.Single(results.Items);
    switch (payload.Type)
    {
      case PokemonType.Electric:
        Assert.Equal(thunderShock.EntityId, move.Id);
        break;
      case PokemonType.Fairy:
        Assert.Equal(sweetKiss.EntityId, move.Id);
        break;
      case PokemonType.Normal:
        Assert.Equal(quickAttack.EntityId, move.Id);
        break;
      case PokemonType.Psychic:
        Assert.Equal(agility.EntityId, move.Id);
        break;
      default:
        Assert.Fail($"The Pokémon type '{payload.Type}' is not valid.");
        break;
    }
  }

  [Fact(DisplayName = "It should throw PropertyConflictException when there is a key conflict.")]
  public async Task Given_KeyConflict_When_Create_Then_PropertyConflictException()
  {
    CreateOrReplaceMovePayload payload = new()
    {
      Key = _move.Key.Value.ToUpperInvariant(),
      PowerPoints = _move.PowerPoints.Value
    };
    Guid id = Guid.NewGuid();

    var exception = await Assert.ThrowsAsync<PropertyConflictException<string>>(async () => await _moveService.CreateOrReplaceAsync(payload, id));
    Assert.Equal(World.Id.ToGuid(), exception.WorldId);
    Assert.Equal("Move", exception.EntityKind);
    Assert.Equal(id, exception.EntityId);
    Assert.Equal(_move.EntityId, exception.ConflictId);
    Assert.Equal(_move.Key.Value, exception.AttemptedValue);
    Assert.Equal("Key", exception.PropertyName);
  }

  [Fact(DisplayName = "It should update an existing move.")]
  public async Task Given_Exists_When_Update_Then_Updated()
  {
    Guid id = _move.EntityId;
    UpdateMovePayload payload = new()
    {
      Name = new Optional<string>(" Thunder Shock "),
      Description = new Optional<string>("  The user attacks the target with a jolt of electricity. This may also leave the target with paralysis.  "),
      Accuracy = new Optional<byte?>(100),
      Power = new Optional<byte?>(40),
      PowerPoints = 30,
      Url = new Optional<string>("https://bulbapedia.bulbagarden.net/wiki/Thunder_Shock_(move)"),
      Notes = new Optional<string>("   Ranged Electric special attack: 40 power, 100% accuracy; on hit, target has a 10% chance to be paralyzed.   ")
    };

    MoveModel? move = await _moveService.UpdateAsync(id, payload);
    Assert.NotNull(move);

    Assert.Equal(id, move.Id);
    Assert.Equal(2, move.Version);
    Assert.Equal(Actor, move.UpdatedBy);
    Assert.Equal(DateTime.UtcNow, move.UpdatedOn, TimeSpan.FromSeconds(10));

    Assert.Equal(_move.Key.Value, move.Key);
    Assert.Equal(payload.Name.Value?.Trim(), move.Name);
    Assert.Equal(payload.Description.Value?.Trim(), move.Description);
    Assert.Equal(payload.Accuracy.Value, move.Accuracy);
    Assert.Equal(payload.Power.Value, move.Power);
    Assert.Equal(payload.PowerPoints, move.PowerPoints);
    Assert.Equal(payload.Url.Value, move.Url);
    Assert.Equal(payload.Notes.Value?.Trim(), move.Notes);
  }
}
