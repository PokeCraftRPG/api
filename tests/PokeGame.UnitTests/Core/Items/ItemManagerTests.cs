using Bogus;
using Moq;
using PokeGame.Builders;

namespace PokeGame.Core.Items;

[Trait(Traits.Category, Categories.Unit)]
public class ItemManagerTests
{
  private const string PropertyName = "PropertyName";

  private readonly CancellationToken _cancellationToken = default;
  private readonly Faker _faker = new();

  private readonly Mock<IItemQuerier> _itemQuerier = new();
  private readonly Mock<IItemRepository> _itemRepository = new();

  private readonly TestContext _context;
  private readonly ItemManager _manager;

  public ItemManagerTests()
  {
    _context = new(_faker);
    _manager = new(_context, _itemQuerier.Object, _itemRepository.Object);
  }

  [Fact(DisplayName = "FindAsync: it should return the item found by ID.")]
  public async Task Given_FoundById_When_FindAsync_Then_ItemReturned()
  {
    Item item = ItemBuilder.Potion(_faker, _context.World);
    _itemRepository.Setup(x => x.LoadAsync(item.Id, _cancellationToken)).ReturnsAsync(item);

    Item found = await _manager.FindAsync($"  {item.EntityId.ToString().ToUpperInvariant()}  ", PropertyName, _cancellationToken);
    Assert.Same(item, found);
  }

  [Fact(DisplayName = "FindAsync: it should return the item found by key.")]
  public async Task Given_FoundByKey_When_FindAsync_Then_ItemReturned()
  {
    Item item = ItemBuilder.Potion(_faker, _context.World);
    _itemRepository.Setup(x => x.LoadAsync(item.Id, _cancellationToken)).ReturnsAsync(item);

    string key = $"  {item.Key.Value.ToUpperInvariant()}  ";
    _itemQuerier.Setup(x => x.FindIdAsync(key, _cancellationToken)).ReturnsAsync(item.Id);

    Item found = await _manager.FindAsync(key, PropertyName, _cancellationToken);
    Assert.Same(item, found);
  }

  [Fact(DisplayName = "FindAsync: it should throw InvalidOperationException when the item was not loaded.")]
  public async Task Given_NotLoaded_When_FindAsync_Then_InvalidOperationException()
  {
    Item item = ItemBuilder.Potion(_faker, _context.World);
    _itemQuerier.Setup(x => x.FindIdAsync(item.Key.Value, _cancellationToken)).ReturnsAsync(item.Id);

    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await _manager.FindAsync(item.Key.Value, PropertyName, _cancellationToken));
    Assert.Equal($"The item 'Id={item.Id}' was not loaded.", exception.Message);
  }

  [Fact(DisplayName = "FindAsync: it should throw ItemNotFoundException when the item was not found.")]
  public async Task Given_NotFound_When_FindAsync_Then_ItemNotFoundException()
  {
    string key = $"  {Guid.NewGuid().ToString().ToUpperInvariant()}  ";

    var exception = await Assert.ThrowsAsync<ItemNotFoundException>(async () => await _manager.FindAsync(key, PropertyName, _cancellationToken));
    Assert.Equal(_context.WorldUid, exception.WorldId);
    Assert.Equal(key, exception.Item);
    Assert.Equal(PropertyName, exception.PropertyName);

    _itemRepository.Verify(x => x.LoadAsync(new ItemId(_context.WorldId, Guid.Parse(key)), _cancellationToken), Times.Once());
    _itemQuerier.Verify(x => x.FindIdAsync(key, _cancellationToken), Times.Once());
  }
}
