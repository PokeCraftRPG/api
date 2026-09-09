using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Items.Models;

namespace PokeGame.Core.Items.Queries;

internal record SearchItemsQuery(SearchItemsPayload Payload) : IQuery<SearchResults<ItemDto>>;

internal class SearchItemsQueryHandler : IQueryHandler<SearchItemsQuery, SearchResults<ItemDto>>
{
  private readonly IItemQuerier _itemQuerier;

  public SearchItemsQueryHandler(IItemQuerier itemQuerier)
  {
    _itemQuerier = itemQuerier;
  }

  public async Task<SearchResults<ItemDto>> HandleAsync(SearchItemsQuery query, CancellationToken cancellationToken)
  {
    SearchItemsPayload payload = query.Payload;
    payload.Validate();

    return await _itemQuerier.SearchAsync(payload, cancellationToken);
  }
}
