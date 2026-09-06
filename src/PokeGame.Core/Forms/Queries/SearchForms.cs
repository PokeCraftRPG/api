using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms.Queries;

internal record SearchFormsQuery(SearchFormsPayload Payload) : IQuery<SearchResults<FormDto>>;

internal class SearchFormsQueryHandler : IQueryHandler<SearchFormsQuery, SearchResults<FormDto>>
{
  private readonly IFormQuerier _formQuerier;

  public SearchFormsQueryHandler(IFormQuerier formQuerier)
  {
    _formQuerier = formQuerier;
  }

  public async Task<SearchResults<FormDto>> HandleAsync(SearchFormsQuery query, CancellationToken cancellationToken)
  {
    SearchFormsPayload payload = query.Payload;
    payload.Validate();

    return await _formQuerier.SearchAsync(payload, cancellationToken);
  }
}
