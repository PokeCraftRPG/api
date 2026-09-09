using Krakenar.Contracts.Search;
using PokeGame.Core.Items.Models;

namespace PokeGame.Core.Items;

public interface IItemQuerier
{
  Task<ItemId?> GetIdAsync(Key key, CancellationToken cancellationToken = default);

  Task<ItemDto> ReadAsync(Item item, CancellationToken cancellationToken = default);
  Task<ItemDto?> ReadAsync(ItemId id, CancellationToken cancellationToken = default);
  Task<ItemDto?> ReadAsync(Guid id, CancellationToken cancellationToken = default);
  Task<ItemDto?> ReadAsync(string key, CancellationToken cancellationToken = default);

  Task<SearchResults<ItemDto>> SearchAsync(SearchItemsPayload payload, CancellationToken cancellationToken = default);
}
