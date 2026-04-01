using Bogus;
using Moq;
using PokeGame.Builders;

namespace PokeGame.Core.Moves;

[Trait(Traits.Category, Categories.Unit)]
public class MoveManagerTests
{
  private const string PropertyName = "PropertyName";

  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IMoveQuerier> _moveQuerier = new();
  private readonly Mock<IMoveRepository> _moveRepository = new();

  private readonly TestContext _context;
  private readonly MoveManager _manager;

  public MoveManagerTests()
  {
    _context = new(_faker);
    _manager = new(_context, _moveQuerier.Object, _moveRepository.Object);
  }

  [Fact(DisplayName = "FindAsync: it should return the move found by ID.")]
  public async Task Given_FoundById_When_FindAsync_Then_MoveReturned()
  {
    Move move = MoveBuilder.Agility(_faker, _context.World);
    _moveRepository.Setup(x => x.LoadAsync(move.Id, _cancellationToken)).ReturnsAsync(move);

    Move found = await _manager.FindAsync($"  {move.EntityId.ToString().ToUpperInvariant()}  ", PropertyName, _cancellationToken);
    Assert.Same(move, found);
  }

  [Fact(DisplayName = "FindAsync: it should return the move found by key.")]
  public async Task Given_FoundByKey_When_FindAsync_Then_MoveReturned()
  {
    Move move = MoveBuilder.QuickAttack(_faker, _context.World);
    _moveRepository.Setup(x => x.LoadAsync(move.Id, _cancellationToken)).ReturnsAsync(move);

    string key = $"  {move.Key.Value.ToUpperInvariant()}  ";
    _moveQuerier.Setup(x => x.FindIdAsync(key, _cancellationToken)).ReturnsAsync(move.Id);

    Move found = await _manager.FindAsync(key, PropertyName, _cancellationToken);
    Assert.Same(move, found);
  }

  [Fact(DisplayName = "FindAsync: it should throw InvalidOperationException when the move was not loaded.")]
  public async Task Given_NotLoaded_When_FindAsync_Then_InvalidOperationException()
  {
    Move move = MoveBuilder.ThunderShock(_faker, _context.World);
    _moveQuerier.Setup(x => x.FindIdAsync(move.Key.Value, _cancellationToken)).ReturnsAsync(move.Id);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _manager.FindAsync(move.Key.Value, PropertyName, _cancellationToken));
    Assert.Equal($"The move 'Id={move.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "FindAsync: it should throw MoveNotFoundException when the species was not found.")]
  public async Task Given_NotFound_When_FindAsync_Then_MoveNotFoundException()
  {
    string key = $"  {Guid.NewGuid().ToString().ToUpperInvariant()}  ";

    var exception = await Assert.ThrowsAsync<MoveNotFoundException>(async () => await _manager.FindAsync(key, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(key, exception.Move);
    Assert.Equal(PropertyName, exception.PropertyName);

    _moveRepository.Verify(x => x.LoadAsync(new MoveId(_context.WorldId, Guid.Parse(key)), _cancellationToken), Times.Once());
    _moveQuerier.Verify(x => x.FindIdAsync(key, _cancellationToken), Times.Once());
  }
}
