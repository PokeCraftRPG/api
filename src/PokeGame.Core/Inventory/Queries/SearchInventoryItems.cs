using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;
using PokeGame.Core.Trainers;

namespace PokeGame.Core.Inventory.Queries;

internal record SearchInventoryItemsQuery(Guid TrainerId, SearchInventoryItemsPayload Payload) : IQuery<SearchResults<InventoryItemDto>?>;

internal class SearchInventoryItemsQueryHandler : IQueryHandler<SearchInventoryItemsQuery, SearchResults<InventoryItemDto>?>
{
  private readonly IInventoryQuerier _inventoryQuerier;
  private readonly ITrainerQuerier _trainerQuerier;

  public SearchInventoryItemsQueryHandler(IInventoryQuerier inventoryQuerier, ITrainerQuerier trainerQuerier)
  {
    _inventoryQuerier = inventoryQuerier;
    _trainerQuerier = trainerQuerier;
  }

  public async Task<SearchResults<InventoryItemDto>?> HandleAsync(SearchInventoryItemsQuery query, CancellationToken cancellationToken)
  {
    SearchInventoryItemsPayload payload = query.Payload;
    payload.Validate();

    if (!await _trainerQuerier.ExistsAsync(query.TrainerId, cancellationToken))
    {
      return null;
    }

    return await _inventoryQuerier.SearchAsync(query.TrainerId, payload, cancellationToken);
  }
}
