using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Core.Inventory.Queries;

internal record SearchInventoryQuery(Guid TrainerId) : IQuery<SearchResults<InventoryItemDto>?>;

internal class SearchInventoryQueryHandler : IQueryHandler<SearchInventoryQuery, SearchResults<InventoryItemDto>?>
{
  private readonly IInventoryQuerier _inventoryQuerier;

  public SearchInventoryQueryHandler(IInventoryQuerier inventoryQuerier)
  {
    _inventoryQuerier = inventoryQuerier;
  }

  public async Task<SearchResults<InventoryItemDto>?> HandleAsync(SearchInventoryQuery query, CancellationToken cancellationToken)
  {
    return await _inventoryQuerier.SearchAsync(query.TrainerId, cancellationToken);
  }
}
