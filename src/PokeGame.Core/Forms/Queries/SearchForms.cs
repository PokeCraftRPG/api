using Krakenar.Contracts.Search;
using Logitar.CQRS;
using PokeGame.Core.Forms.Models;

namespace PokeGame.Core.Forms.Queries;

internal record SearchFormsQuery(SearchFormsPayload Payload) : IQuery<SearchResults<FormModel>>;

internal class SearchFormsQueryHandler : IQueryHandler<SearchFormsQuery, SearchResults<FormModel>>
{
  private readonly IFormQuerier _formQuerier;

  public SearchFormsQueryHandler(IFormQuerier formQuerier)
  {
    _formQuerier = formQuerier;
  }

  public async Task<SearchResults<FormModel>> HandleAsync(SearchFormsQuery query, CancellationToken cancellationToken)
  {
    return await _formQuerier.SearchAsync(query.Payload, cancellationToken);
  }
}
