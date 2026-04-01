using Bogus;
using Moq;
using PokeGame.Builders;
using PokeGame.Core.Moves;
using PokeGame.Core.Varieties.Models;

namespace PokeGame.Core.Varieties;

[Trait(Traits.Category, Categories.Unit)]
public class VarietyManagerTests
{
  private const string PropertyName = "PropertyName";

  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMoveQuerier> _moveQuerier = new();
  private readonly Mock<IVarietyQuerier> _varietyQuerier = new();
  private readonly Mock<IVarietyRepository> _varietyRepository = new();

  private readonly TestContext _context;
  private readonly VarietyManager _manager;

  public VarietyManagerTests()
  {
    _context = new(_faker);
    _manager = new(_context, _moveQuerier.Object, _varietyQuerier.Object, _varietyRepository.Object);
  }

  [Fact(DisplayName = "FindAsync: it should return the variety found by ID.")]
  public async Task Given_FoundById_When_FindAsync_Then_VarietyReturned()
  {
    Variety variety = VarietyBuilder.Pikachu(_faker, _context.World);
    _varietyRepository.Setup(x => x.LoadAsync(variety.Id, _cancellationToken)).ReturnsAsync(variety);

    Variety found = await _manager.FindAsync($"  {variety.EntityId.ToString().ToUpperInvariant()}  ", PropertyName, _cancellationToken);
    Assert.Same(variety, found);
  }

  [Fact(DisplayName = "FindAsync: it should return the variety found by key.")]
  public async Task Given_FoundByKey_When_FindAsync_Then_VarietyReturned()
  {
    Variety variety = VarietyBuilder.Pikachu(_faker, _context.World);
    _varietyRepository.Setup(x => x.LoadAsync(variety.Id, _cancellationToken)).ReturnsAsync(variety);

    string key = $"  {variety.Key.Value.ToUpperInvariant()}  ";
    _varietyQuerier.Setup(x => x.FindIdAsync(key, _cancellationToken)).ReturnsAsync(variety.Id);

    Variety found = await _manager.FindAsync(key, PropertyName, _cancellationToken);
    Assert.Same(variety, found);
  }

  [Fact(DisplayName = "FindAsync: it should throw InvalidOperationException when the variety was not loaded.")]
  public async Task Given_NotLoaded_When_FindAsync_Then_InvalidOperationException()
  {
    Variety variety = VarietyBuilder.Pikachu(_faker, _context.World);
    _varietyQuerier.Setup(x => x.FindIdAsync(variety.Key.Value, _cancellationToken)).ReturnsAsync(variety.Id);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _manager.FindAsync(variety.Key.Value, PropertyName, _cancellationToken));
    Assert.Equal($"The variety 'Id={variety.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "FindAsync: it should throw VarietyNotFoundException when the variety was not found.")]
  public async Task Given_NotFound_When_FindAsync_Then_VarietyNotFoundException()
  {
    string key = $"  {Guid.NewGuid().ToString().ToUpperInvariant()}  ";

    var exception = await Assert.ThrowsAsync<VarietyNotFoundException>(async () => await _manager.FindAsync(key, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(key, exception.Variety);
    Assert.Equal(PropertyName, exception.PropertyName);

    _varietyRepository.Verify(x => x.LoadAsync(new VarietyId(_context.WorldId, Guid.Parse(key)), _cancellationToken), Times.Once());
    _varietyQuerier.Verify(x => x.FindIdAsync(key, _cancellationToken), Times.Once());
  }

  [Fact(DisplayName = "FindMovesAsync: it should not query the database if there is no payload.")]
  public async Task Given_NoPayload_When_FindMovesAsync_Then_NotQueried()
  {
    await _manager.FindMovesAsync([], PropertyName, _cancellationToken);

    _moveQuerier.Verify(x => x.ListKeysAsync(_cancellationToken), Times.Never());
  }

  [Fact(DisplayName = "FindMovesAsync: it should return the correct regional numbers.")]
  public async Task Given_Payloads_When_FindMovesAsync_Then_RegionalNumbers()
  {
    Move thunderShock = MoveBuilder.ThunderShock(_faker);
    Move quickAttack = MoveBuilder.QuickAttack(_faker);
    Move sweetKiss = MoveBuilder.SweetKiss(_faker);
    MoveKey[] keys = new Move[] { thunderShock, quickAttack, sweetKiss }.Select(move => new MoveKey(move.Id, move.EntityId, move.Key.Value)).ToArray();
    _moveQuerier.Setup(x => x.ListKeysAsync(_cancellationToken)).ReturnsAsync(keys);

    VarietyMovePayload[] payloads =
    [
      new($"  {thunderShock.EntityId.ToString().ToUpperInvariant()}  "),
      new(quickAttack.Key.Value, level: 4),
      new($"  {sweetKiss.Key.Value.ToUpperInvariant()}  ", level: 1)
    ];
    IReadOnlyDictionary<MoveId, int?> varietyMoves = await _manager.FindMovesAsync(payloads, PropertyName, _cancellationToken);

    Assert.Equal(payloads.Length, varietyMoves.Count);
    Assert.Contains(varietyMoves, x => x.Key == thunderShock.Id && x.Value is null);
    Assert.Contains(varietyMoves, x => x.Key == quickAttack.Id && x.Value is not null && x.Value.Value == 4);
    Assert.Contains(varietyMoves, x => x.Key == sweetKiss.Id && x.Value is not null && x.Value.Value == 1);
  }

  [Fact(DisplayName = "FindMovesAsync: it should throw MovesNotFoundException when some moves were not found.")]
  public async Task Given_NotFound_When_FindMovesAsync_Then_MovesNotFoundException()
  {
    Move thunderShock = MoveBuilder.ThunderShock(_faker);
    Move quickAttack = MoveBuilder.QuickAttack(_faker);
    MoveKey[] keys = new Move[] { thunderShock, quickAttack }.Select(move => new MoveKey(move.Id, move.EntityId, move.Key.Value)).ToArray();
    _moveQuerier.Setup(x => x.ListKeysAsync(_cancellationToken)).ReturnsAsync(keys);

    Move sweetKiss = MoveBuilder.SweetKiss(_faker);
    Move agility = MoveBuilder.Agility(_faker);
    VarietyMovePayload[] payloads =
    [
      new($"  {thunderShock.EntityId.ToString().ToUpperInvariant()}  "),
      new($"  {quickAttack.Key.Value.ToUpperInvariant()}  ", level: 4),
      new(sweetKiss.EntityId.ToString(), level: 1),
      new(agility.Key.Value, level: 24)
    ];

    var exception = await Assert.ThrowsAsync<MovesNotFoundException>(async () => await _manager.FindMovesAsync(payloads, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal([sweetKiss.EntityId.ToString(), agility.Key.Value], exception.Moves);
    Assert.Equal(PropertyName, exception.PropertyName);
  }
}
