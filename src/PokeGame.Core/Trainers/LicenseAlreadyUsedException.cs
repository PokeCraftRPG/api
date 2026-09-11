namespace PokeGame.Core.Trainers;

public sealed class LicenseAlreadyUsedException : ConflictException
{
  public LicenseAlreadyUsedException(Trainer trainer, TrainerId conflictId)
    : base("The specified trainer license is already used.")
  {
    Data["WorldId"] = trainer.WorldId.EntityId;
    Data["TrainerId"] = trainer.EntityId;
    Data["ConflictId"] = conflictId.EntityId;
    Data["AttemptedLicense"] = trainer.License?.Value;
    Data["PropertyName"] = nameof(Trainer.License);
  }
}
