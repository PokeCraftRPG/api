using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Queries;

internal record ReadRegionQuery(Guid? Id, string? Key) : IQuery<RegionDto?>;

internal class ReadRegionQueryHandler : IQueryHandler<ReadRegionQuery, RegionDto?>
{
  private readonly IRegionQuerier _regionQuerier;

  public ReadRegionQueryHandler(IRegionQuerier regionQuerier)
  {
    _regionQuerier = regionQuerier;
  }

  public async Task<RegionDto?> HandleAsync(ReadRegionQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, RegionDto> regions = new(capacity: 2);

    if (query.Id.HasValue)
    {
      RegionDto? region = await _regionQuerier.ReadAsync(query.Id.Value, cancellationToken);
      if (region is not null)
      {
        regions[region.Id] = region;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      RegionDto? region = await _regionQuerier.ReadAsync(query.Key, cancellationToken);
      if (region is not null)
      {
        regions[region.Id] = region;
      }
    }

    if (regions.Count > 1)
    {
      throw TooManyResultsException<RegionDto>.ExpectedSingle(regions.Count);
    }

    return regions.Values.SingleOrDefault();
  }
}
