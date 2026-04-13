using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Core.Inventory.Queries;

internal record SearchInventoryQuery(Guid TrainerId) : IQuery<SearchResults<InventoryItemModel>>;

internal class SearchInventoryQueryHandler : IQueryHandler<SearchInventoryQuery, SearchResults<InventoryItemModel>>
{
  private readonly IInventoryQuerier _inventoryQuerier;

  public SearchInventoryQueryHandler(IInventoryQuerier inventoryQuerier)
  {
    _inventoryQuerier = inventoryQuerier;
  }

  public async Task<SearchResults<InventoryItemModel>> HandleAsync(SearchInventoryQuery query, CancellationToken cancellationToken)
  {
    return await _inventoryQuerier.SearchAsync(query.TrainerId, cancellationToken);
  }
}
