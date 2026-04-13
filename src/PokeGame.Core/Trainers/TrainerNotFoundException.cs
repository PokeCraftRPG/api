using Krakenar.Contracts;
using Logitar;
using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

public class TrainerNotFoundException : NotFoundException
{
  private const string ErrorMessage = "The specified trainer was not found.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public string Trainer
  {
    get => (string)Data[nameof(Trainer)]!;
    private set => Data[nameof(Trainer)] = value;
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
      error.Data[nameof(Trainer)] = Trainer;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public TrainerNotFoundException(WorldId worldId, string trainer, string propertyName)
    : base(BuildMessage(worldId, trainer, propertyName))
  {
    WorldId = worldId.ToGuid();
    Trainer = trainer;
    PropertyName = propertyName;
  }

  private static string BuildMessage(WorldId worldId, string trainer, string propertyName) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), worldId.ToGuid())
    .AddData(nameof(Trainer), trainer)
    .AddData(nameof(PropertyName), propertyName)
    .Build();
}
