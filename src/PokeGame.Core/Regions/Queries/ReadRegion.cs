using Krakenar.Contracts;
using Logitar.CQRS;
using PokeGame.Core.Regions.Models;

namespace PokeGame.Core.Regions.Queries;

internal record ReadRegionQuery(Guid? Id, string? Key) : IQuery<RegionModel?>;

internal class ReadRegionQueryHandler : IQueryHandler<ReadRegionQuery, RegionModel?>
{
  private readonly IRegionRepository _regionRepository;

  public ReadRegionQueryHandler(IRegionRepository regionRepository)
  {
    _regionRepository = regionRepository;
  }

  public async Task<RegionModel?> HandleAsync(ReadRegionQuery query, CancellationToken cancellationToken)
  {
    Dictionary<Guid, RegionModel> regions = new(capacity: 2);

    if (query.Id.HasValue)
    {
      RegionModel? region = await _regionRepository.ReadAsync(query.Id.Value, cancellationToken);
      if (region is not null)
      {
        regions[region.Id] = region;
      }
    }

    if (!string.IsNullOrWhiteSpace(query.Key))
    {
      RegionModel? region = await _regionRepository.ReadAsync(query.Key, cancellationToken);
      if (region is not null)
      {
        regions[region.Id] = region;
      }
    }

    if (regions.Count > 1)
    {
      throw TooManyResultsException<RegionModel>.ExpectedSingle(regions.Count);
    }

    return regions.SingleOrDefault().Value;
  }
}
