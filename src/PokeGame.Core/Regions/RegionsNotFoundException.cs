using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions;

public sealed class RegionsNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified regions were not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public IReadOnlyCollection<Guid> RegionIds
  {
    get => (IReadOnlyCollection<Guid>)Data[nameof(RegionIds)]!;
    private set => Data[nameof(RegionIds)] = value;
  }
  public string PropertyName
  {
    get => (string)Data[nameof(PropertyName)]!;
    private set => Data[nameof(PropertyName)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(WorldId)] = WorldId;
      error.Data[nameof(RegionIds)] = RegionIds;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public RegionsNotFoundException(WorldId worldId, IEnumerable<Guid> regionIds, string propertyName)
    : base(BuildMessage(worldId, regionIds, propertyName))
  {
    WorldId = worldId.EntityId;
    RegionIds = regionIds.Distinct().OrderBy(id => id).ToList().AsReadOnly();
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, IEnumerable<Guid> regionIds, string propertyName)
  {
    StringBuilder message = new();
    message.AppendLine(ErrorMessage);
    message.Append(nameof(WorldId)).Append(": ").Append(worldId.EntityId).AppendLine();

    message.Append(nameof(RegionIds)).Append(':');
    if (regionIds.Any())
    {
      IEnumerable<Guid> sanitizedIds = regionIds.Distinct().OrderBy(id => id);
      message.AppendLine();
      foreach (Guid regionId in sanitizedIds)
      {
        message.Append(" - ").Append(regionId).AppendLine();
      }
    }
    else
    {
      message.AppendLine(" []");
    }

    message.Append(nameof(PropertyName)).Append(": ").AppendLine(propertyName);
    return message.ToString();
  }
}
