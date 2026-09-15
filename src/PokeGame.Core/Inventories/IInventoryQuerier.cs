using Krakenar.Contracts.Search;
using PokeGame.Core.Inventories.Models;

namespace PokeGame.Core.Inventories;

public interface IInventoryQuerier
{
  Task<InventoryItemDto?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken = default);
  Task<SearchResults<InventoryItemDto>> SearchAsync(Guid trainerId, SearchInventoryItemsPayload payload, CancellationToken cancellationToken = default);
}
