using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions;

public sealed class RegionsNotFoundException : NotFoundException
{
  public RegionsNotFoundException(WorldId worldId, IEnumerable<Guid> regionIds, string propertyName) : base("The specified regions were not found.")
  {
    Data["WorldId"] = worldId.EntityId;
    Data["RegionIds"] = regionIds.Distinct().OrderBy(id => id).ToList().AsReadOnly();
    Data["PropertyName"] = propertyName;
  }
}
