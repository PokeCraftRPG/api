using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Species;

public class SpeciesNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified species was not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public string Species
  {
    get => (string)Data[nameof(Species)]!;
    private set => Data[nameof(Species)] = value;
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
      error.Data[nameof(Species)] = Species;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public SpeciesNotFoundException(WorldId worldId, string species, string propertyName)
    : base(BuildMessage(worldId, species, propertyName))
  {
    WorldId = worldId.ToGuid();
    Species = species;
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, string species, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), worldId.ToGuid())
    .AddData(nameof(Species), species)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
