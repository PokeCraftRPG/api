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

  private readonly TestContext _context;
  private readonly VarietyManager _manager;

  public VarietyManagerTests()
  {
    _context = new(_faker);
    _manager = new(_context, _moveQuerier.Object);
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
    Move thunderShock = new MoveBuilder(_faker).WithType(PokemonType.Electric).WithCategory(MoveCategory.Special).WithKey(new Slug("thunder-shock")).Build();
    Move quickAttack = new MoveBuilder(_faker).WithType(PokemonType.Normal).WithCategory(MoveCategory.Physical).WithKey(new Slug("quick-attack")).Build();
    Move sweetKiss = new MoveBuilder(_faker).WithType(PokemonType.Fairy).WithCategory(MoveCategory.Status).WithKey(new Slug("sweet-kiss")).Build();
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
    Move thunderShock = new MoveBuilder(_faker).WithType(PokemonType.Electric).WithCategory(MoveCategory.Special).WithKey(new Slug("thunder-shock")).Build();
    Move quickAttack = new MoveBuilder(_faker).WithType(PokemonType.Normal).WithCategory(MoveCategory.Physical).WithKey(new Slug("quick-attack")).Build();
    MoveKey[] keys = new Move[] { thunderShock, quickAttack }.Select(move => new MoveKey(move.Id, move.EntityId, move.Key.Value)).ToArray();
    _moveQuerier.Setup(x => x.ListKeysAsync(_cancellationToken)).ReturnsAsync(keys);

    Move sweetKiss = new MoveBuilder(_faker).WithType(PokemonType.Fairy).WithCategory(MoveCategory.Status).WithKey(new Slug("sweet-kiss")).Build();
    Move agility = new MoveBuilder(_faker).WithType(PokemonType.Psychic).WithCategory(MoveCategory.Status).WithKey(new Slug("agility")).Build();
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
