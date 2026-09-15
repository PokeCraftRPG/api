using Logitar.CQRS;
using PokeGame.Core.Rosters.Models;

namespace PokeGame.Core.Rosters.Queries;

internal record ReadTagQuery(Guid TrainerId, Guid TagId) : IQuery<TagDto?>;

internal class ReadTagQueryHandler : IQueryHandler<ReadTagQuery, TagDto?>
{
  private readonly IRosterQuerier _rosterQuerier;

  public ReadTagQueryHandler(IRosterQuerier rosterQuerier)
  {
    _rosterQuerier = rosterQuerier;
  }

  public async Task<TagDto?> HandleAsync(ReadTagQuery query, CancellationToken cancellationToken)
  {
    return await _rosterQuerier.ReadTagAsync(query.TrainerId, query.TagId, cancellationToken);
  }
}
