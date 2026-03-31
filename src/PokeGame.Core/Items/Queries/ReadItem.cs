using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Items.Models;

namespace PokeGame.Core.Items.Queries;

internal record ReadItemQuery(Guid? Id, string? Key) : IQuery<ItemModel?>;

internal class ReadItemQueryHandler : IQueryHandler<ReadItemQuery, ItemModel?>
{
  private readonly IItemQuerier _itemQuerier;

  public ReadItemQueryHandler(IItemQuerier itemQuerier)
  {
    _itemQuerier = itemQuerier;
  }

  public async Task<ItemModel?> HandleAsync(ReadItemQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, ItemModel> items = new(capacity: 2);

    if (query.Id.HasValue)
    {
      ItemModel? item = await _itemQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (item is not null)
      {
        items[item.Id] = item;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      ItemModel? item = await _itemQuerier.ReadAsync(query.Key, cancellationToken);
      if (item is not null)
      {
        items[item.Id] = item;
      }
    }

    if (items.Count > 1)
    {
      throw TooManyResultsException<ItemModel>.ExpectedSingle(items.Count);
    }

    return items.Values.SingleOrDefault();
  }
}
