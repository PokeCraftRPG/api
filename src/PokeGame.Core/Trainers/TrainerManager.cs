using PokeGame.Core.Worlds;

namespace PokeGame.Core.Trainers;

public interface ITrainerManager
{
  Task<Trainer> FindAsync(string trainer, string propertyName, CancellationToken cancellationToken = default);
}

internal class TrainerManager : ITrainerManager
{
  private readonly IContext _context;
  private readonly ITrainerQuerier _trainerQuerier;
  private readonly ITrainerRepository _trainerRepository;

  public TrainerManager(IContext context, ITrainerQuerier trainerQuerier, ITrainerRepository trainerRepository)
  {
    _context = context;
    _trainerQuerier = trainerQuerier;
    _trainerRepository = trainerRepository;
  }

  public async Task<Trainer> FindAsync(string idOrKey, string propertyName, CancellationToken cancellationToken)
  {
    WorldId worldId = _context.WorldId;

    if (Guid.TryParse(idOrKey, out Guid id))
    {
      TrainerId trainerId = new(worldId, id);
      Trainer? trainer = await _trainerRepository.LoadAsync(trainerId, cancellationToken);
      if (trainer is not null)
      {
        return trainer;
      }
    }

    TrainerId? foundId = await _trainerQuerier.FindIdAsync(idOrKey, cancellationToken);
    if (!foundId.HasValue)
    {
      throw new TrainerNotFoundException(worldId, idOrKey, propertyName);
    }

    return await _trainerRepository.LoadAsync(foundId.Value, cancellationToken)
      ?? throw new InvalidOperationException($"The trainer 'Id={foundId}' was not loaded.");
  }
}
