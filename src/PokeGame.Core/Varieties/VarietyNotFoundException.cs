using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Varieties;

public class VarietyNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified variety was not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public string Variety
  {
    get => (string)Data[nameof(Variety)]!;
    private set => Data[nameof(Variety)] = value;
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
      error.Data[nameof(Variety)] = Variety;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public VarietyNotFoundException(WorldId worldId, string variety, string propertyName)
    : base(BuildMessage(worldId, variety, propertyName))
  {
    WorldId = worldId.ToGuid();
    Variety = variety;
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, string variety, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), worldId.ToGuid())
    .AddData(nameof(Variety), variety)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
