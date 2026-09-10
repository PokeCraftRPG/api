using Krakenar.Contracts.Search;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Core.Inventory;

public interface IInventoryQuerier
{
  Task<InventoryItemDto?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken = default);
  Task<SearchResults<InventoryItemDto>?> SearchAsync(Guid trainerId, SearchInventoryItemsPayload payload, CancellationToken cancellationToken = default);
}
