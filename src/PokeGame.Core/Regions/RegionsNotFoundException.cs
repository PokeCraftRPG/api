using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Regions;

public class RegionsNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified regions were not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public IReadOnlyCollection<string> Regions
  {
    get => (IReadOnlyCollection<string>)Data[nameof(Regions)]!;
    private set => Data[nameof(Regions)] = value;
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
      error.Data[nameof(Regions)] = Regions;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public RegionsNotFoundException(WorldId worldId, IEnumerable<string> regions, string propertyName)
    : base(BuildMessage(worldId, regions, propertyName))
  {
    WorldId = worldId.ToGuid();
    Regions = regions.Distinct().ToList().AsReadOnly();
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, IEnumerable<string> regions, string propertyName)
  {
    StringBuilder message = new();
    message.AppendLine(ErrorMessage);
    message.Append(nameof(WorldId)).Append(": ").Append(worldId.ToGuid()).AppendLine();
    message.Append(nameof(PropertyName)).Append(": ").AppendLine(propertyName);
    if (regions.Any())
    {
      message.Append(nameof(Regions)).Append(':').AppendLine();
      regions = regions.Distinct();
      foreach (string region in regions)
      {
        message.Append(" - ").AppendLine(region);
      }
    }
    return message.ToString();
  }
}
