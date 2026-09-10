using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Core.Inventory.Queries;

internal record ReadInventoryItemQuery(Guid TrainerId, Guid ItemId) : IQuery<InventoryItemDto?>;

internal class ReadInventoryItemQueryHandler : IQueryHandler<ReadInventoryItemQuery, InventoryItemDto?>
{
  private readonly IInventoryQuerier _inventoryQuerier;

  public ReadInventoryItemQueryHandler(IInventoryQuerier inventoryQuerier)
  {
    _inventoryQuerier = inventoryQuerier;
  }

  public async Task<InventoryItemDto?> HandleAsync(ReadInventoryItemQuery query, CancellationToken cancellationToken)
  {
    return await _inventoryQuerier.ReadAsync(query.TrainerId, query.ItemId, cancellationToken);
  }
}
