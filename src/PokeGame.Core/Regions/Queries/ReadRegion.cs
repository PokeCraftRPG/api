using Logitar.CQRS;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Queries;

internal record ReadRegionQuery(Guid Id) : IQuery<RegionModel?>;

internal class ReadRegionQueryHandler : IQueryHandler<ReadRegionQuery, RegionModel?>
{
  private readonly IRegionQuerier _regionQuerier;

  public ReadRegionQueryHandler(IRegionQuerier regionQuerier)
  {
    _regionQuerier = regionQuerier;
  }

  public async Task<RegionModel?> HandleAsync(ReadRegionQuery query, CancellationToken cancellationToken)
  {
    return await _regionQuerier.ReadAsync(query.Id, cancellationToken);
  }
}
