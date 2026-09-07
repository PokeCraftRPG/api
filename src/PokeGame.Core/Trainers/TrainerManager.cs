using Logitar.EventSourcing;
using PokeGame.Core.Trainers.Events;

namespace PokeGame.Core.Trainers;

public interface ITrainerManager
{
  Task EnsureUnicityAsync(Trainer trainer, CancellationToken cancellationToken = default);
}

internal class TrainerManager : ITrainerManager
{
  private readonly ITrainerQuerier _trainerQuerier;

  public TrainerManager(ITrainerQuerier trainerQuerier)
  {
    _trainerQuerier = trainerQuerier;
  }

  public async Task EnsureUnicityAsync(Trainer trainer, CancellationToken cancellationToken)
  {
    Key? key = null;
    License? license = null;
    foreach (IEvent change in trainer.Changes)
    {
      if (change is TrainerCreated created)
      {
        key = created.Key;
      }
      else if (change is TrainerKeyChanged changed)
      {
        key = changed.Key;
      }
      else if (change is TrainerLicenseChanged licenseChanged)
      {
        license = licenseChanged.License;
      }
    }

    if (key is not null)
    {
      TrainerId? trainerId = await _trainerQuerier.GetIdAsync(key, cancellationToken);
      if (trainerId.HasValue && !trainerId.Value.Equals(trainer.Id))
      {
        throw new KeyAlreadyUsedException(trainer, trainerId.Value.EntityId, trainer.Key, nameof(trainer.Key));
      }
    }

    if (license is not null)
    {
      TrainerId? trainerId = await _trainerQuerier.GetIdAsync(license, cancellationToken);
      if (trainerId.HasValue && !trainerId.Value.Equals(trainer.Id))
      {
        throw new LicenseAlreadyUsedException(trainer, trainerId.Value);
      }
    }
  }
}
