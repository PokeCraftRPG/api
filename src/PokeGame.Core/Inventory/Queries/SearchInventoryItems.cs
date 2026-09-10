using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Core.Inventory.Queries;

internal record SearchInventoryItemsQuery(Guid TrainerId, SearchInventoryItemsPayload Payload) : IQuery<SearchResults<InventoryItemDto>?>;

internal class SearchInventoryItemsQueryHandler : IQueryHandler<SearchInventoryItemsQuery, SearchResults<InventoryItemDto>?>
{
  private readonly IInventoryQuerier _inventoryQuerier;

  public SearchInventoryItemsQueryHandler(IInventoryQuerier inventoryQuerier)
  {
    _inventoryQuerier = inventoryQuerier;
  }

  public async Task<SearchResults<InventoryItemDto>?> HandleAsync(SearchInventoryItemsQuery query, CancellationToken cancellationToken)
  {
    SearchInventoryItemsPayload payload = query.Payload;
    payload.Validate();

    return await _inventoryQuerier.SearchAsync(query.TrainerId, payload, cancellationToken);
  }
}
