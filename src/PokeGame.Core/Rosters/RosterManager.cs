using PokeGame.Core.Trainers;

namespace PokeGame.Core.Rosters;

public interface IRosterManager
{
  Task<Roster> FindAsync(TrainerId trainerId, string propertyName, CancellationToken cancellationToken = default);
}

internal class RosterManager : IRosterManager
{
  private readonly IRosterRepository _rosterRepository;
  private readonly ITrainerRepository _trainerRepository;

  public RosterManager(IRosterRepository rosterRepository, ITrainerRepository trainerRepository)
  {
    _rosterRepository = rosterRepository;
    _trainerRepository = trainerRepository;
  }

  public async Task<Roster> FindAsync(TrainerId trainerId, string propertyName, CancellationToken cancellationToken)
  {
    RosterId rosterId = new(trainerId);
    Roster? roster = await _rosterRepository.LoadAsync(rosterId, cancellationToken);
    if (roster is null)
    {
      Trainer trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken) ?? throw new EntityNotFoundException(trainerId, propertyName);
      roster = new Roster(trainer);
    }
    return roster;
  }
}
