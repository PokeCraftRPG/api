using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species;

internal static class RegionalNumberResolver
{
  public static async Task<IReadOnlyDictionary<Region, int>> ResolveAsync(
    IRegionRepository regionRepository,
    IEnumerable<RegionalNumberPayload> payloads,
    string propertyName,
    CancellationToken cancellationToken)
  {
    int capacity = payloads.Count();
    Dictionary<Region, int> regionalNumbers = new(capacity);

    if (capacity > 0)
    {
      IReadOnlyCollection<Region> regions = await regionRepository.LoadAsync(cancellationToken);
      Dictionary<Guid, Region> regionByIds = new(capacity: regions.Count);
      Dictionary<string, Region> regionByKeys = new(capacity: regions.Count);
      foreach (Region region in regions)
      {
        regionByIds[region.Id] = region;
        regionByKeys[region.Key] = region;
      }

      HashSet<string> missingRegions = new(capacity);
      foreach (RegionalNumberPayload payload in payloads)
      {
        if ((Guid.TryParse(payload.Region, out Guid regionId) && regionByIds.TryGetValue(regionId, out Region? region))
          || regionByKeys.TryGetValue(SlugHelper.Format(payload.Region), out region))
        {
          regionalNumbers[region] = payload.Number;
        }
        else
        {
          missingRegions.Add(payload.Region);
        }
      }

      if (missingRegions.Count > 0)
      {
        throw new RegionsNotFoundException(missingRegions, propertyName);
      }
    }

    return regionalNumbers.AsReadOnly();
  }
}
