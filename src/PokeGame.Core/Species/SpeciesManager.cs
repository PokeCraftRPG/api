using PokeGame.Core.Regions;
using PokeGame.Core.Species.Models;

namespace PokeGame.Core.Species;

public interface ISpeciesManager
{
  Task<IReadOnlyDictionary<RegionId, Number?>> FindRegionalNumbersAsync(
    IEnumerable<RegionalNumberPayload> payloads,
    string propertyName,
    CancellationToken cancellationToken = default);
}

internal class SpeciesManager : ISpeciesManager
{
  private readonly IContext _context;
  private readonly IRegionQuerier _regionQuerier;

  public SpeciesManager(IContext context, IRegionQuerier regionQuerier)
  {
    _context = context;
    _regionQuerier = regionQuerier;
  }

  public async Task<IReadOnlyDictionary<RegionId, Number?>> FindRegionalNumbersAsync(
    IEnumerable<RegionalNumberPayload> payloads,
    string propertyName,
    CancellationToken cancellationToken)
  {
    int capacity = payloads.Count();
    Dictionary<RegionId, Number?> regionalNumbers = new(capacity);

    if (capacity > 0)
    {
      IReadOnlyCollection<RegionKey> allKeys = await _regionQuerier.ListKeysAsync(cancellationToken);
      Dictionary<Guid, RegionId> ids = new(capacity: allKeys.Count);
      Dictionary<string, RegionId> keys = new(capacity: allKeys.Count);
      foreach (RegionKey key in allKeys)
      {
        ids[key.Id] = key.RegionId;
        keys[key.Key] = key.RegionId;
      }

      List<string> missing = new(capacity);
      foreach (RegionalNumberPayload payload in payloads)
      {
        string region = Slug.Normalize(payload.Region);
        Number? number = payload.Number == 0 ? null : new(payload.Number);
        if ((Guid.TryParse(region, out Guid id) && ids.TryGetValue(id, out RegionId regionId)) || keys.TryGetValue(region, out regionId))
        {
          regionalNumbers[regionId] = number;
        }
        else
        {
          missing.Add(payload.Region);
        }
      }

      if (missing.Count > 0)
      {
        throw new RegionsNotFoundException(_context.WorldId, missing, propertyName);
      }
    }

    return regionalNumbers.AsReadOnly();
  }
}
