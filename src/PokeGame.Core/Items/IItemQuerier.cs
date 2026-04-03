using Krakenar.Contracts.Search;
using PokeGame.Core.Items.Models;

namespace PokeGame.Core.Items;

public interface IItemQuerier
{
  Task EnsureUnicityAsync(Item item, CancellationToken cancellationToken = default);

  Task<ItemId?> FindIdAsync(string key, CancellationToken cancellationToken = default);

  Task<ItemModel> ReadAsync(Item item, CancellationToken cancellationToken = default);
  Task<ItemModel?> ReadAsync(ItemId id, CancellationToken cancellationToken = default);
  Task<ItemModel?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<ItemModel?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<ItemModel>> SearchAsync(SearchItemsPayload payload, CancellationToken cancellationToken = default);
}
