using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Core.Inventory.Queries;

internal record SearchInventoryQuery(Guid TrainerId, SearchInventoryItemsPayload Payload) : IQuery<SearchResults<InventoryItemDto>?>;

internal class SearchInventoryQueryHandler : IQueryHandler<SearchInventoryQuery, SearchResults<InventoryItemDto>?>
{
  private readonly IInventoryQuerier _inventoryQuerier;

  public SearchInventoryQueryHandler(IInventoryQuerier inventoryQuerier)
  {
    _inventoryQuerier = inventoryQuerier;
  }

  public async Task<SearchResults<InventoryItemDto>?> HandleAsync(SearchInventoryQuery query, CancellationToken cancellationToken)
  {
    SearchInventoryItemsPayload payload = query.Payload;
    payload.Validate();

    return await _inventoryQuerier.SearchAsync(query.TrainerId, payload, cancellationToken);
  }
}
