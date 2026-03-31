using PokeGame.Core.Items.Models;

namespace PokeGame.Core.Items;

public interface IItemQuerier
{
  Task EnsureUnicityAsync(Item item, CancellationToken cancellationToken = default);

  Task<ItemModel> ReadAsync(Item item, CancellationToken cancellationToken = default);
  Task<ItemModel?> ReadAsync(ItemId id, CancellationToken cancellationToken = default);
  Task<ItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<ItemModel?> ReadAsync(string key, CancellationToken cancellationToken = default);
}
