using Logitar.CQRS;
using PokeGame.Core.Inventory.Models;

namespace PokeGame.Core.Inventory.Queries;

internal record ReadInventoryQuery(Guid TrainerId, Guid ItemId) : IQuery<InventoryItemModel?>;

internal class ReadInventoryQueryHandler : IQueryHandler<ReadInventoryQuery, InventoryItemModel?>
{
  private readonly IInventoryQuerier _inventoryQuerier;

  public ReadInventoryQueryHandler(IInventoryQuerier inventoryQuerier)
  {
    _inventoryQuerier = inventoryQuerier;
  }

  public async Task<InventoryItemModel?> HandleAsync(ReadInventoryQuery query, CancellationToken cancellationToken)
  {
    return await _inventoryQuerier.ReadAsync(query.TrainerId, query.ItemId, cancellationToken);
  }
}
