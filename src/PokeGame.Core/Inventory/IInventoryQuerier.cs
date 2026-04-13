using Krakenar.Contracts.Search;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Core.Inventory;

public interface IInventoryQuerier
{
  Task<InventoryItemModel?> ReadAsync(Guid trainerId, Guid itemId, CancellationToken cancellationToken = default);
  Task<SearchResults<InventoryItemModel>> SearchAsync(Guid trainerId, CancellationToken cancellationToken = default);
}
