using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Trainers;

public sealed class LicenseAlreadyUsedException : ConflictException
{
  private const string ErrorMessage = "The specified license is already used.";

  public Guid WorldId
  {
    get => (Guid)Data[nameof(WorldId)]!;
    private set => Data[nameof(WorldId)] = value;
  }
  public Guid TrainerId
  {
    get => (Guid)Data[nameof(TrainerId)]!;
    private set => Data[nameof(TrainerId)] = value;
  }
  public Guid ConflictId
  {
    get => (Guid)Data[nameof(ConflictId)]!;
    private set => Data[nameof(ConflictId)] = value;
  }
  public string AttemptedLicense
  {
    get => (string)Data[nameof(AttemptedLicense)]!;
    private set => Data[nameof(AttemptedLicense)] = value;
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
      error.Data[nameof(TrainerId)] = TrainerId;
      error.Data[nameof(ConflictId)] = ConflictId;
      error.Data[nameof(AttemptedLicense)] = AttemptedLicense;
      error.Data[nameof(PropertyName)] = PropertyName;
      return error;
    }
  }

  public LicenseAlreadyUsedException(Trainer trainer, TrainerId conflictId)
    : base(BuildMessage(trainer, conflictId))
  {
    WorldId = trainer.WorldId.EntityId;
    TrainerId = trainer.EntityId;
    ConflictId = conflictId.EntityId;
    AttemptedLicense = trainer.License?.Value ?? throw new ArgumentException("A license is required.", nameof(trainer));
    PropertyName = nameof(trainer.License);
  }

  private static string BuildMessage(Trainer trainer, TrainerId conflictId) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(WorldId), trainer.WorldId.EntityId)
    .AddData(nameof(TrainerId), trainer.EntityId)
    .AddData(nameof(ConflictId), conflictId.EntityId)
    .AddData(nameof(AttemptedLicense), trainer.License)
    .AddData(nameof(PropertyName), nameof(Trainer.License))
    .Build();
}
